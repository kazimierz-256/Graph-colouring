using Algorithms;
using Algorithms.GraphFactory;
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

            var nRange = Enumerable.Range(24, 6);
            var rangeCount = 10;
            var densityRange = Enumerable.Range(0, rangeCount).Select(number => Math.Cos(Math.PI * ((2 * number + 1) / (2d * rangeCount))));

            Console.WriteLine("Performance tests:");
            var random = new Random(1);
            foreach (var n in nRange)
            {
                Console.WriteLine($"NEW SIZE {n}");
                Console.WriteLine();
                foreach (var density in densityRange)
                {
                    Console.WriteLine($"  NEW DENSITY {density}");

                    // IMPORTANT2 the problem
                    var graph = GraphFactory.GenerateRandom2(n, density, random.Next());
                    PerformTestGetCSVResults(graph);
                }
            }

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
