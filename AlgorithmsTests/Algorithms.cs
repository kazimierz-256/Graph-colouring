using Algorithms;
using Algorithms.GraphFactory;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace AlgorithmsTests
{
    public class Algorithms
    {
        [Fact]
        public void Test1()
        {
            var graph = GraphFactory.GeneratePetersenGraph();
            var solution = new ExactClassicAlgorithm().ColourGraph(graph);

            Assert.Equal(graph.VerticesKVPs.Keys.Count, solution.Keys.Count);
            var min = solution.Values.Min();
            var max = solution.Values.Max();
            Assert.Equal(0, min);
            Assert.Equal(3, 1 + max - min);
            VerifyClassicSolution(graph, solution);

            //cyclicity tests
            var acyclicSolution = new ExactAcyclicAlgorithm().ColourGraph(graph);
            Assert.Equal(graph.VerticesKVPs.Keys.Count, acyclicSolution.Keys.Count);
            Assert.Equal(0, acyclicSolution.Values.Min());
            Assert.True(acyclicSolution.Values.Max() >= max);
            VerifyClassicSolution(graph, acyclicSolution);
        }

        private void VerifyClassicSolution(Graph graph, Dictionary<int, int> solution)
        {
            foreach (var vertexKVP in graph.VerticesKVPs)
            {
                foreach (var neighbour in vertexKVP.Value)
                {
                    Assert.NotEqual(solution[vertexKVP.Key], solution[neighbour]);
                }
            }
        }

        private void VerifyAcyclicSolution(Dictionary<int, int> solution)
        {
            // TODO: implement
        }
    }
}
