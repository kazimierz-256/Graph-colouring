using Algorithms.Solver;
using System;
using System.Collections.Generic;
using System.Linq;
using static Algorithms.Graph;

namespace Algorithms
{

    public class RecurseLargestFirst
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

        public Dictionary<int, int> ColourGraph(Graph graph)
        {
            var dummySolution1 = new Solution()
            {
                colourCount = 0,
                vertexToColour = new int[graph.VerticesKVPs.Length]
            };

            for (int i = 0; i < graph.VerticesKVPs.Length; i++)
            {
                dummySolution1.vertexToColour[i] = -1;
            }

            int upperBoundOnNumberOfColours = 0;
            int UpperChromaticNumber = 4;

            while (!IsPossible(graph, dummySolution1, UpperChromaticNumber))
            {
                UpperChromaticNumber = 2 * UpperChromaticNumber;
            }

            int DownChromaticNumber = UpperChromaticNumber / 2;
            bool IsRight = true;
            if (IsPossible(graph, dummySolution1, UpperChromaticNumber - 1) == false)
            {
                IsRight = false;
                dummySolution1 = Recurse(graph, dummySolution1, upperBoundOnNumberOfColours);

            }
            while (IsRight)
            {

                upperBoundOnNumberOfColours = (UpperChromaticNumber + DownChromaticNumber) / 2;

                if (IsPossible(graph, dummySolution1, upperBoundOnNumberOfColours) == true && IsPossible(graph, dummySolution1, upperBoundOnNumberOfColours - 1) == false)
                {
                    dummySolution1 = Recurse(graph, dummySolution1, upperBoundOnNumberOfColours);

                    break;
                }

                if (IsPossible(graph, dummySolution1, upperBoundOnNumberOfColours) == true && IsPossible(graph, dummySolution1, upperBoundOnNumberOfColours - 1) == true)
                    UpperChromaticNumber = (upperBoundOnNumberOfColours);
                else
                    DownChromaticNumber = (upperBoundOnNumberOfColours);
            }



            var solution = dummySolution1.vertexToColour;
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
            if (solution.colourCount > maxColour) return false;
            while (!IsGraphColored(solution))
            {
                BestOption = ChooseSuitableVertex(graph, solution);
                solution = ChooseBestNeighbour(graph, solution, BestOption, BorderValue);
                if (solution.colourCount > maxColour) return false;
            }
            return true;
        }


        //To w dokuentacji to algorytm b
        private Solution Recurse(Graph graphToColour, Solution currentSolution, int PossibleColoring)
        {
            double n = graphToColour.VerticesKVPs.Length;
            double BorderValue = Math.Pow(n, (1 - 1 / (n - 1)));
            var BestOption = ChooseSuitableVertex(graphToColour, currentSolution);

            Solution solution = ChooseBestNeighbour(graphToColour, currentSolution, BestOption, BorderValue);

            while (!IsGraphColored(solution))
            {
                BestOption = ChooseSuitableVertex(graphToColour, solution);
                solution = ChooseBestNeighbour(graphToColour, solution, BestOption, BorderValue);
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
                    if (currentSolution.vertexToColour[i] == -1 && currentSolution.vertexToColour[graph.VerticesKVPs[i][j]] == -1)
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
            Solution solution = currentSolution.DeepClone();


            if (BestOption == -1)
            {
                int colourCandidate = 1;
                for (int i = 0; i < graph.VerticesKVPs.Length; i++)
                {
                    colourCandidate = 1;
                    if (solution.vertexToColour[i] == -1)
                    {
                        while (!EnsureAcyclicityAndValidity(graph, i, colourCandidate, solution))
                        {
                            colourCandidate++;
                        }
                        solution.vertexToColour[i] = colourCandidate;

                        if (colourCandidate > solution.colourCount)
                        {
                            solution.colourCount = colourCandidate;
                        }

                    }
                }
                return solution;
            }

            var maxNeighbourCount = 0;
            var count = -1;
            var BestNeighbour = -1;

            List<int> Temp = new List<int>();
            for (int i = 0; i < graph.VerticesKVPs[BestOption].Length; i++) //znajduje niepokolorowanych sasiadow
            {
                if (solution.vertexToColour[graph.VerticesKVPs[BestOption][i]] == -1)
                {
                    Temp.Add(graph.VerticesKVPs[BestOption][i]);
                }
            }

            int[] PossibleNeighours = Temp.ToArray();

            if (PossibleNeighours.Length < borderValue)
            {
                solution = BruteForce(graph, PossibleNeighours, BestOption, solution);
                return solution;
            }

            do
            {
                maxNeighbourCount = 0;
                for (int i = 0; i < PossibleNeighours.Length; i++)
                {
                    count = 0;
                    for (int j = 0; j < PossibleNeighours.Length; j++)
                    {
                        if (IsNeighbour(graph, solution, PossibleNeighours[i], PossibleNeighours[j]))
                        {
                            count++;
                            if (count > maxNeighbourCount)
                            {
                                maxNeighbourCount = count;
                                BestNeighbour = PossibleNeighours[i];
                            }
                        }
                    }
                }

                if (BestNeighbour != -1)
                {
                    PossibleNeighours = PossibleNeighours.Intersect(graph.VerticesKVPs[BestNeighbour]).ToArray();
                }
                else BestNeighbour = BestOption;
            }
            while (maxNeighbourCount > borderValue);
            solution = BruteForce(graph, PossibleNeighours, BestNeighbour, solution);


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
            int smallestColour = 1;

            while (!EnsureAcyclicityAndValidity(graph, center, smallestColour, solution))
            {
                smallestColour++;
            }

            solution.vertexToColour[center] = smallestColour;
            if (smallestColour > solution.colourCount)
            {
                solution.colourCount = smallestColour;
            }
            smallestColour = 1;

            for (int i = 0; i < vertexsToColour.Length; i++)
            {
                if (EnsureAcyclicityAndValidity(graph, vertexsToColour[i], smallestColour, solution))
                {
                    solution.vertexToColour[vertexsToColour[i]] = smallestColour;
                    if (solution.colourCount < smallestColour)
                    {
                        solution.colourCount = smallestColour;
                    }
                    smallestColour = 1;
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
            var coloursOccupiedByNeighbours = new int[currentSolution.colourCount + 1];
            var neighboursToConsider = new int[graph.VerticesKVPs[vertex].Length];
            var neighbourToConsiderCount = 0;

            foreach (var neighbour in graph.VerticesKVPs[vertex])
            {
                var colour = currentSolution.vertexToColour[neighbour];
                if (colour != -1)
                {
                    if (colour == colourCandidate)
                        return false;
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