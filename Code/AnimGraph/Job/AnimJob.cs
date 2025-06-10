using System;
using System.Collections.Generic;
using System.Linq;
using MANIFOLD.Jobs;

namespace MANIFOLD.AnimGraph {
    /// <summary>
    /// Default animation job implementation.
    /// </summary>
    public abstract class AnimJob : IAnimJob {
        protected Input<JobResults>[] inputs = new Input<JobResults>[1];
        protected List<Output<JobResults>> outputs = new List<Output<JobResults>>();
        
        public Guid ID { get; init; }
        public IJobGraph Graph { get; set; }
        
        public JobContext Context { get; set; }
        public JobBindData BindData { get; set; }

        public IReadOnlyList<Input<JobResults>> Inputs => inputs;
        IReadOnlyList<IInputSocket> IInputJob.Inputs => inputs;

        public IReadOnlyList<Output<JobResults>> Outputs => outputs;
        IReadOnlyList<IOutputSocket> IOutputJob.Outputs => outputs;
        public JobResults OutputData { get; protected set; }

        object IOutputJob.OutputData => OutputData;

        public AnimJob() : this(Guid.NewGuid()) { }
        
        public AnimJob(Guid id) {
            ID = id;
        }

        public virtual void Bind() {
            
        }
        public virtual void Reset() {
            foreach (var input in Inputs) {
                if (input.Job is IBaseAnimJob animJob) {
                    animJob.Reset();
                }
            }
        }

        public virtual void Prepare() {
            
        }
        
        public abstract void Run();

        // INPUT API
        public void SetInput(int index, IOutputJob<JobResults> job) {
            inputs[index] = new Input<JobResults>(job);
        }

        void IInputJob.SetInput(int index, IOutputJob job) {
            SetInput(index, (IOutputJob<JobResults>)job);
        }
        
        public virtual void DisconnectInputs() {
            for (int i = 0; i < Inputs.Count; i++) {
                this.RemoveInput(i);
            }
        }

        // OUTPUT API
        public void AddOutput(IInputJob<JobResults> job, int targetIndex) {
            outputs.Add(new Output<JobResults>(job, targetIndex));
        }

        void IOutputJob.AddOutput(IInputJob job, int targetIndex) {
            AddOutput((IInputJob<JobResults>)job, targetIndex);
        }
        
        public void RemoveOutput(IInputJob<JobResults> job, int targetIndex) {
            if (targetIndex <= -1) {
                outputs.Remove(outputs.First(o => o.Job == job));
            } else {
                outputs.Remove(outputs.First(o => o.Job == job && o.Index == targetIndex));
            }
        }

        void IOutputJob.RemoveOutput(IInputJob job, int targetIndex) {
            RemoveOutput((IInputJob<JobResults>)job, targetIndex);
        }
        
        public virtual void DisconnectOutputs() {
            while (Outputs.Count > 0) {
                this.RemoveOutput(0);
            }
        }
    }
}
