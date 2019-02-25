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
            var random = new Random(0);
            for (int n = 50; n < 100; n++)
            {
                Console.WriteLine("NEW SIZE");
                for (double density = 0.05d; density < 1d; density += 0.05d)
                {
                    var graph = GraphFactory.GenerateRandom(n, density, random.Next());
                    var solution = new Dictionary<int, int>();

                    Console.WriteLine($"Size {n}, density {density:f2}");

                    //stopwatch.Restart();
                    //var classicSolution = new ExactClassicAlgorithm().ColourGraph(graph);
                    //stopwatch.Stop();
                    //Console.WriteLine($"Graph was classically coloured by using {classicSolution.Values.Max() + 1} colours in {stopwatch.Elapsed.TotalMilliseconds:f3}ms");

                    //stopwatch.Restart();
                    //solution = new ExactClassicAlgorithmNoDel().ColourGraph(graph);
                    //stopwatch.Stop();
                    //Console.WriteLine($"Graph was classically coloured by using {solution.Values.Max() + 1} colours in {stopwatch.Elapsed.TotalMilliseconds:f3}ms (without deleting)");

                    //stopwatch.Restart();
                    //solution = new ExactClassicAlgorithmNoDelLazy().ColourGraph(graph);
                    //stopwatch.Stop();
                    //Console.WriteLine($"Graph was classically coloured by using {solution.Values.Max() + 1} colours in {stopwatch.Elapsed.TotalMilliseconds:f3}ms (without deleting, lazily)");

                    stopwatch.Restart();
                    solution = new ExactClassicAlgorithmNoDelLazyCutting().ColourGraph(graph);
                    stopwatch.Stop();
                    Console.WriteLine($"Graph was classically coloured by using {solution.Values.Max() + 1} colours in {stopwatch.Elapsed.TotalMilliseconds:f3}ms (without deleting, lazily, cutting)");

                    stopwatch.Restart();
                    solution = new ExactClassicAlgorithmNoDelLazyCuttingOptimized().ColourGraph(graph);
                    stopwatch.Stop();
                    Console.WriteLine($"Graph was classically coloured by using {solution.Values.Max() + 1} colours in {stopwatch.Elapsed.TotalMilliseconds:f3}ms (without deleting, lazily, cutting, optimized)");

                    //stopwatch.Restart();
                    //solution = new ExactAcyclicAlgorithm().ColourGraph(graph);
                    //stopwatch.Stop();
                    //Console.WriteLine($"Graph was acyclically coloured by using {solution.Values.Max() + 1} colours in {stopwatch.Elapsed.TotalMilliseconds:f3}ms");

                    //stopwatch.Restart();
                    //solution = new ExactAcyclicAlgorithmLazy().ColourGraph(graph);
                    //stopwatch.Stop();
                    //Console.WriteLine($"Graph was acyclically coloured by using {solution.Values.Max() + 1} colours in {stopwatch.Elapsed.TotalMilliseconds:f3}ms (lazily)");

                    //stopwatch.Restart();
                    //solution = new ExactAcyclicAlgorithmLazyCutting().ColourGraph(graph);
                    //stopwatch.Stop();
                    //Console.WriteLine($"Graph was acyclically coloured by using {solution.Values.Max() + 1} colours in {stopwatch.Elapsed.TotalMilliseconds:f3}ms (lazily, cutting)");

                    //stopwatch.Restart();
                    //var alpha = 1.5;
                    //solution = new ExactAcyclicAlgorithmLazyCutting().ColourGraphApproximately(graph, alpha);
                    //stopwatch.Stop();
                    //Console.WriteLine($"Graph was acyclically coloured by using {solution.Values.Max() + 1} colours (alpha={alpha:f2}) in {stopwatch.Elapsed.TotalMilliseconds:f3}ms");

                    //stopwatch.Restart();
                    //solution = new ExactL21AlgorithmNoDelLazyCutting().ColourGraph(graph);
                    //stopwatch.Stop();
                    //Console.WriteLine($"Graph was L(2,1) coloured by using {solution.Values.Max() + 1} colours in {stopwatch.Elapsed.TotalMilliseconds:f3}ms (lazily, cutting)");

                    Console.WriteLine();
                }
            }
        }
    }
}
