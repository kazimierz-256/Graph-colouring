using Algorithms.Solver;
using System;
using System.Collections.Generic;
using System.Linq;
using static Algorithms.Graph;

namespace Algorithms
{

    class RecurseLargestFirst
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

        //public Dictionary<int, int> ColourGraph(Graph graph) => ColourGraph(graph);

        public Dictionary<int, int> ColourGraph(Graph graph)
        {
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
            int upperBoundOnNumberOfColours = 5;
            int UpperChromaticNumber = 2;

            while (!IsPossible(graph, dummySolution, UpperChromaticNumber))
            {
                UpperChromaticNumber = 2 * UpperChromaticNumber;
            }

            int DownChromaticNumber = UpperChromaticNumber / 2;
            bool IsRight = true;
            if (IsPossible(graph, dummySolution, UpperChromaticNumber - 1) == false)
            {
                IsRight = false;
                upperBoundOnNumberOfColours = UpperChromaticNumber;
            }
            while (IsRight)
            {
                upperBoundOnNumberOfColours = (UpperChromaticNumber + DownChromaticNumber) / 2;
                if (IsPossible(graph, dummySolution, upperBoundOnNumberOfColours) == true && IsPossible(graph, dummySolution, upperBoundOnNumberOfColours - 1) == false)
                    IsRight = false;
                else if (IsPossible(graph, dummySolution, upperBoundOnNumberOfColours) == true && IsPossible(graph, dummySolution, upperBoundOnNumberOfColours - 1) == true)
                    DownChromaticNumber = (UpperChromaticNumber + upperBoundOnNumberOfColours) / 2;
                //else if (IsPossible(graph, dummySolution, upperBoundOnNumberOfColours) == true)
                else
                    UpperChromaticNumber = (DownChromaticNumber + upperBoundOnNumberOfColours) / 2;
            }



            var solution = Recurse(graph, initialSolution, upperBoundOnNumberOfColours).vertexToColour;
            var dictionary = new Dictionary<int, int>();
            for (int i = 0; i < solution.Length; i++)
            {
                dictionary.Add(i, solution[i]);
            }
            return dictionary;
        }
        //Alg c
        private bool IsPossible(Graph graph, Solution currentSolution, int maxColour)
        {
            double n = maxColour;
            double BorderValue = Math.Pow(n, (1 - 1 / (n - 1)));
            var BestOption = ChooseSuitableVertex(graph, currentSolution);
            Solution solution = ChooseBestNeighbour(graph, currentSolution, BestOption, BorderValue);

            while (!IsGraphColored(currentSolution))
            {
                BestOption = ChooseSuitableVertex(graph, currentSolution);
                solution = ChooseBestNeighbour(graph, currentSolution, BestOption, BorderValue);
                if (solution.colourCount > maxColour) return false;
            }
            return true;
        }


        //To w dokuentacji to algorytm b
        private Solution Recurse(Graph graphToColour, Solution currentSolution, int PossibleColoring)
        {
            double n = PossibleColoring;
            double BorderValue = Math.Pow(n, (1 - 1 / (n - 1)));
            var BestOption = ChooseSuitableVertex(graphToColour, currentSolution);

            Solution solution = ChooseBestNeighbour(graphToColour, currentSolution, BestOption, BorderValue);

            while (!IsGraphColored(currentSolution))
            {
                BestOption = ChooseSuitableVertex(graphToColour, currentSolution);
                solution = ChooseBestNeighbour(graphToColour, currentSolution, BestOption, BorderValue);
            }

            return solution;
        }

        private bool IsGraphColored(Solution solution)
        {
            for (int i = 0; i < solution.vertexToColour.Length; i++)
            {
                if (solution.vertexToColour[i] == -1)
                    return false;
            }
            return true;
        }
        private int ChooseSuitableVertex(Graph graph, Solution currentSolution)
        {
            var maxNeighbourCount = -1;
            var maxVertex = -1;
            //bestColouring = null;
            var count = -1;

            for (int i = 0; i < graph.VerticesKVPs.Length; i++)
            {
                count = 0;
                for (int j = 0; j < graph.VerticesKVPs[i].Length; j++)
                {
                    if (currentSolution.vertexToColour[i] == -1 && currentSolution.vertexToColour[j] == -1)
                    {
                        count++;
                        if (count > maxNeighbourCount)
                        {
                            maxVertex = i;
                            maxNeighbourCount = count;
                        }
                    }
                }
            }

            return maxVertex;
        }

