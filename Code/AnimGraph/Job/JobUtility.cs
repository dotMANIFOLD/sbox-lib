using MANIFOLD.Jobs;

namespace MANIFOLD.AnimGraph {
    public static class JobUtility {
        public static void BindAnimData(this IJobGraph jobGraph, JobBindData bindData) {
            foreach (var job in jobGraph.GetAllJobs()) {
                if (job is IBaseAnimJob casted) {
                    casted.BindData = bindData;
                    casted.Bind();
                }
            }
        }
        
        public static void SetAnimContext(this IJobGraph jobGraph, JobContext context) {
            foreach (var job in jobGraph.GetAllJobs()) {
                if (job is IBaseAnimJob casted) {
                    casted.Context = context;
                }
            }
        }
    }
}
