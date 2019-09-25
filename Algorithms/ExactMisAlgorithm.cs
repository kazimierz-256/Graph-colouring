using Algorithms.Solver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static Algorithms.Graph;

namespace Algorithms
{
    public class ExactMisAlgorithm : ISolver
    {

        private class GraphFast
        {
            public int[][] adjacencyMatrix;
            public int[] vertices;
            public int vertexCount;

            // negative number means the vertex is not present in the graph
            public int[] vertexPositionInVertices;

            public int[][] neighbours;
            public int[][] neighbourPositionInNeighbourhood;
            public int[] neighbourCount;
            public GraphFast(Graph graph)
            {
                // copy over bare details
                // generate proper graph representation
                vertices = new int[graph.VerticesKVPs.Length];
                vertexCount = graph.VerticesKVPs.Length;
                neighbourCount = new int[graph.VerticesKVPs.Length];
                vertexPositionInVertices = new int[graph.VerticesKVPs.Length];
                neighbours = new int[graph.VerticesKVPs.Length][];
                adjacencyMatrix = new int[graph.VerticesKVPs.Length][];
                for (int i = 0; i < graph.VerticesKVPs.Length; i++)
                {
                    neighbours[i] = new int[graph.VerticesKVPs[i].Length];
                    adjacencyMatrix[i] = new int[graph.VerticesKVPs[i].Length];
                    graph.VerticesKVPs[i].CopyTo(neighbours[i], 0);
                    graph.VerticesKVPs[i].CopyTo(adjacencyMatrix[i], 0);

                    vertices[i] = i;
                    neighbourCount[i] = graph.VerticesKVPs[i].Length;
                    vertexPositionInVertices[i] = i;
                }


            }

            private void RemoveElementFromArray(
                int elementToRemove,
                int[] positionIdentificationArray,
                int[] arrayItself,
                ref int arraySize)
            {
                var removedElementPosition = positionIdentificationArray[elementToRemove];
                var elementToSwap = arrayItself[arraySize - 1];
                arrayItself[arraySize - 1] = elementToRemove;
                arrayItself[removedElementPosition] = elementToSwap;
                positionIdentificationArray[elementToSwap] = removedElementPosition;
                positionIdentificationArray[elementToRemove] = arraySize - 1;// could store "-removedElementPosition" to retain a backup
                arraySize -= 1;
            }

            // does not preserve order upon backtracking
            private int RestoreElementFromArray(
                int[] positionIdentificationArray,
                int[] arrayItself,
                ref int arraySize)
            {
                // var elementToRestore = arrayItself[arraySize];
                // var vertexToSwapWithPosition = -positionIdentificationArray[elementToRestore];
                // var elementToSwapWith = arrayItself[vertexToSwapWithPosition];
                // arrayItself[vertexToSwapWithPosition] = elementToRestore;
                // arrayItself[arraySize] = elementToSwapWith;
                // positionIdentificationArray[elementToRestore] = vertexToSwapWithPosition;
                // positionIdentificationArray[elementToSwapWith] = arraySize;
                arraySize += 1;

                return arrayItself[arraySize - 1];
            }

            public void RemoveVertex(int vertexToDelete)
            {
                RemoveElementFromArray(vertexToDelete, vertexPositionInVertices, vertices, ref vertexCount);

                // TODO: remove the vertex from neighbours
                // for each neighbour swap using the same method
                for (int i = 0; i < neighbourCount[vertexToDelete]; i++)
                {
                    var neighbour = neighbours[vertexToDelete][i];
                    RemoveElementFromArray(vertexToDelete, neighbourPositionInNeighbourhood[neighbour], neighbours[neighbour], ref vertexCount);
                }
            }
            public void RestoreDeletedVertex()
            {
                var restoredVertex = RestoreElementFromArray(vertexPositionInVertices, vertices, ref vertexCount);

                // TODO: remove the vertex from neighbours
                // for each neighbour swap using the same method
                for (int i = 0; i < neighbourCount[restoredVertex]; i++)
                {
                    var neighbour = neighbours[restoredVertex][i];
                    RestoreElementFromArray(neighbourPositionInNeighbourhood[neighbour], neighbours[neighbour], ref vertexCount);
                }
            }
        }

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

        private class MisResult
        {
            public List<int> vertices;
            public GraphFast graphWithoutMIS;
        }

