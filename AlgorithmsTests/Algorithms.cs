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
        public void PetersenGraph() => TestGraph(GraphFactory.GeneratePetersenGraph(), 3, null);

        [Fact]
        public void FiveRegularClebschGraph() => TestGraph(GraphFactory.Generate5RegularClebschGraph(), 4, null);

        private void TestGraph(Graph graph, int? expectedChromaticNumber, int? expectedAcyclicNumber)
        {
            // classical tests
            var solution = new ExactClassicAlgorithm().ColourGraph(graph);
            DetailedClassicVerification(graph, solution, expectedChromaticNumber);

            // cyclicity tests
            var acyclicSolution = new ExactAcyclicAlgorithm().ColourGraph(graph);
            DetailedAcyclicVerification(graph, acyclicSolution, null, expectedAcyclicNumber);
        }

        private void DetailedClassicVerification(Graph graph, Dictionary<int, int> solution, int? expectedChromaticNumber)
        {
            Assert.Equal(graph.VerticesKVPs.Keys.Count, solution.Keys.Count);
            var min = solution.Values.Min();
            var max = solution.Values.Max();
            Assert.Equal(0, min);
            if (expectedChromaticNumber.HasValue)
                Assert.Equal(expectedChromaticNumber.Value, 1 + max - min);
            VerifyClassicSolution(graph, solution);
        }

        private void DetailedAcyclicVerification(Graph graph, Dictionary<int, int> solution, Dictionary<int, int> classicSolution = null, int? expectedAcyclicNumber = null)
        {
            Assert.Equal(graph.VerticesKVPs.Keys.Count, solution.Keys.Count);
            var min = solution.Values.Min();
            var max = solution.Values.Max();
            Assert.Equal(0, min);
            if (expectedAcyclicNumber.HasValue)
                Assert.Equal(expectedAcyclicNumber.Value, 1 + max - min);
            if (classicSolution != null)
                Assert.True(max >= classicSolution.Values.Max());
            VerifyClassicSolution(graph, solution);
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
