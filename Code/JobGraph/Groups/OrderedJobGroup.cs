using System.Collections.Generic;
using System.Linq;

namespace MANIFOLD.Jobs {
    public class OrderedJobGroup : JobGroup {
        private SortedList<int, IJob> order = new SortedList<int, IJob>();

        public override void Run() {
            foreach (var job in order.Values) {
                job.Run();
            }
        }

        public override void AddJob(IJob job) {
            base.AddJob(job);
            order.Add(order.Count > 0 ? order.Keys.Max() + 1 : 0, job);
        }

        public override void RemoveJob(IJob job) {
            base.RemoveJob(job);
            order.RemoveAt(order.IndexOfValue(job));
        }

        public void SetOrder(IJob job, int value) {
            order.RemoveAt(order.IndexOfValue(job));
            order.Add(value, job);
        }

        public override IEnumerator<IJob> GetEnumerator() {
            return order.Values.GetEnumerator();
        }
    }
}
