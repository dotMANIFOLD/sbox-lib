using System;
using System.Collections.Generic;
using System.Linq;

namespace MANIFOLD.Jobs {
    public static class JobManagement {
        public class JobBranch {
            /// <summary>
            /// In execution order.
            /// </summary>
            public List<IJob> jobs = new();
            public List<JobBranch> entryPoints = new();
            public List<JobBranch> executes = new();
            public int depth = 0;

            public T CreateGraph<T>() where T : IJobGraph, new() {
                T graph = new T();
                foreach (var job in jobs) {
                    job.SetGraph(graph);
                }
                return graph;
            }
        }
        
        public delegate void TraverseCallback<in T>(T job) where T : IJob;
        
        // GRAPH
        public static T SetGraph<T>(this T job, IJobGraph jobGraph, bool patch = true) where T : IJob {
            if (job.Graph == jobGraph) return job;
            if (job.Graph != null) {
                job.Graph.RemoveJob(job);
            }

            job.Graph = jobGraph;
            if (jobGraph != null) {
                jobGraph.AddJob(job);
            }
            return job;
        }

        public static List<IJob> GetAllJobs(this IJobGraph graph) {
            List<IJob> jobs = new List<IJob>();
            GetAllJobs(graph, jobs);
            return jobs;
        }
        
        public static void GetAllJobs(this IJobGraph graph, List<IJob> jobs) {
            foreach (var job in graph) {
                if (job is IJobGraph child) {
                    GetAllJobs(child, jobs);
                } else {
                    jobs.Add(job);
                }
            }
        }
        
        // CONNECTIONS
        public static TJob OutputTo<TJob, TData>(this TJob output, IInputJob<TData> input, int index) where TJob : IOutputJob<TData> {
            if (index < 0 || input.Inputs.Count <= index) {
                throw new IndexOutOfRangeException();
            }

            // SetSameGraph(input, output);
            if (input.Inputs[index] != null) {
                if (ReferenceEquals(input.Inputs[index].Job, output)) return output;
                input.RemoveInput(index);
            }
            
            input.SetInput(index, output);
            output.AddOutput(input, index);
            return output;
        }
        
        public static TJob InputFrom<TJob, TData>(this TJob input, IOutputJob<TData> output, int index) where TJob : IInputJob<TData> {
            if (index < 0 || input.Inputs.Count <= index) {
                throw new IndexOutOfRangeException();
            }
            
            // SetSameGraph(input, output);
            if (input.Inputs[index] != null) {
                if (ReferenceEquals(input.Inputs[index].Job, output)) return input;
                input.RemoveInput(index);
            }
            
            input.SetInput(index, output);
            output.AddOutput(input, index);
            return input;
        }

        public static void RemoveOutput<TOutput>(this IOutputJob<TOutput> job, int index) {
            if (index < 0 || job.Outputs.Count <= index) {
                throw new IndexOutOfRangeException();
            }
            
            var target = job.Outputs[index];
            target.Job.SetInput(index, null);
            job.RemoveOutput(target.Job, index);
        }
        
        public static void RemoveInput<TInput>(this IInputJob<TInput> job, int index) {
            if (index < 0 || job.Inputs.Count <= index) {
                throw new IndexOutOfRangeException();
            }
            
            var target = job.Inputs[index];
            job.SetInput(index, null);
            target.Job.RemoveOutput(job, index);
        }
        
        // TRAVERSAL
        public static void TraverseLeft(this IInputJob job, TraverseCallback<IJob> callback) {
            HashSet<IJob> visited = new HashSet<IJob>();
            TraverseLeftInternal(job, visited, callback);
        }
        
        public static void TraverseLeft<TJob, TInput>(this TInput job, TraverseCallback<TJob> callback) where TJob : IJob where TInput : IInputJob {
            HashSet<IJob> visited = new HashSet<IJob>();
            TraverseLeftInternal(job, visited, callback);
        }