        private Solution ChooseBestNeighbour(Graph graph, Solution currentSolution, int BestOption, double borderValue)
        {
            Solution solution = currentSolution;
            var maxNeighbourCount = 0;
            var maxVertex = -1;
            //bestColouring = null;
            var count = -1;
            var BestNeighbour = -1;
            // var Temp = graph.VerticesKVPs[BestOption];  //.Intersect(graph.VerticesKVPs[i]);
            List<int> Temp = new List<int>();
            for (int i = 0; i < graph.VerticesKVPs[BestOption].Length; i++) //znajduje niepokolorowanych sasiadow
            {
                if (currentSolution.vertexToColour[graph.VerticesKVPs[BestOption][i]] == -1)
                {
                    Temp.Add(graph.VerticesKVPs[BestOption][i]);
                }
            }

            int[] PossibleNeighours = Temp.ToArray();

            do
            {
                maxNeighbourCount = 0;
                for (int i = 0; i < PossibleNeighours.Length; i++)
                {
                    count = 0;
                    for (int j = 0; j < PossibleNeighours.Length; j++)
                    {
                        if (IsNeighbour(graph, currentSolution, PossibleNeighours[i], PossibleNeighours[j]))
                        {
                            count++;
                            if (count > maxNeighbourCount)
                            {
                                maxNeighbourCount = count;
                                BestNeighbour = j;
                            }
                        }
                    }
                    //[0].Intersect(matrix[1]);
                }
                //PossibleNeighours = PossibleNeighours.Intersect(graph.VerticesKVPs[BestNeighbour]);
                PossibleNeighours = PossibleNeighours.Intersect(graph.VerticesKVPs[BestNeighbour]).ToArray();
            }
            while (maxNeighbourCount >= borderValue);
            solution = BruteForce(graph, PossibleNeighours, BestNeighbour, solution);
            // while (BestVertexNeighbers(graph, currentSolution, BestOption, neighbours) >)

            return solution;
        }

        private bool IsNeighbour(Graph graph, Solution solution, int a, int b)
        {
            for (int i = 0; i < graph.VerticesKVPs[a].Length; i++)
            {
                if (graph.VerticesKVPs[a][i] == b)
                    return true;
            }
            return false;
        }

        private Solution BruteForce(Graph graph, int[] vertexsToColour, int center, Solution solution)
        {
            int smallestColour = 0;

            while (!EnsureAcyclicityAndValidity(graph, center, smallestColour, solution))
            {
                smallestColour++;
            }

            solution.vertexToColour[center] = smallestColour;

            for (int i = 0; i < vertexsToColour.Length; i++)
            {
                smallestColour = 0;
                if (EnsureAcyclicityAndValidity(graph, vertexsToColour[i], smallestColour, solution)) //tutaj skonczylem
                {
                    solution.vertexToColour[vertexsToColour[i]] = smallestColour;
                    if (solution.colourCount < smallestColour)
                    {
                        solution.colourCount = smallestColour;
                    }
                    smallestColour = 0;
                }
                else
                {
                    smallestColour++;
                    i--;
                }

            }

            return solution;

        }

        private bool EnsureAcyclicityAndValidity(Graph graph, int vertex, int colourCandidate, Solution currentSolution)
        {
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
                                if (FoundCycleBicolourable(graph, neighbour2, vertex, colourCandidate, complementaryColour, currentSolution, exploredVertices))
                                    return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        private bool FoundCycleBicolourable(Graph graph, int alreadyConsidered, int vertex, int colourCandidate, int complementaryColour, Solution currentSolution, HashSet<int> exploredVertices)
        {
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
                        if (FoundCycleBicolourable(graph, neighbour, vertex, colourCandidate, complementaryColour, currentSolution, exploredVertices))
                            return true;
                    }
                    // looking for even
                    else if (exploredVertices.Count % 2 == 1 && currentSolution.vertexToColour[neighbour] == complementaryColour)
                    {
                        if (FoundCycleBicolourable(graph, neighbour, vertex, colourCandidate, complementaryColour, currentSolution, exploredVertices))
                            return true;
                    }
                    exploredVertices.Remove(neighbour);
                }
            }
            return false;
        }



    }
}