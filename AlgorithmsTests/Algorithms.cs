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
        public void EvenCycle() => TestGraph(GraphFactory.GenerateCycle(4), 2, 3);

        [Fact]
        public void FiveRegularClebschGraph() => TestGraph(GraphFactory.Generate5RegularClebschGraph(), 4, null);

        [Fact]
        public void GruenbaumsGraph() => TestGraph(GraphFactory.GenerateGruenbaumsGraph(), 3, 5);

        private void TestGraph(Graph graph, int? expectedChromaticNumber, int? expectedAcyclicNumber)
        {
            // classical tests
            var solution = new ExactClassicAlgorithmNoDelLazyCuttingOptimized().ColourGraph(graph);
            DetailedClassicVerification(graph, solution, expectedChromaticNumber);

            // cyclicity tests
            var acyclicSolution = new ExactAcyclicAlgorithmLazyCuttingOptimized().ColourGraph(graph);
            DetailedAcyclicVerification(graph, acyclicSolution, null, expectedAcyclicNumber);
        }

        private void DetailedClassicVerification(Graph graph, Dictionary<int, int> solution, int? expectedChromaticNumber)
        {
            Assert.Equal(graph.VerticesKVPs.Length, solution.Keys.Count);
            var min = solution.Values.Min();
            var max = solution.Values.Max();
            Assert.Equal(0, min);
            if (expectedChromaticNumber.HasValue)
                Assert.Equal(expectedChromaticNumber.Value, 1 + max - min);
            VerifyClassicSolution(graph, solution);
        }

        private void DetailedAcyclicVerification(Graph graph, Dictionary<int, int> solution, Dictionary<int, int> classicSolution = null, int? expectedAcyclicNumber = null)
        {
            Assert.Equal(graph.VerticesKVPs.Length, solution.Keys.Count);
            var min = solution.Values.Min();
            var max = solution.Values.Max();
            Assert.Equal(0, min);
            if (expectedAcyclicNumber.HasValue)
                Assert.Equal(expectedAcyclicNumber.Value, 1 + max - min);
            if (classicSolution != null)
                Assert.True(max >= classicSolution.Values.Max());
            VerifyClassicSolution(graph, solution);
            VerifyAcyclicSolution(graph, solution);
        }

        private void VerifyClassicSolution(Graph graph, Dictionary<int, int> solution)
        {
            for (int i = 0; i < graph.VerticesKVPs.Length; i++)
            {
                foreach (var neighbour in graph.VerticesKVPs[i])
                {
                    Assert.NotEqual(solution[i], solution[neighbour]);
                }
            }
        }

        private void VerifyAcyclicSolution(Graph graph, Dictionary<int, int> solution)
        {
            foreach (var beginningVertex in solution.Keys)
            {
                var beginningColour = solution[beginningVertex];
                foreach (var neighbouringVertex in graph.VerticesKVPs[beginningVertex])
                {
                    var secondColour = solution[neighbouringVertex];
                    foreach (var neighbour2 in graph.VerticesKVPs[neighbouringVertex])
                    {
                        if (neighbour2 != beginningVertex && solution[neighbour2] == beginningColour)
                            Assert.True(DFS(graph, solution, beginningVertex, neighbour2, beginningColour, secondColour, new HashSet<int>() { neighbouringVertex, neighbour2 }));
                    }
                }
            }
        }

        private bool DFS(Graph graph, Dictionary<int, int> solution, int beginningVertex, int consideringVertex, int beginningColour, int secondColour, HashSet<int> hashSet)
        {
            foreach (var neighbour in graph.VerticesKVPs[consideringVertex])
            {
                if (!hashSet.Contains(neighbour))
                {
                    hashSet.Add(neighbour);
                    if (hashSet.Count % 2 == 1 && solution[neighbour] == secondColour)
                    {
                        if (!DFS(graph, solution, beginningVertex, neighbour, beginningColour, secondColour, hashSet))
                            return false;
                    }
                    else if (hashSet.Count % 2 == 0 && solution[neighbour] == beginningColour)
                    {
                        if (neighbour == beginningVertex)
                            return false;

                        if (!DFS(graph, solution, beginningVertex, neighbour, beginningColour, secondColour, hashSet))
                            return false;
                    }
                    hashSet.Remove(neighbour);
                }
            }
            return true;
        }
    }
}
