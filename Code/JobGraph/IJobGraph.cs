using System;
using System.Collections.Generic;
using Sandbox;

namespace MANIFOLD.Jobs {
    public interface IJobGraph : IEnumerable<IJob>, IValid {
        public void Run();
        
        /// <summary>
        /// Adds a job to the graph.
        /// Please use <see cref="JobManagement.SetGraph"/>
        /// </summary>
        /// <param name="job">Job to add.</param>
        public void AddJob(IJob job);
        /// <summary>
        /// Removes a job from the graph.
        /// Please use <see cref="JobManagement.SetGraph"/>
        /// </summary>
        /// <param name="job">Job to remove.</param>
        public void RemoveJob(IJob job);
        public IJob GetJob(Guid id);
    }

    public interface IOrderedJobGraph : IJobGraph {
        
    }
}
