﻿using Algorithms;
using Algorithms.GraphFactory;
using colGraphReader;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace UserInterface
{
    class Program
    {
        static void Main(string[] args)
        {
            // There are 2 important parts to change, both are listed below

            // IMPORTANT1 the algorithm
            var algorithmDescription = string.Empty;
            Dictionary<int, int> generateNewAlgorithmAndSolve(EventHandler<PerformanceReport> performanceReport, Graph graph)
            {
                var algorithm = new ExactAcyclicAlgorithmDepthLimit();
                algorithmDescription = "acyclic";
                algorithm.NewBestSolutionFound += performanceReport;
                return algorithm.ColourGraph(graph);
            }

            var nRange = Enumerable.Range(24, 60);
            var rangeCount = 10;
            var densityRange = Enumerable.Range(0, rangeCount).Select(number => .5d - .5d * Math.Cos(Math.PI * ((2 * number + 1) / (2d * rangeCount))));

            Console.WriteLine("Performance tests:");
#if true
            var random = new Random(1);
            foreach (var n in nRange)
            {
                Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine();
                foreach (var density in densityRange)
                {
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.Write($"New random graph benchmark. Vertices ");
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.Write($"{n}");
                    Console.ResetColor();
                    Console.Write($", edge density ");
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine($"{density}");
                    Console.ResetColor();
                    Console.WriteLine();
                    // IMPORTANT2 the problem
                    var graph = GraphFactory.GenerateRandom2(n, density, random.Next());
                    PerformTestGetCSVResults(graph);
                }
            }
#else
            var graph2 = Reader.ParseGraph("flat300_28_0");
            PerformTestGetCSVResults(graph2);
#endif

            string PerformTestGetCSVResults(Graph graph)
            {
                var stopwatch = new Stopwatch();
                var reportedTimes = new List<TimeSpan>();
                var reportedColourings = new List<int>();

                string GetTextToExport()
                {
                    var toExport = new StringBuilder();
                    for (int i = 0; i < reportedTimes.Count; i++)
                    {
                        toExport.AppendLine($"{reportedTimes[i].TotalMilliseconds},{reportedColourings[i]}");
                    }

                    return toExport.ToString();
                }

                void PerformanceReport(object sender, PerformanceReport report)
                {
                    Console.Write($"Found new solution: ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write($"{report.minimalNumberOfColoursUsed}");
                    Console.ResetColor();
                    Console.Write($" after ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write($"{report.elapsedProcessorTime}");
                    Console.ResetColor();
                    Console.WriteLine($" ms");
                    reportedTimes.Add(report.elapsedProcessorTime);
                    reportedColourings.Add(report.minimalNumberOfColoursUsed);
                    var text = GetTextToExport();
                    File.WriteAllText($"{algorithmDescription}-{graph.ToString()}.tmp.csv", text);
                }

                //var solution = new Dictionary<int, int>();

                //Console.WriteLine($"Size {n}, density {density:f2}");

                stopwatch.Restart();
                generateNewAlgorithmAndSolve(PerformanceReport, graph);
                stopwatch.Stop();

                // mark the end of computation time
                PerformanceReport(null, new PerformanceReport()
                {
                    elapsedProcessorTime = stopwatch.Elapsed,
                    minimalNumberOfColoursUsed = reportedColourings.Last()
                });

                return GetTextToExport();

            }

        }
    }
}
