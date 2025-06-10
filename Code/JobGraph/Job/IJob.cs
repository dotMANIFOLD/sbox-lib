using System;
using System.Collections.Generic;

namespace MANIFOLD.Jobs {
    /// <summary>
    /// Minimum job data.
    /// </summary>
    public interface IJob {
        public Guid ID { get; }
        public IJobGraph Graph { get; set; }

        public void Run();

        public int GetHashCode() {
            return ID.GetHashCode();
        }
    }
}
