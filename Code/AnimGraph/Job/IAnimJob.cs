using System;
using System.Collections.Generic;
using System.Linq;
using MANIFOLD.Jobs;

namespace MANIFOLD.AnimGraph {
    public record struct JobResults(Pose Pose, float CyclePosition = 0, bool Finished = false) {
        
    }
    
    public interface IBaseAnimJob : IJob {
        public JobContext Context { get; set; }
        public JobBindData BindData { get; set; }

        public void Bind();
        public void Reset();

        public void Prepare();
    }

    public interface IOutputAnimJob : IBaseAnimJob, IOutputJob<JobResults> {
        
    }

    public interface IInputAnimJob : IBaseAnimJob, IInputJob<JobResults> {
        
    }

    public interface IAnimJob : IOutputAnimJob, IInputAnimJob {
        
    }
}
