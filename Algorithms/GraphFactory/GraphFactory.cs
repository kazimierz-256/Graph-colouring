using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Algorithms.GraphFactory
{
    public static class GraphFactory
    {
        public static void Connect(Dictionary<int, HashSet<int>> edges, int i, int j)
        {
            if (!edges.ContainsKey(i))
                edges.Add(i, new HashSet<int>());
            if (!edges.ContainsKey(j))
                edges.Add(j, new HashSet<int>());

            edges[i].Add(j);
            edges[j].Add(i);
        }

        public static Graph GeneratePetersenGraph()
        {
            var vertices = new HashSet<int>(Enumerable.Range(0, 10));
            var edges = new Dictionary<int, HashSet<int>>();
            for (int i = 0; i < 5; i += 1)
            {
                Connect(edges, i, (i + 1) % 5);
                Connect(edges, i, i + 5);
            }

            Connect(edges, 5, 7);
            Connect(edges, 5, 8);
            Connect(edges, 6, 8);
            Connect(edges, 6, 9);
            Connect(edges, 7, 9);

            return new Graph
            {
                VerticesKVPs = edges
            };
        }
    }
}
