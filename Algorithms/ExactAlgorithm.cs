using Algorithms.Solver;
using System;
using System.Collections.Generic;
using static Algorithms.Graph;

namespace Algorithms
{
    public class ExactAlgorithm : ISolver
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

            var solution = Recurse(graph, initialRestrictions, initialSolution, dummySolution);
            
            // TODO: colour those who have negative colours in increasing order
            // figure out colouring order
            // colour greedily the uncoloured vertices

            return solution.vertexToColour;
        }

        private Solution Recurse(Graph graphToColour, Restrictions restrictions, Solution currentSolution, Solution bestSolution)
        {
            if (graphToColour.VerticesKVPs.Count > 0)
            {
                // choose a vertex to colour
                var vertexToColour = ChooseSuitableVertex(graphToColour);

                // for in possible colours
                foreach (var colour in GetPossibleColourings(graphToColour, vertexToColour, currentSolution, restrictions, bestSolution))
                {
                    var increasedColourCount = false;
                    if (colour >= currentSolution.colourCount)
                    {
                        currentSolution.colourCount += 1;
                        increasedColourCount = true;
                    }
                    // append restrictions to neighbours
                    foreach (var neighbour in graphToColour.VerticesKVPs[vertexToColour])
                    {
                        restrictions.VertexToColourCount[neighbour][colour] += 1;
                    }

                    // remove vertex
                    var restoreOperations = new Stack<RestoreOp>();
                    restoreOperations.Push(graphToColour.RemoveVertex(vertexToColour));
                    currentSolution.vertexToColour.Add(vertexToColour, colour);

                    // remove vertices that are easily colourable (consider candidates only!)
                    //var foundEasilyColourableVertex = true;
                    //while (foundEasilyColourableVertex)
                    //{
                    //    foundEasilyColourableVertex = false;
                    //    foreach (var vertexKVP in graphToColour.VerticesKVPs)
                    //    {
                    //        var upperBoundOnNeighbouringDifferentColourCount = vertexKVP.Value.Count;

                    //        // for speed, improve bound if necessary
                    //        if (upperBoundOnNeighbouringDifferentColourCount >= currentSolution.colourCount)
                    //        {
                    //            var maximumColour = restrictions.VertexToColourCount[vertexKVP.Key].Length;
                    //            for (int possibleColour = 0; possibleColour < maximumColour; possibleColour++)
                    //            {
                    //                upperBoundOnNeighbouringDifferentColourCount -= Math.Max(0, restrictions.VertexToColourCount[vertexKVP.Key][possibleColour] - 1);
                    //            }
                    //        }

                    //        if (upperBoundOnNeighbouringDifferentColourCount < currentSolution.colourCount)
                    //        {
                    //            restoreOperations.Push(graphToColour.RemoveVertex(vertexKVP.Key));
                    //            // "I suppose you think that was terribly clever"
                    //            currentSolution.vertexToColour.Add(vertexKVP.Key, -currentSolution.vertexToColour.Keys.Count);
                    //            foundEasilyColourableVertex = true;
                    //            break;
                    //        }
                    //    }
                    //}

                    // recurse and update best statistics
                    bestSolution = Recurse(graphToColour, restrictions, currentSolution, bestSolution);

                    // restore all vertices
                    while (restoreOperations.Count > 0)
                    {
                        var restoreOp = restoreOperations.Pop();
                        graphToColour.RestoreVertex(restoreOp);
                        currentSolution.vertexToColour.Remove(restoreOp.vertex);
                    }

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

        // assumes vertices exist in graph
        private int ChooseSuitableVertex(Graph graphToColour)
        {
            var maxNeighbourCount = -1;
            var maxVertex = -1;
            foreach (var vertexKVP in graphToColour.VerticesKVPs)
            {
                if (vertexKVP.Value.Count > maxNeighbourCount)
                {
                    maxNeighbourCount = vertexKVP.Value.Count;
                    maxVertex = vertexKVP.Key;
                }
            }

            return maxVertex;
        }

        private List<int> GetPossibleColourings(Graph graph, int vertex, Solution currentSolution, Restrictions restrictions, Solution bestSolution)
        {
            var possibilities = new List<int>();
            var maximumInclusivePermissibleColour = Math.Min(currentSolution.colourCount, bestSolution.colourCount - 2);
            for (int colourCandidate = 0; colourCandidate <= maximumInclusivePermissibleColour; colourCandidate++)
            {
                if (restrictions.VertexToColourCount[vertex][colourCandidate] == 0)
                    possibilities.Add(colourCandidate);
            }
            return possibilities;
        }

        private List<int> GetPossibleAcyclicColourings(Graph graph, int vertex, Solution currentSolution, Restrictions restrictions, Solution bestSolution)
        {
            // TODO: important part to implement
            throw new NotImplementedException();
        }
    }
}
