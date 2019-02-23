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
                if (restrictions.VertexToColourCount[vertex][colourCandidate] == 0)
                    possibilities.Add(colourCandidate);
            }
            return possibilities;
        }
    }
}
