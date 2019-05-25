using Algorithms.Solver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static Algorithms.Graph;

namespace Algorithms
{
    public class ExactAcyclicAlgorithmDepthLimit : ISolver
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
        public Dictionary<int, int> ColourGraph(Graph graph) => ColourGraph(graph, -1, 1.0, -1);
        public Dictionary<int, int> ColourGraphApproximately(Graph graph, int upperBoundOnNumberOfSteps) => ColourGraph(graph, upperBoundOnNumberOfSteps, 1.0, -1);
        public Dictionary<int, int> ColourGraphApproximatelyDepth(Graph graph, int depth) => ColourGraph(graph, -1, 1.0, depth);
        public Dictionary<int, int> ColourGraphApproximately(Graph graph, double alphaRatio) => ColourGraph(graph, -1, alphaRatio, -1);

        private Dictionary<int, int> ColourGraph(Graph graph, int upperBoundOnNumberOfSteps, double alphaRatio, int maxDepth)
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


            var leftSteps = upperBoundOnNumberOfSteps;
            var solution = Recurse(graph, initialSolution, dummySolution, ref leftSteps, alphaRatio, maxDepth).vertexToColour;
            var dictionary = new Dictionary<int, int>();
            for (int i = 0; i < solution.Length; i++)
            {
                dictionary.Add(i, solution[i]);
            }
            stopwatch.Stop();
            return dictionary;
        }

        private Solution Recurse(Graph graphToColour, Solution currentSolution, Solution bestSolution, ref int upperBoundOnNumberOfSteps, double alphaRatio, int maxDepth)
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
                var vertexToColour = ChooseSuitableVertex(graphToColour, currentSolution, bestSolution, out var colouringPossibilities, maxDepth);

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
                        bestSolution = Recurse(graphToColour, currentSolution, bestSolution, ref upperBoundOnNumberOfSteps, alphaRatio, maxDepth);

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
        private int ChooseSuitableVertex(Graph graph, Solution currentSolution, Solution bestSolution, out List<int> bestColouring, int maxDepth)
        {
            var minColourPossibilities = int.MaxValue;
            var maxNeighbourCount = -1;
            var maxVertex = -1;
            bestColouring = null;

            for (int i = 0; i < graph.VerticesKVPs.Length; i++)
            {
                var colouringsNeighbour = GetPossibleAcyclicColourings(graph, i, currentSolution, bestSolution, maxDepth);
                if (colouringsNeighbour.Count == 0)
                {
                    bestColouring = colouringsNeighbour;
                    return i;
                }
                if (currentSolution.vertexToColour[i] == -1 && (colouringsNeighbour.Count < minColourPossibilities || (colouringsNeighbour.Count == minColourPossibilities && graph.VerticesKVPs[i].Length > maxNeighbourCount)))
                {
                    maxNeighbourCount = graph.VerticesKVPs[i].Length;
                    minColourPossibilities = colouringsNeighbour.Count;
                    maxVertex = i;
                    bestColouring = colouringsNeighbour;
                }
            }

            return maxVertex;
        }

        private List<int> GetPossibleAcyclicColourings(Graph graph, int vertex, Solution currentSolution, Solution bestSolution, int maxDepth)
        {
            var possibilities = new List<int>();
            var maximumInclusivePermissibleColour = Math.Min(currentSolution.colourCount, bestSolution.colourCount - 2);
            var occupiedColours = new bool[maximumInclusivePermissibleColour + 1];
            foreach (var neighbour in graph.VerticesKVPs[vertex])
            {
                if (currentSolution.vertexToColour[neighbour] != -1)
                {
                    var colour = currentSolution.vertexToColour[neighbour];
                    if (colour < occupiedColours.Length)
                        occupiedColours[colour] = true;
                }
            }

            for (int colourCandidate = 0; colourCandidate <= maximumInclusivePermissibleColour; colourCandidate++)
            {
                if (!occupiedColours[colourCandidate])
                {
                    if (EnsureAcyclicityAndValidity(graph, vertex, colourCandidate, currentSolution, maxDepth))
                        possibilities.Add(colourCandidate);
                }
            }
            return possibilities;
        }

        private bool EnsureAcyclicityAndValidity(Graph graph, int vertex, int colourCandidate, Solution currentSolution, int maxDepth)
        {
            // TODO: limit depth?
            var coloursOccupiedByNeighbours = new int[currentSolution.colourCount];
            var neighboursToConsider = new int[graph.VerticesKVPs[vertex].Length];
            var neighbourToConsiderCount = 0;

            foreach (var neighbour in graph.VerticesKVPs[vertex])
            {
                var colour = currentSolution.vertexToColour[neighbour];
                if (colour != -1)
                {
                    coloursOccupiedByNeighbours[colour] += 1;
                    if (coloursOccupiedByNeighbours[colour] > 1)
                    {
                        neighboursToConsider[neighbourToConsiderCount] = neighbour;
                        neighbourToConsiderCount += 1;
                    }
                }
            }
            var coloursOccupiedByNeighboursCovered = new int[currentSolution.colourCount * currentSolution.colourCount];
            for (int i = 0; i < neighbourToConsiderCount; i++)
            {
                var neighbour = neighboursToConsider[i];
                if (currentSolution.vertexToColour[neighbour] != -1)
                {
                    var complementaryColour = currentSolution.vertexToColour[neighbour];
                    foreach (var neighbour2 in graph.VerticesKVPs[neighbour])
                    {
                        if (currentSolution.vertexToColour[neighbour2] != -1 && currentSolution.vertexToColour[neighbour2] == colourCandidate)
                        {
                            if (neighbour2 != vertex)
                            {
                                var exploredVertices = new HashSet<int>() { neighbour, neighbour2 };
                                if (FoundCycleBicolourable(graph, neighbour2, vertex, colourCandidate, complementaryColour, currentSolution, exploredVertices, maxDepth))
                                    return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        private bool FoundCycleBicolourable(Graph graph, int alreadyConsidered, int vertex, int colourCandidate, int complementaryColour, Solution currentSolution, HashSet<int> exploredVertices, int availableDepth)
        {
            if (availableDepth == 0)
            {
                return true;
            }
            foreach (var neighbour in graph.VerticesKVPs[alreadyConsidered])
            {
                if (neighbour == vertex)
                    return true;
                if (currentSolution.vertexToColour[neighbour] != -1 && !exploredVertices.Contains(neighbour))
                {
                    exploredVertices.Add(neighbour);
                    // looking for odd
                    if (exploredVertices.Count % 2 == 0 && currentSolution.vertexToColour[neighbour] == colourCandidate)
                    {
                        if (FoundCycleBicolourable(graph, neighbour, vertex, colourCandidate, complementaryColour, currentSolution, exploredVertices, availableDepth - 1))
                            return true;
                    }
                    // looking for even
                    else if (exploredVertices.Count % 2 == 1 && currentSolution.vertexToColour[neighbour] == complementaryColour)
                    {
                        if (FoundCycleBicolourable(graph, neighbour, vertex, colourCandidate, complementaryColour, currentSolution, exploredVertices, availableDepth - 1))
                            return true;
                    }
                    exploredVertices.Remove(neighbour);
                }
            }
            return false;
        }
    }
}
