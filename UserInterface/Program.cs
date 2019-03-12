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
            for (int n = 30; n < 1000; n++)
            {
                Console.WriteLine("NEW SIZE");
                Console.WriteLine();
                for (double density = 0.05d; density < 1d; density += 0.05d)
                {
                    var graph = GraphFactory.GenerateRandom(n, density, random.Next());
                    var solution = new Dictionary<int, int>();

                    Console.WriteLine($"Size {n}, density {density:f2}");

                    //stopwatch.Restart();
                    //solution = new ExactClassicAlgorithm().ColourGraph(graph);
                    //stopwatch.Stop();
                    //Console.WriteLine($"Graph was classically coloured by using {solution.Values.Max() + 1} colours in {stopwatch.Elapsed.TotalMilliseconds:f3}ms (without deleting, lazily, cutting, optimized)");

                    //stopwatch.Restart();
                    //solution = new ExactClassicAlgorithm2().ColourGraph(graph);
                    //stopwatch.Stop();
                    //Console.WriteLine($"Graph was classically2 coloured by using {solution.Values.Max() + 1} colours in {stopwatch.Elapsed.TotalMilliseconds:f3}ms (without deleting, lazily, cutting, optimized)");

                    stopwatch.Restart();
                    solution = new ExactAcyclicAlgorithm().ColourGraph(graph);
                    stopwatch.Stop();
                    Console.WriteLine($"Graph was acyclically coloured by using {solution.Values.Max() + 1} colours in {stopwatch.Elapsed.TotalMilliseconds:f3}ms (lazily, cutting, optimized)"); stopwatch.Restart();

                    stopwatch.Restart();
                    solution = new ExactAcyclicAlgorithm2().ColourGraph(graph);
                    stopwatch.Stop();
                    Console.WriteLine($"Graph was acyclically2 coloured by using {solution.Values.Max() + 1} colours in {stopwatch.Elapsed.TotalMilliseconds:f3}ms (lazily, cutting, optimized)"); stopwatch.Restart();

                    //stopwatch.Restart();
                    //solution = new ExactL21Algorithm().ColourGraph(graph);
                    //stopwatch.Stop();
                    //Console.WriteLine($"Graph was L(2,1) coloured by using {solution.Values.Max() + 1} colours in {stopwatch.Elapsed.TotalMilliseconds:f3}ms (lazily, cutting, optimized)");

                    //stopwatch.Restart();
                    //solution = new ExactL21Algorithm2().ColourGraph(graph);
                    //stopwatch.Stop();
                    //Console.WriteLine($"Graph was L(2,1)-2 coloured by using {solution.Values.Max() + 1} colours in {stopwatch.Elapsed.TotalMilliseconds:f3}ms (lazily, cutting, optimized)");

                    Console.WriteLine();
                }
            }
        }
    }
}
