using System;
using System.Collections.Generic;
using System.Linq;

namespace MANIFOLD.Jobs {
    public static class JobGraphDebug {
        public class GraphEntry {
            public string name;
            public WeakReference<IJobGraph> graph;
        }
        
        private static List<GraphEntry> graphs;

        public static IReadOnlyList<GraphEntry> Graphs => graphs;
        
        static JobGraphDebug() {
            graphs = new List<GraphEntry>();
        }

        public static void AddToDebug(this IJobGraph graph, string name) {
            if (graph == null) return;
            graphs.Add(new GraphEntry() {
                name = name,
                graph = new WeakReference<IJobGraph>(graph)
            });
        }

        public static void RemoveFromDebug(this IJobGraph graph) {
            if (graph == null) return;
            var entry = graphs.FirstOrDefault(x => x.graph.TryGetTarget(out var target) && target == graph);
            if (entry == null) return;
            graphs.Remove(entry);
        }

        public static void RunChecks() {
            for (int i = 0; i < graphs.Count; i++) {
                if (!graphs[i].graph.TryGetTarget(out _)) {
                    graphs.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}
