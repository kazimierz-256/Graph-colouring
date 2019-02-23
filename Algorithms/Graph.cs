using System.Collections.Generic;

namespace Algorithms
{
    // undirected graph without loops
    public class Graph
    {
        public Dictionary<int, HashSet<int>> VerticesKVPs;
        public RestoreOp RemoveVertex(int vertex)
        {
            var neighbourhood = VerticesKVPs[vertex];
            foreach (var neighbour in neighbourhood)
            {
                VerticesKVPs[neighbour].Remove(vertex);
            }
            VerticesKVPs.Remove(vertex);
            return new RestoreOp()
            {
                vertex = vertex,
                neighbourhood = neighbourhood
            };
        }

        public void RestoreVertex(RestoreOp restore)
        {
            VerticesKVPs.Add(restore.vertex, restore.neighbourhood);
            foreach (var neighbour in restore.neighbourhood)
            {
                VerticesKVPs[neighbour].Add(restore.vertex);
            }
        }

        public class RestoreOp
        {
            public int vertex;
            public HashSet<int> neighbourhood;
        }

    }
}