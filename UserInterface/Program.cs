using Algorithms;
using Algorithms.GraphFactory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace UserInterface
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Performance tests:");
            var stopwatch = new Stopwatch();
            var random = new Random(1);
            for (int n = 60; n < 1000; n++)
            {
                Console.WriteLine("NEW SIZE");
                Console.WriteLine();
                for (double density = 0.05d; density < 1; density += 0.05d)
                {
                    var graph = GraphFactory.GenerateRandom(n, density, random.Next());
                    var solution = new Dictionary<int, int>();

                    Console.WriteLine($"Size {n}, density {density:f2}");

                    stopwatch.Restart();
                    solution = new ExactClassicAlgorithm().ColourGraph(graph);
                    stopwatch.Stop();
                    Console.WriteLine($"Graph was classically coloured by using {solution.Values.Max() + 1} colours in {stopwatch.Elapsed.TotalMilliseconds:f3}ms");

                    var newGraph = graph.CloneWithoutSmallestVertex(degree => degree < solution.Max(kvp => kvp.Value + 1));
                    stopwatch.Restart();
                    solution = new ExactClassicAlgorithm().ColourGraph(newGraph);
                    stopwatch.Stop();
                    if (solution.Count > 0)
                        Console.WriteLine($"miniG was classically coloured by using {solution.Values.Max() + 1} colours in {stopwatch.Elapsed.TotalMilliseconds:f3}ms size {newGraph.VerticesKVPs.Length}");

                    //stopwatch.Restart();
                    //solution = new ExactAcyclicAlgorithm().ColourGraph(graph);
                    //stopwatch.Stop();
                    //Console.WriteLine($"Graph was acyclically coloured by using {solution.Values.Max() + 1} colours in {stopwatch.Elapsed.TotalMilliseconds:f3}ms"); stopwatch.Restart();

                    //stopwatch.Restart();
                    //solution = new ExactAcyclicAlgorithmDepthLimit().ColourGraphApproximatelyDepth(graph, 3);
                    //stopwatch.Stop();
                    //Console.WriteLine($"Graph was acyclically coloured by using {solution.Values.Max() + 1} colours in {stopwatch.Elapsed.TotalMilliseconds:f3}ms"); stopwatch.Restart();

                    Console.WriteLine();
                }
            }
        }
    }
}
