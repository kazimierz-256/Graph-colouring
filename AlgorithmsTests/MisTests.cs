using Algorithms;
using Algorithms.GraphFactory;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace AlgorithmsTests
{
    public class MIS
    {
        [Fact]
        public void PetersenGraph() => TestGraph(GraphFactory.GeneratePetersenGraph(), 4);

        [Fact]
        public void EvenCycle() => TestGraph(GraphFactory.GenerateCycle(4), 2);

        [Fact]
        public void OddCycle() => TestGraph(GraphFactory.GenerateCycle(5), 2);
        [Fact]
        public void FiveRegularClebschGraph() => TestGraph(GraphFactory.Generate5RegularClebschGraph(), 5);

        [Fact]
        public void GruenbaumsGraph() => TestGraph(GraphFactory.GenerateGruenbaumsGraph());


        private void TestGraph(Graph graph, int? independenceNumber = null)
        {
            var largestMIS = 0;
            foreach (var mis in new ExactMisAlgorithm().EnumerateMIS(graph))
            {
                if ((mis == null || mis.Count == 0) && graph.VerticesKVPs.Length > 0)
                    throw new Exception("Empty MIS!");

                VerifyUniqueness(graph, mis);
                VerifyMaximality(graph, mis);
                VerifyIndependence(graph, mis);

                if (mis.Count > largestMIS)
                    largestMIS = mis.Count;
            }
            if (independenceNumber.HasValue)
            {
                if (largestMIS != independenceNumber.Value)
                    throw new Exception("Could not find the largest independent set");
            }
        }
        private void VerifyUniqueness(Graph graph, List<int> mis)
        {
            // there are no repeating vertices
            var set = new HashSet<int>(mis);
            Assert.Equal(mis.Count, set.Count);
        }

        private void VerifyIndependence(Graph graph, List<int> mis)
        {
            // there are no adjacent vertices in MIS
            var set = new HashSet<int>(mis);
            for (int i = 0; i < mis.Count; i++)
            {
                for (int j = 0; j < graph.VerticesKVPs[i].Length; j++)
                {
                    if (set.Contains(graph.VerticesKVPs[mis[i]][j]))
                        throw new Exception($"Vertices {mis[i]} and {j} are adjacent in MIS in graph {graph.graphName}");
                }
            }
        }

        private void VerifyMaximality(Graph graph, List<int> mis)
        {
            // there is no vertex who has no neighbour in MIS
            var set = new HashSet<int>(mis);
            for (int i = 0; i < graph.VerticesKVPs.Length; i++)
            {
                if (graph.VerticesKVPs[i].Length == 0)
                {
                    if (!set.Contains(i))
                        throw new Exception($"Singleton {i} from graph {graph.graphName} not in MIS!");
                }
                else if (!set.Contains(i))
                {
                    var thereExistsAnAdjacentMIS = false;
                    for (int j = 0; j < graph.VerticesKVPs[i].Length; j++)
                    {
                        if (set.Contains(graph.VerticesKVPs[i][j]))
                        {
                            thereExistsAnAdjacentMIS = true;
                            break;
                        }
                    }

                    if (!thereExistsAnAdjacentMIS)
                    {
                        throw new Exception($"Vertex {i} is not adjacent to any vertex of MIS");
                    }
                }
            }
        }
    }
}