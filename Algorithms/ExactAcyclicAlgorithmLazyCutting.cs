using Algorithms.Solver;
using System;
using System.Collections.Generic;
using static Algorithms.Graph;

namespace Algorithms
{
    public class ExactAcyclicAlgorithmLazyCutting : ISolver
    {
        private class Solution
        {
            public int colourCount;
            public Dictionary<int, int> vertexToColour;
            public Solution DeepClone()
            {
                var dictionaryClone = new Dictionary<int, int>();
                foreach (var kvp in vertexToColour)
                    dictionaryClone.Add(kvp.Key, kvp.Value);

                return new Solution
                {
                    colourCount = colourCount,
                    vertexToColour = dictionaryClone
                };
            }
        }

        public Dictionary<int, int> ColourGraph(Graph graph) => ColourGraph(graph, -1, 1.0);
        public Dictionary<int, int> ColourGraphApproximately(Graph graph, int upperBoundOnNumberOfSteps) => ColourGraph(graph, upperBoundOnNumberOfSteps, 1.0);
        public Dictionary<int, int> ColourGraphApproximately(Graph graph, double alphaRatio) => ColourGraph(graph, -1, alphaRatio);

        private Dictionary<int, int> ColourGraph(Graph graph, int upperBoundOnNumberOfSteps, double alphaRatio)
        {
            var dummySolution = new Solution()
            {
                colourCount = int.MaxValue,
            };
            var initialSolution = new Solution()
            {
                colourCount = 0,
                vertexToColour = new Dictionary<int, int>()
            };


            var leftSteps = upperBoundOnNumberOfSteps;
            return Recurse(graph, initialSolution, dummySolution, ref leftSteps, alphaRatio).vertexToColour;
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
            if (currentSolution.vertexToColour.Count < graphToColour.VerticesKVPs.Count)
            {
                // choose a vertex to colour
                var vertexToColour = ChooseSuitableVertex(graphToColour, currentSolution, bestSolution);

                // for in possible colours
                foreach (var colour in GetPossibleAcyclicColourings(graphToColour, vertexToColour, currentSolution, bestSolution))
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

                        // remove vertex
                        //var restoreOperations = new Stack<RestoreOp>();
                        //restoreOperations.Push(graphToColour.RemoveVertex(vertexToColour));
                        currentSolution.vertexToColour.Add(vertexToColour, colour);

                        // recurse and update best statistics
                        bestSolution = Recurse(graphToColour, currentSolution, bestSolution, ref upperBoundOnNumberOfSteps, alphaRatio);

                        currentSolution.vertexToColour.Remove(vertexToColour);
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
            }

            return bestSolution;
        }

        // may return -1 if there is no suitable vertex
        private int ChooseSuitableVertex(Graph graph, Solution currentSolution, Solution bestSolution)
        {
            var minColourPossibilities = int.MaxValue;
            var maxNeighbourCount = -1;
            var maxVertex = -1;
            var vertexToPossibleColourings = new Dictionary<int, int>();
            foreach (var vertexKVP in graph.VerticesKVPs)
            {
                vertexToPossibleColourings.Add(vertexKVP.Key, GetPossibleAcyclicColourings(graph, vertexKVP.Key, currentSolution, bestSolution).Count);
            }

            foreach (var vertexKVP in graph.VerticesKVPs)
            {
                // ensure larger neighbourhood and ensure it is not coloured
                var colouringsNeighbour = vertexToPossibleColourings[vertexKVP.Key];
                if (!currentSolution.vertexToColour.ContainsKey(vertexKVP.Key) && (colouringsNeighbour < minColourPossibilities || (colouringsNeighbour == minColourPossibilities && vertexKVP.Value.Count > maxNeighbourCount)))
                {
                    maxNeighbourCount = vertexKVP.Value.Count;
                    minColourPossibilities = colouringsNeighbour;
                    maxVertex = vertexKVP.Key;
                }
            }

            return maxVertex;
        }

        private List<int> GetPossibleAcyclicColourings(Graph graph, int vertex, Solution currentSolution, Solution bestSolution)
        {
            var possibilities = new List<int>();
            var maximumInclusivePermissibleColour = Math.Min(currentSolution.colourCount, bestSolution.colourCount - 2);
            var secondLimitingColourInclusive = graph.VerticesKVPs[vertex].Count;
            var occupiedColours = new bool[graph.VerticesKVPs[vertex].Count + 1];
            foreach (var neighbour in graph.VerticesKVPs[vertex])
            {
                if (currentSolution.vertexToColour.ContainsKey(neighbour))
                {
                    var colour = currentSolution.vertexToColour[neighbour];
                    if (colour < occupiedColours.Length)
                    {
                        if (occupiedColours[colour])
                            secondLimitingColourInclusive -= 1;
                        else
                            occupiedColours[colour] = true;
                    }
                }
            }
            maximumInclusivePermissibleColour = Math.Min(maximumInclusivePermissibleColour, secondLimitingColourInclusive);
            for (int colourCandidate = 0; colourCandidate <= maximumInclusivePermissibleColour; colourCandidate++)
            {
                if (!occupiedColours[colourCandidate])
                {
                    if (EnsureAcyclicityAndValidity(graph, vertex, colourCandidate, currentSolution))
                        possibilities.Add(colourCandidate);
                }
            }
            return possibilities;
        }

        private bool EnsureAcyclicityAndValidity(Graph graph, int vertex, int colourCandidate, Solution currentSolution)
        {
            foreach (var neighbour in graph.VerticesKVPs[vertex])
            {
                if (currentSolution.vertexToColour.ContainsKey(neighbour))
                {
                    var complementaryColour = currentSolution.vertexToColour[neighbour];
                    foreach (var neighbour2 in graph.VerticesKVPs[neighbour])
                    {
                        if (currentSolution.vertexToColour.ContainsKey(neighbour2) && currentSolution.vertexToColour[neighbour2] == colourCandidate)
                        {
                            if (neighbour2 != vertex)
                            {
                                var exploredVertices = new HashSet<int>() { neighbour, neighbour2 };
                                if (!Explore(graph, neighbour2, vertex, colourCandidate, complementaryColour, currentSolution, exploredVertices))
                                    return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        private bool Explore(Graph graph, int alreadyConsidered, int vertex, int colourCandidate, int complementaryColour, Solution currentSolution, HashSet<int> exploredVertices)
        {
            foreach (var neighbour in graph.VerticesKVPs[alreadyConsidered])
            {
                if (neighbour == vertex)
                    return false;
                if (currentSolution.vertexToColour.ContainsKey(neighbour) && !exploredVertices.Contains(neighbour))
                {
                    exploredVertices.Add(neighbour);
                    // looking for odd
                    if (exploredVertices.Count % 2 == 0 && currentSolution.vertexToColour[neighbour] == colourCandidate)
                    {
                        if (!Explore(graph, neighbour, vertex, colourCandidate, complementaryColour, currentSolution, exploredVertices))
                            return false;
                    }
                    // looking for even
                    else if (exploredVertices.Count % 2 == 1 && currentSolution.vertexToColour[neighbour] == complementaryColour)
                    {
                        if (!Explore(graph, neighbour, vertex, colourCandidate, complementaryColour, currentSolution, exploredVertices))
                            return false;
                    }
                    exploredVertices.Remove(neighbour);
                }
            }
            return true;
        }
    }
}
