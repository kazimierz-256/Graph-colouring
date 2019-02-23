using Algorithms.Solver;
using System;
using System.Collections.Generic;
using static Algorithms.Graph;

namespace Algorithms
{
    public class ExactAcyclicAlgorithm : ISolver
    {
        private class Restrictions
        {
            public Dictionary<int, int[]> VertexToColourCount;
        }

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

        public Dictionary<int, int> ColourGraph(Graph graph)
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
            var initialRestrictions = new Restrictions()
            {
                VertexToColourCount = new Dictionary<int, int[]>(),
            };

            foreach (var vertexKVP in graph.VerticesKVPs)
            {
                initialRestrictions.VertexToColourCount.Add(vertexKVP.Key, new int[graph.VerticesKVPs.Count]);
            }

            return Recurse(graph, initialRestrictions, initialSolution, dummySolution).vertexToColour;
        }

        private Solution Recurse(Graph graphToColour, Restrictions restrictions, Solution currentSolution, Solution bestSolution)
        {
            if (currentSolution.vertexToColour.Count < graphToColour.VerticesKVPs.Count)
            {
                // choose a vertex to colour
                var vertexToColour = ChooseSuitableVertex(graphToColour, currentSolution);

                // for in possible colours
                foreach (var colour in GetPossibleAcyclicColourings(graphToColour, vertexToColour, currentSolution, restrictions, bestSolution))
                {
                    var increasedColourCount = false;
                    if (colour >= currentSolution.colourCount)
                    {
                        if (colour >= bestSolution.colourCount - 1)
                            continue;
                        currentSolution.colourCount += 1;
                        increasedColourCount = true;
                    }
                    // append restrictions to neighbours
                    foreach (var neighbour in graphToColour.VerticesKVPs[vertexToColour])
                    {
                        restrictions.VertexToColourCount[neighbour][colour] += 1;
                    }

                    // remove vertex
                    //var restoreOperations = new Stack<RestoreOp>();
                    //restoreOperations.Push(graphToColour.RemoveVertex(vertexToColour));
                    currentSolution.vertexToColour.Add(vertexToColour, colour);

                    // recurse and update best statistics
                    bestSolution = Recurse(graphToColour, restrictions, currentSolution, bestSolution);

                    currentSolution.vertexToColour.Remove(vertexToColour);

                    // restore restrictions
                    foreach (var neighbour in graphToColour.VerticesKVPs[vertexToColour])
                    {
                        restrictions.VertexToColourCount[neighbour][colour] -= 1;
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
                    throw new Exception("Something went terribly wrong, proposed solution is not better");
                // warning! there may be vertices with negative colourings!
                bestSolution = currentSolution.DeepClone();
            }

            return bestSolution;
        }

        // may return -1 if there is no suitable vertex
        private int ChooseSuitableVertex(Graph graphToColour, Solution currentSolution)
        {
            var maxNeighbourCount = -1;
            var maxVertex = -1;
            foreach (var vertexKVP in graphToColour.VerticesKVPs)
            {
                // ensure larger neighbourhood and ensure it is not coloured
                if (vertexKVP.Value.Count > maxNeighbourCount && !currentSolution.vertexToColour.ContainsKey(vertexKVP.Key))
                {
                    maxNeighbourCount = vertexKVP.Value.Count;
                    maxVertex = vertexKVP.Key;
                }
            }

            return maxVertex;
        }

        private List<int> GetPossibleAcyclicColourings(Graph graph, int vertex, Solution currentSolution, Restrictions restrictions, Solution bestSolution)
        {
            var possibilities = new List<int>();
            var maximumInclusivePermissibleColour = Math.Min(currentSolution.colourCount, bestSolution.colourCount - 2);
            for (int colourCandidate = 0; colourCandidate <= maximumInclusivePermissibleColour; colourCandidate++)
            {
                // TODO: ensure acyclicity!
                // perform a DFS search and try to reach this very vertex by following a pattern
                if (restrictions.VertexToColourCount[vertex][colourCandidate] == 0)
                {
                    if (EnsureAcyclicity(graph, vertex, colourCandidate, currentSolution))
                        possibilities.Add(colourCandidate);
                }
            }
            return possibilities;
        }

        private bool EnsureAcyclicity(Graph graph, int vertex, int colourCandidate, Solution currentSolution)
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
                if (currentSolution.vertexToColour.ContainsKey(neighbour) && !exploredVertices.Contains(neighbour))
                {
                    exploredVertices.Add(neighbour);
                    // looking for odd
                    if (exploredVertices.Count % 2 == 0 && currentSolution.vertexToColour[neighbour] == complementaryColour)
                    {
                        if (!Explore(graph, neighbour, vertex, colourCandidate, complementaryColour, currentSolution, exploredVertices))
                            return false;
                    }
                    // looking for even
                    else if (exploredVertices.Count % 2 == 1 && currentSolution.vertexToColour[neighbour] == colourCandidate)
                    {
                        if (neighbour == vertex)
                            return false;

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
