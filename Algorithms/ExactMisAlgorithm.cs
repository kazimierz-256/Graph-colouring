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
            //public bool[][] adjacencyMatrix;
            public int[] vertices;
            public int vertexCount;

            // negative number means the vertex is not present in the graph
            public int[] vertexPositionInVertices;

            public int[][] neighbours;
            public int[][] neighbourPositionInNeighbourhood;
            public int[] neighbourCount;
            public GraphFast ShallowClone()
            {
                return new GraphFast(vertices, vertexCount, vertexPositionInVertices, neighbours, neighbourPositionInNeighbourhood, neighbourCount);
            }
            public GraphFast DeepIrreversibleClone()
            {
                var newVertices = new int[vertexCount];
                var newVertexPositionInVertices = new int[vertexPositionInVertices.Length];
                var newNeighbourCount = new int[neighbourCount.Length];
                var newNeighbours = new int[neighbours.Length][];
                var newNeighbourPositionInNeighbourhood = new int[neighbourPositionInNeighbourhood.Length][];
                for (int i = 0; i < vertexCount; i++)
                {
                    var vertex = vertices[i];
                    newVertices[i] = vertex;
                    newVertexPositionInVertices[vertex] = vertexPositionInVertices[vertex];
                    newNeighbourCount[vertex] = neighbourCount[vertex];
                    newNeighbours[vertex] = new int[neighbourCount[vertex]];
                    newNeighbourPositionInNeighbourhood[vertex] = new int[neighbourPositionInNeighbourhood.Length];
                    for (int j = 0; j < neighbourCount[vertex]; j++)
                    {
                        var neighbour = neighbours[vertex][j];
                        newNeighbours[vertex][j] = neighbour;
                        newNeighbourPositionInNeighbourhood[vertex][neighbour] = neighbourPositionInNeighbourhood[vertex][neighbour];
                    }
                }
                return new GraphFast(newVertices, vertexCount, newVertexPositionInVertices, newNeighbours, newNeighbourPositionInNeighbourhood, newNeighbourCount);
            }
            public GraphFast(Graph graph)
            {
                // copy over bare details
                // generate proper graph representation
                vertices = new int[graph.VerticesKVPs.Length];
                vertexCount = graph.VerticesKVPs.Length;
                neighbourCount = new int[graph.VerticesKVPs.Length];
                vertexPositionInVertices = new int[graph.VerticesKVPs.Length];
                neighbours = new int[graph.VerticesKVPs.Length][];
                //adjacencyMatrix = new bool[graph.VerticesKVPs.Length][];
                neighbourPositionInNeighbourhood = new int[graph.VerticesKVPs.Length][];
                for (int i = 0; i < graph.VerticesKVPs.Length; i++)
                {
                    neighbours[i] = new int[graph.VerticesKVPs[i].Length];
                    neighbourPositionInNeighbourhood[i] = new int[graph.VerticesKVPs.Length];
                    //adjacencyMatrix[i] = new bool[graph.VerticesKVPs[i].Length];
                    graph.VerticesKVPs[i].CopyTo(neighbours[i], 0);
                    for (int j = 0; j < graph.VerticesKVPs[i].Length; j++)
                    {
                        //adjacencyMatrix[i][j] = true;
                        neighbourPositionInNeighbourhood[i][neighbours[i][j]] = j;
                    }

                    vertices[i] = i;
                    neighbourCount[i] = graph.VerticesKVPs[i].Length;
                    vertexPositionInVertices[i] = i;
                }


            }

            public GraphFast(int[] vertices, int vertexCount, int[] vertexPositionInVertices, int[][] neighbours, int[][] neighbourPositionInNeighbourhood, int[] neighbourCount)
            {
                //this.adjacencyMatrix = adjacencyMatrix;
                this.vertices = vertices;
                this.vertexCount = vertexCount;
                this.vertexPositionInVertices = vertexPositionInVertices;
                this.neighbours = neighbours;
                this.neighbourPositionInNeighbourhood = neighbourPositionInNeighbourhood;
                this.neighbourCount = neighbourCount;
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
                // positionIdentificationArray[elementToRemove] = -removedElementPosition;
                arraySize -= 1;
            }

            // does not preserve order upon backtracking
            private void RestoreElementFromArray(
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

                // return arrayItself[arraySize - 1];
            }

            public void RemoveVertex(int vertexToDelete)
            {
                RemoveElementFromArray(vertexToDelete, vertexPositionInVertices, vertices, ref vertexCount);

                // TODO: remove the vertex from neighbours
                // for each neighbour swap using the same method
                for (int i = 0; i < neighbourCount[vertexToDelete]; i++)
                {
                    var neighbour = neighbours[vertexToDelete][i];
                    RemoveElementFromArray(vertexToDelete, neighbourPositionInNeighbourhood[neighbour], neighbours[neighbour], ref neighbourCount[neighbour]);
                }
            }
            public void RestoreDeletedVertex()
            {
                RestoreElementFromArray(vertexPositionInVertices, vertices, ref vertexCount);
                var restoredVertex = vertices[vertexCount - 1];
                // TODO: remove the vertex from neighbours
                // for each neighbour swap using the same method
                for (int i = 0; i < neighbourCount[restoredVertex]; i++)
                {
                    var neighbour = neighbours[restoredVertex][i];
                    RestoreElementFromArray(neighbourPositionInNeighbourhood[neighbour], neighbours[neighbour], ref neighbourCount[neighbour]);
                }
            }
        }

        private class Solution
        {
            public int colourCount;
            public Stack<List<int>> colourLayer;

            public Solution DeepClone()
            {
                var colourLayerClone = new Stack<List<int>>();
                foreach (var layer in colourLayer)
                {
                    var clonedLayer = new List<int>(layer);
                    colourLayerClone.Push(clonedLayer);
                }

                return new Solution
                {
                    colourCount = colourCount,
                    colourLayer = colourLayerClone
                };
            }
        }

        private Stopwatch stopwatch = new Stopwatch();
        public event EventHandler<PerformanceReport> NewBestSolutionFound;
        public Dictionary<int, int> ColourGraph(Graph graph)
        {
            stopwatch.Restart();
            var dummySolution = new Solution()
            {
                colourCount = int.MaxValue,
            };
            var initialSolution = new Solution()
            {
                colourCount = 0,
                colourLayer = new Stack<List<int>>()
            };

            graph = graph.CloneNeighboursSorted(v => graph.VerticesKVPs[v].Length);

            // var solution = Recurse(graph, initialSolution, dummySolution, ref leftSteps, alphaRatio).vertexToColour;
            var dictionary = new Dictionary<int, int>();
            //var misCount = 0;
            //var largestMIS = 0;
            //foreach (var mis in EnumerateMIS(graph))
            //{
            //    misCount += 1;
            //    if (mis.vertices.Count > largestMIS)
            //        largestMIS = mis.vertices.Count;
            //}
            //System.Console.WriteLine($"MIS count: {misCount}, largest: {largestMIS}");
            var ignoredVertices = new bool[graph.VerticesKVPs.Length];
            var primaryGraph = new GraphFast(graph);
            var complementaryGraph = new GraphFast(graph);
            var bestSolution = Recurse(graph.VerticesKVPs.Length, primaryGraph, complementaryGraph, initialSolution, dummySolution);
            var layerColour = 0;
            foreach (var layer in bestSolution.colourLayer)
            {
                foreach (var vertex in layer)
                {
                    dictionary.Add(vertex, layerColour);
                }
                layerColour += 1;
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
        private bool FindMIS(
            GraphFast graph,
            GraphFast complementaryGraph,
            List<int> verticesAlreadyInMIS,
            bool[] ignoredVertex,
            int ignoredVertexCount,
            Func<MisResult, bool> consumeAndDecideToContinue)
        {
            var continueExecution = true;
            // add detailed parameters
            if (graph.vertexCount == 0)
            {
                // ignored vertices can only be removed by nonignored so the following if is unnecessary
                // if (ignoredVertexCount == 0)
                continueExecution = consumeAndDecideToContinue(new MisResult()
                {
                    graphWithoutMIS = complementaryGraph,
                    vertices = verticesAlreadyInMIS
                });
            }
            else
            {
                // choose a vertex v
                var chosenVertex = -1;

                var vertexScore = int.MinValue;
                for (int i = 0; i < graph.vertexCount; i++)
                {
                    var tmpVertex = graph.vertices[i];
                    if (!ignoredVertex[tmpVertex])
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
                        if (ignoredVertex[neighbour])
                            newIgnoredVertexCount -= 1;
                        graph.RemoveVertex(neighbour);
                    }

                    // construct complementary graph
                    complementaryGraph.RemoveVertex(chosenVertex);
                    verticesAlreadyInMIS.Add(chosenVertex);
                    continueExecution = FindMIS(graph, complementaryGraph, verticesAlreadyInMIS, ignoredVertex, newIgnoredVertexCount, consumeAndDecideToContinue);
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
                    // TODO make extra checks here and there is it possible to satisfy an ignored vertex, if not STOP
                    if (continueExecution && graph.neighbourCount[chosenVertex] > 0)
                    {
                        ignoredVertex[chosenVertex] = true;
                        continueExecution = FindMIS(graph, complementaryGraph, verticesAlreadyInMIS, ignoredVertex, ignoredVertexCount + 1, consumeAndDecideToContinue);
                        ignoredVertex[chosenVertex] = false;
                    }
                }
            }
            return continueExecution;
        }

        //public IEnumerable<List<int>> EnumerateMIS(Graph graph)
        //{
        //    var primaryGraph = new GraphFast(graph);
        //    var complementaryGraph = new GraphFast(graph);
        //    var ignoredVertex = new bool[graph.VerticesKVPs.Length];
        //    var verticesAlreadyInMIS = new List<int>();
        //    foreach (var mis in FindMIS(primaryGraph, complementaryGraph, verticesAlreadyInMIS, ignoredVertex, 0))
        //    {
        //        yield return mis.vertices;
        //    }
        //}

        private Solution Recurse(int n, GraphFast primaryGraph, GraphFast complementaryGraph, Solution currentSolution, Solution bestSolution, int latestMisSize = int.MaxValue, int lastMisId = int.MaxValue)
        {
            if (complementaryGraph.vertexCount == 0)
            {
                if (currentSolution.colourCount < bestSolution.colourCount)
                {
                    bestSolution = currentSolution.DeepClone();
                    NewBestSolutionFound?.Invoke(null, new PerformanceReport()
                    {
                        minimalNumberOfColoursUsed = bestSolution.colourCount,
                        elapsedProcessorTime = stopwatch.Elapsed
                    });
                }
            }
            else if (currentSolution.colourCount + 1 < bestSolution.colourCount) // could improve this bound by finding a clique
            {
                // get MIS get and complimentary graph
                // compute subsolution
                var verticesAlreadyInMIS = new List<int>();
                currentSolution.colourCount += 1;
                var ignoredVertex = new bool[n];
                FindMIS(primaryGraph, complementaryGraph, verticesAlreadyInMIS, ignoredVertex, 0, mis =>
                {
                    // optionally check for cliques to delete
                    if (mis.vertices.Count < latestMisSize || (mis.vertices.Count == latestMisSize && mis.vertices[0] > lastMisId))// pre-optimization
                    {
                        currentSolution.colourLayer.Push(mis.vertices);
                        bestSolution = Recurse(n, complementaryGraph, complementaryGraph.DeepIrreversibleClone(), currentSolution, bestSolution, mis.vertices.Count, mis.vertices[0]);
                        currentSolution.colourLayer.Pop();
                    }
                    // need to fix the enumerator to ensure breakability
                    return currentSolution.colourCount < bestSolution.colourCount;
                });
                currentSolution.colourCount -= 1;
            }
            return bestSolution;

        }

    }
}
