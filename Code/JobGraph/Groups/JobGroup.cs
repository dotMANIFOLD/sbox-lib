using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MANIFOLD.Jobs {
    public class JobGroup : IJobGraph, IJob {
        private Dictionary<Guid, IJob> allJobs = new Dictionary<Guid, IJob>();

        public Guid ID { get; } = Guid.NewGuid();
        public IJobGraph Graph { get; set; }
        
        public bool IsValid => allJobs.Count > 0;

        public virtual void Run() {
            foreach (var job in allJobs.Values) {
                job.Run();
            }
        }
        
        public virtual void AddJob(IJob job) {
            allJobs.Add(job.ID, job);
        }

        public virtual void RemoveJob(IJob job) {
            allJobs.Remove(job.ID);
        }

        public IJob GetJob(Guid id) {
            return allJobs.GetValueOrDefault(id);
        }
        
        // ENUMERATOR
        public virtual IEnumerator<IJob> GetEnumerator() {
            return allJobs.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
