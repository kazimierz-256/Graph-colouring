using Algorithms.Solver;
using System;
using System.Collections.Generic;
using static Algorithms.Graph;

namespace Algorithms
{
    public class ExactL21Algorithm2 : ISolver
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

        public Dictionary<int, int> ColourGraph(Graph graph) => ColourGraph(graph, -1, 1.0);
        public Dictionary<int, int> ColourGraph(Graph graph, int upperBoundOnNumberOfSteps) => ColourGraph(graph, upperBoundOnNumberOfSteps, 1.0);
        public Dictionary<int, int> ColourGraph(Graph graph, double alphaRatio) => ColourGraph(graph, -1, alphaRatio);

        private Dictionary<int, int> ColourGraph(Graph graph, int upperBoundOnNumberOfSteps, double alphaRatio)
        {
            var dummySolution = new Solution()
            {
                colourCount = 0,
                vertexToColour = null
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
            Solution tmpSolution = dummySolution;
            while (tmpSolution.vertexToColour == null)
            {
                dummySolution.colourCount += 1;
                tmpSolution = Recurse(graph, initialSolution, dummySolution, ref leftSteps, alphaRatio);
            }
            var solution = tmpSolution.vertexToColour;
            var dictionary = new Dictionary<int, int>();
            for (int i = 0; i < solution.Length; i++)
            {
                dictionary.Add(i, solution[i]);
            }
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
                    var originalColourCount = currentSolution.colourCount;
                    if (colour >= currentSolution.colourCount)
                    {
                        if (colour > currentSolution.colourCount + 1)
                            throw new Exception("New colour is too large");
                        currentSolution.colourCount = colour + 1;
                    }
                    if (currentSolution.colourCount * alphaRatio <= bestSolution.colourCount)
                    {
                        currentSolution.vertexToColour[vertexToColour] = colour;
                        currentSolution.solvedCount += 1;

                        // recurse and update best statistics
                        bestSolution = Recurse(graphToColour, currentSolution, bestSolution, ref upperBoundOnNumberOfSteps, alphaRatio);

                        currentSolution.solvedCount -= 1;
                        currentSolution.vertexToColour[vertexToColour] = -1;
                    }
                    currentSolution.colourCount = originalColourCount;
                }
            }
            else
            {
                // no more vertices to colour
                // if the solution is indeed better...
                if (currentSolution.colourCount > bestSolution.colourCount)
                    throw new Exception("Proposed solution is not better");
                // warning! there may be vertices with negative colourings!
                bestSolution = currentSolution.DeepClone();
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
                var colouringsNeighbour = GetPossibleL21Colourings(graph, i, currentSolution, bestSolution);
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

        private List<int> GetPossibleL21Colourings(Graph graph, int vertex, Solution currentSolution, Solution bestSolution)
        {
            var possibilities = new List<int>();
            var maximumInclusivePermissibleColour = Math.Min(currentSolution.colourCount + 1, bestSolution.colourCount - 2);
            var occupiedColours = new bool[maximumInclusivePermissibleColour + 1];
            foreach (var neighbour in graph.VerticesKVPs[vertex])
            {
                if (currentSolution.vertexToColour[neighbour] != -1)
                {
                    var colour = currentSolution.vertexToColour[neighbour];
                    if (colour < occupiedColours.Length)
                        occupiedColours[colour] = true;
                    if (colour + 1 < occupiedColours.Length)
                        occupiedColours[colour + 1] = true;
                    if (colour - 1 >= 0)
                        occupiedColours[colour - 1] = true;
                }

                foreach (var neighbour2 in graph.VerticesKVPs[neighbour])
                {
                    if (currentSolution.vertexToColour[neighbour2] != -1)
                    {
                        var colour = currentSolution.vertexToColour[neighbour2];
                        if (colour < occupiedColours.Length)
                            occupiedColours[colour] = true;
                    }
                }
            }

            for (int colourCandidate = 0; colourCandidate <= maximumInclusivePermissibleColour; colourCandidate++)
            {
                if (!occupiedColours[colourCandidate])
                {
                    possibilities.Add(colourCandidate);
                }
            }

            return possibilities;
        }
    }
}