        // ignoredVertexCount might turn out to be useless
        private IEnumerable<MisResult> FindMIS(GraphFast graph,
                                               GraphFast complementaryGraph,
                                               List<int> verticesAlreadyInMIS,
                                               bool[] ignoredVerex,
                                               int ignoredVertexCount)
        {
            // add detailed parameters
            if (graph.vertexCount == 0)
            {
                yield return new MisResult()
                {
                    graphWithoutMIS = complementaryGraph,
                    vertices = verticesAlreadyInMIS
                };

            }
            else if (graph.vertexCount == 1)
            {
                // ensure everything is all wright with leftovers!

                var chosenVertex = graph.vertices[0];
                complementaryGraph.RemoveVertex(chosenVertex);
                verticesAlreadyInMIS.Add(chosenVertex);

                yield return new MisResult()
                {
                    graphWithoutMIS = complementaryGraph,
                    vertices = verticesAlreadyInMIS
                };

                verticesAlreadyInMIS.RemoveAt(verticesAlreadyInMIS.Count - 1);
                complementaryGraph.RestoreDeletedVertex();
            }
            else
            {
                // choose a vertex v
                var chosenVertex = -1;

                var vertexScore = int.MinValue;
                for (int i = 0; i < graph.vertexCount; i++)
                {
                    var tmpVertex = graph.vertices[i];
                    if (!ignoredVerex[tmpVertex])
                    {
                        var tmpVertexScore = graph.neighbourCount[tmpVertex];
                        if (tmpVertexScore > vertexScore)
                        {
                            chosenVertex = tmpVertex;
                            vertexScore = tmpVertexScore;
                        }
                    }
                }

                if (chosenVertex != -1)
                {

                    // ADD VERTEX V TO MIS
                    // remove the vertex and all its neighbours
                    var neighbours = graph.neighbours[chosenVertex];
                    var neighbourCount = graph.neighbourCount[chosenVertex];

                    graph.RemoveVertex(chosenVertex);
                    var newIgnoredVertexCount = ignoredVertexCount;
                    for (int i = 0; i < neighbourCount; i++)
                    {
                        var neighbour = neighbours[i];
                        if (ignoredVerex[neighbour])
                            newIgnoredVertexCount -= 1;
                        graph.RemoveVertex(neighbour);
                    }

                    // construct complementary graph
                    complementaryGraph.RemoveVertex(chosenVertex);
                    verticesAlreadyInMIS.Add(chosenVertex);
                    foreach (var MIS in FindMIS(graph, complementaryGraph, verticesAlreadyInMIS, ignoredVerex, newIgnoredVertexCount))
                    {
                        yield return MIS;
                    }
                    verticesAlreadyInMIS.RemoveAt(verticesAlreadyInMIS.Count - 1);
                    complementaryGraph.RestoreDeletedVertex();

                    // restore

                    for (int i = 0; i < neighbourCount; i++)
                    {
                        graph.RestoreDeletedVertex();
                    }
                    graph.RestoreDeletedVertex();

                    // DO NOT ADD VERTEX V TO MIS
                    // TODO: add warnings to his neighbours, add notion that the chosen vertex needs to be "covered" by someone!
                    // each vertex has a satifiability list
                    // there is a variable representing vertices left to satisfy
                    // each vertex has a counter that (when >0) triggers satisfiability stuff
                    ignoredVerex[chosenVertex] = true;
                    foreach (var MIS in FindMIS(graph, complementaryGraph, verticesAlreadyInMIS, ignoredVerex, ignoredVertexCount + 1))
                    {
                        yield return MIS;
                    }
                    ignoredVerex[chosenVertex] = false;
                }
            }
        }

        public IEnumerable<List<int>> EnumerateMIS(Graph graph)
        {
            var primaryGraph = new GraphFast(graph);
            var complementaryGraph = new GraphFast(graph);
            var ignoredVerex = new bool[graph.VerticesKVPs.Length];
            var verticesAlreadyInMIS = new List<int>();
            foreach (var mis in FindMIS(primaryGraph, complementaryGraph, verticesAlreadyInMIS, ignoredVerex, 0))
            {
                yield return mis.vertices;
            }
        }

        private Solution Recurse(GraphFast graph, Graph graphToColour, Solution currentSolution, Solution bestSolution, ref int upperBoundOnNumberOfSteps, double alphaRatio)
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
            var secondLimitingColourInclusive = graph.VerticesKVPs[vertex].Length;
            var occupiedColours = new bool[graph.VerticesKVPs[vertex].Length + 1];
            foreach (var neighbour in graph.VerticesKVPs[vertex])
            {
                if (currentSolution.vertexToColour[neighbour] != -1)
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
                    possibilities.Add(colourCandidate);
                }
            }

            return possibilities;
        }
    }
}
