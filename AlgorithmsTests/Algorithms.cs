using Algorithms;
using Algorithms.GraphFactory;
using System;
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
            var exactAlgorithm = new ExactAlgorithm();
            var solution = exactAlgorithm.ColourGraph(graph);
            Assert.Equal(graph.VerticesKVPs.Keys.Count, solution.Keys.Count);
            var min = solution.Values.Min();
            var max = solution.Values.Max();
            Assert.Equal(0, min);
            Assert.Equal(3, 1 + max - min);
        }
    }
}