        private static void TraverseLeftInternal<TJob, TInput>(TInput job, HashSet<IJob> visited, TraverseCallback<TJob> callback) where TJob : IJob where TInput : IInputJob {
            foreach (var input in job.Inputs) {
                if (input == null) continue;
                
                visited.Add(input.Job);
                if (input.Job is TJob jobCast) {
                    callback(jobCast);
                }
                if (input.Job is TInput inputCast) {
                    TraverseLeftInternal(inputCast, visited, callback);
                }
            }
        }
        
        // BRANCHES
        public static JobBranch ResolveBranches(this IInputJob job) {
            Dictionary<IJob, JobBranch> branchCache = new Dictionary<IJob, JobBranch>();
            JobBranch initialBranch = new JobBranch();
            initialBranch.jobs.Add(job);
            branchCache.Add(job, initialBranch);
            ResolveBranchesInternal(job, initialBranch, branchCache);
            return initialBranch;
        }

        public static IEnumerable<JobBranch> ResolveBranchesFlat(this IInputJob job) {
            Dictionary<IJob, JobBranch> branchCache = new Dictionary<IJob, JobBranch>();
            JobBranch initialBranch = new JobBranch();
            initialBranch.jobs.Add(job);
            branchCache.Add(job, initialBranch);
            ResolveBranchesInternal(job, initialBranch, branchCache);
            return branchCache.Values.Distinct();
        }

        private static void ResolveBranchesInternal(IInputJob job, JobBranch currentBranch, Dictionary<IJob, JobBranch> branchCache) {
            bool singleBranch = false;
            if (job.Inputs.Count > 1) {
                if (job.Inputs.All(x => x.Job == job.Inputs[0].Job)) {
                    singleBranch = true;
                } else {
                    foreach (var input in job.Inputs) {
                        if (!branchCache.TryGetValue(input.Job, out JobBranch nextBranch)) {
                            nextBranch = new JobBranch();
                            nextBranch.jobs.Insert(0, input.Job);
                            branchCache.Add(input.Job, nextBranch);
                            
                            if (input.Job is IInputJob casted) {
                                ResolveBranchesInternal(casted, nextBranch, branchCache);
                            }
                        }
                        
                        if (!currentBranch.executes.Contains(nextBranch)) {
                            currentBranch.executes.Add(nextBranch);
                            nextBranch.entryPoints.Add(currentBranch);
                        }
                        nextBranch.depth = Math.Max(nextBranch.depth, currentBranch.depth + 1);
                    }
                }
            } else if (job.Inputs.Count == 1) {
                singleBranch = true;
            }

            if (singleBranch) {
                currentBranch.jobs.Insert(0, job.Inputs[0].Job);
                branchCache.Add(job.Inputs[0].Job, currentBranch);
                if (job.Inputs[0].Job is IInputJob casted) {
                    ResolveBranchesInternal(casted, currentBranch, branchCache);
                }
            }
        }
        
        public static void GetAllRoots(this IJob job, List<IJob> list) {
            if (job is not IInputJob casted) {
                list.Add(job);
                return;
            }
            if (casted.Inputs.All(x => x == null)) {
                list.Add(casted);
                return;
            }
            foreach (var input in casted.Inputs) {
                GetAllRoots(input.Job, list);
            }
        }
        
        public static void GetAllTails(this IJob job, List<IJob> list) {
            if (job is not IOutputJob casted) {
                list.Add(job);
                return;
            }
            if (casted.Outputs.Count <= 0) {
                list.Add(casted);
                return;
            }
            foreach (var output in casted.Outputs) {
                GetAllTails(output.Job, list);
            }
        }

        // CHAINS
        public static bool GetChainRight(this IJob job, IJob target, List<IJob> list) {
            Stack<IJob> stack = new Stack<IJob>();
            var result = GetChainRightRecursive(job, target, stack);
            if (result) {
                list.AddRange(stack);
            }
            return result;
        }

        private static bool GetChainRightRecursive(IJob job, IJob target, Stack<IJob> stack) {
            if (job == target) {
                stack.Push(target);
                return true;
            }
            
            if (job is not IOutputJob casted) {
                return false;
            }
            
            stack.Push(job);
            
            foreach (var output in casted.Outputs) {
                var result = GetChainRightRecursive(output.Job, target, stack);
                if (!result) stack.Pop();
                else return true;
            }
            return false;
        }
    }
}
