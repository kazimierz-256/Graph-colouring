using Algorithms.Solver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static Algorithms.Graph;

namespace Algorithms
{
    public class ExactClassicAlgorithm : ISolver
    {
        private class Solution
        {
            public int colourCount;
            public int[] vertexToColour;
            public int solvedCount;
            public Solution DeepClone()
            {
                var dictionaryClone = new int[vertexToColour.Length];
                Array.Copy(vertexToColour, dictionaryClone, vertexToColour.Length);

                return new Solution
                {
                    colourCount = colourCount,
                    vertexToColour = dictionaryClone
                };
            }
        }

        private Stopwatch stopwatch = new Stopwatch();
        public event EventHandler<PerformanceReport> NewBestSolutionFound;
        public Dictionary<int, int> ColourGraph(Graph graph) => ColourGraph(graph, -1, 1.0);
        public Dictionary<int, int> ColourGraph(Graph graph, int upperBoundOnNumberOfSteps) => ColourGraph(graph, upperBoundOnNumberOfSteps, 1.0);
        public Dictionary<int, int> ColourGraph(Graph graph, double alphaRatio) => ColourGraph(graph, -1, alphaRatio);

        private Dictionary<int, int> ColourGraph(Graph graph, int upperBoundOnNumberOfSteps, double alphaRatio)
        {
            stopwatch.Restart();
            var dummySolution = new Solution()
            {
                colourCount = int.MaxValue,
            };
            var initialSolution = new Solution()
            {
                colourCount = 0,
                vertexToColour = new int[graph.VerticesKVPs.Length]
            };
            for (int i = 0; i < graph.VerticesKVPs.Length; i++)
            {
                initialSolution.vertexToColour[i] = -1;
            }

            graph = graph.CloneNeighboursSorted(v => graph.VerticesKVPs[v].Length);

            var leftSteps = upperBoundOnNumberOfSteps;
            var solution = Recurse(graph, initialSolution, dummySolution, ref leftSteps, alphaRatio).vertexToColour;
            var dictionary = new Dictionary<int, int>();
            for (int i = 0; i < solution.Length; i++)
            {
                dictionary.Add(i, solution[i]);
            }
            stopwatch.Stop();
            return dictionary;
        }

        private Solution Recurse(Graph graphToColour, Solution currentSolution, Solution bestSolution, ref int upperBoundOnNumberOfSteps, double alphaRatio)
        {
            if (upperBoundOnNumberOfSteps != -1 && bestSolution.colourCount < int.MaxValue)
            {
                if (upperBoundOnNumberOfSteps == 0)
                    return bestSolution;
                else
                    upperBoundOnNumberOfSteps -= 1;
            }
            if (currentSolution.solvedCount < graphToColour.VerticesKVPs.Length)
            {
                // choose a vertex to colour
                var vertexToColour = ChooseSuitableVertex(graphToColour, currentSolution, bestSolution, out var colouringPossibilities);

                // for in possible colours
                foreach (var colour in colouringPossibilities)
                {
                    var increasedColourCount = false;
                    if (colour >= currentSolution.colourCount)
                    {
                        if (colour > currentSolution.colourCount)
                            throw new Exception("New colour is too large");
                        currentSolution.colourCount += 1;
                        increasedColourCount = true;
                    }
                    if (currentSolution.colourCount * alphaRatio < bestSolution.colourCount)
                    {
                        currentSolution.vertexToColour[vertexToColour] = colour;
                        currentSolution.solvedCount += 1;

                        // recurse and update best statistics
                        bestSolution = Recurse(graphToColour, currentSolution, bestSolution, ref upperBoundOnNumberOfSteps, alphaRatio);

                        currentSolution.solvedCount -= 1;
                        currentSolution.vertexToColour[vertexToColour] = -1;
                    }
                    if (increasedColourCount)
                    {
                        currentSolution.colourCount -= 1;
                    }
                }
            }
            else
            {
                // no more vertices to colour
                // if the solution is indeed better...
                if (currentSolution.colourCount >= bestSolution.colourCount)
                    throw new Exception("Proposed solution is not better");
                // warning! there may be vertices with negative colourings!
                bestSolution = currentSolution.DeepClone();
                NewBestSolutionFound?.Invoke(null, new PerformanceReport()
                {
                    minimalNumberOfColoursUsed = bestSolution.colourCount,
                    elapsedProcessorTime = stopwatch.Elapsed
                });
            }

            return bestSolution;
        }

        // may return -1 if there is no suitable vertex
        private int ChooseSuitableVertex(Graph graph, Solution currentSolution, Solution bestSolution, out List<int> bestColouring)
        {
            var minColourPossibilities = int.MaxValue;
            var maxNeighbourCount = -1;
            var maxVertex = -1;
            bestColouring = null;

            for (int i = 0; i < graph.VerticesKVPs.Length; i++)
            {
                var colouringsNeighbour = GetPossibleColourings(graph, i, currentSolution, bestSolution);
                if (colouringsNeighbour.Count == 0)
                {
                    bestColouring = colouringsNeighbour;
                    return i;
                }
                var score = colouringsNeighbour.Count;
                if (colouringsNeighbour[colouringsNeighbour.Count - 1] == currentSolution.colourCount)
                {
                    score += int.MaxValue / 2;
                }
                if (currentSolution.vertexToColour[i] == -1 && (score < minColourPossibilities || (score == minColourPossibilities && graph.VerticesKVPs[i].Length > maxNeighbourCount)))
                {
                    maxNeighbourCount = graph.VerticesKVPs[i].Length;
                    minColourPossibilities = score;
                    maxVertex = i;
                    bestColouring = colouringsNeighbour;
                }
            }

            return maxVertex;
        }

        private List<int> GetPossibleColourings(Graph graph, int vertex, Solution currentSolution, Solution bestSolution)
        {
            var possibilities = new List<int>();
            var maximumInclusivePermissibleColour = Math.Min(currentSolution.colourCount, bestSolution.colourCount - 2);
            var numberOfUnknownVertices = 0;
            var occupiedColours = new bool[graph.VerticesKVPs[vertex].Length + 1];
            foreach (var neighbour in graph.VerticesKVPs[vertex])
            {
                var colour = currentSolution.vertexToColour[neighbour];
                if (colour == -1)
                    numberOfUnknownVertices += 1;
                else if (colour < occupiedColours.Length)
                        occupiedColours[colour] = true;
                
            }

            for (int colourCandidate = 0; colourCandidate <= maximumInclusivePermissibleColour && possibilities.Count <= numberOfUnknownVertices; colourCandidate++)
            {
                if (!occupiedColours[colourCandidate])
                    possibilities.Add(colourCandidate);
            }

            return possibilities;
        }
    }
}
