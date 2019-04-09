using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Algorithms.GraphFactory
{
    public static class GraphFactory
    {
        private static int[][] DictionaryToArray(Dictionary<int, HashSet<int>> dictionary)
        {
            var array = new int[dictionary.Keys.Count][];
            foreach (var kvp in dictionary)
            {
                array[kvp.Key] = kvp.Value.ToArray();
            }
            return array;
        }

        private static void Connect(Dictionary<int, HashSet<int>> edges, int i, int j)
        {
            if (!edges.ContainsKey(i))
                edges.Add(i, new HashSet<int>());
            if (!edges.ContainsKey(j))
                edges.Add(j, new HashSet<int>());

            edges[i].Add(j);
            edges[j].Add(i);
        }

        public static Graph GeneratePetersenGraph()
        {
            var vertices = new HashSet<int>(Enumerable.Range(0, 10));
            var edges = new Dictionary<int, HashSet<int>>();
            for (int i = 0; i < 5; i += 1)
            {
                Connect(edges, i, (i + 1) % 5);
                Connect(edges, i, i + 5);
            }

            Connect(edges, 5, 7);
            Connect(edges, 5, 8);
            Connect(edges, 6, 8);
            Connect(edges, 6, 9);
            Connect(edges, 7, 9);

            return new Graph
            {
                VerticesKVPs = DictionaryToArray(edges)
            };
        }

        public static Graph GenerateGruenbaumsGraph()
        {
            var edges = new Dictionary<int, HashSet<int>>();

            Connect(edges, 0, 1);
            Connect(edges, 1, 2);
            Connect(edges, 2, 0);

            Connect(edges, 3, 4);
            Connect(edges, 4, 5);
            Connect(edges, 5, 3);

            Connect(edges, 0, 5);
            Connect(edges, 1, 5);

            Connect(edges, 1, 3);
            Connect(edges, 2, 3);

            Connect(edges, 0, 4);
            Connect(edges, 2, 4);

            return new Graph
            {
                VerticesKVPs = DictionaryToArray(edges)
            };
        }

        public static Graph Generate5RegularClebschGraph()
        {
            var vertices = new HashSet<int>(Enumerable.Range(0, 16));
            var edges = new Dictionary<int, HashSet<int>>();
            for (int i = 0; i < 5; i += 1)
            {
                Connect(edges, i, (i + 1) % 5);
                Connect(edges, i, i + 5);
            }

            Connect(edges, 5, 7);
            Connect(edges, 5, 8);
            Connect(edges, 6, 8);
            Connect(edges, 6, 9);
            Connect(edges, 7, 9);


            for (int i = 10; i < 15; i += 1)
            {
                Connect(edges, i, 15);
                Connect(edges, i, 5 + (i - 10));
                Connect(edges, i, 5 + ((i - 9) % 5));
                Connect(edges, i, (4 + (i - 10)) % 5);
                Connect(edges, i, (2 + (i - 10)) % 5);
            }

            return new Graph
            {
                VerticesKVPs = DictionaryToArray(edges)
            };
        }

        public static Graph GenerateCliquesConnectedByChain(int i, int j, int chainLength)
        {
            if (chainLength < 2)
                throw new Exception("The chain is too short, try at least 2 edges");

            var vertices = new HashSet<int>(Enumerable.Range(0, i + j + chainLength - 1));
            var edges = new Dictionary<int, HashSet<int>>();

            foreach (var vertex in vertices)
                edges.Add(vertex, new HashSet<int>());

            for (int i1 = 0; i1 < i; i1 += 1)
                for (int i1helper = 0; i1helper < i1; i1helper += 1)
                    Connect(edges, i1, i1helper);

            for (int j1 = i; j1 < i + j; j1 += 1)
                for (int j1helper = i; j1helper < j1; j1helper += 1)
                    Connect(edges, j1, j1helper);

            Connect(edges, 0, i + j);
            for (int chain = 0; chain < chainLength - 2; chain += 1)
                Connect(edges, i + j + chain, i + j + chain + 1);
            Connect(edges, i + j + chainLength - 2, i);

            return new Graph
            {
                VerticesKVPs = DictionaryToArray(edges)
            };
        }

        public static Graph GenerateCycle(int n)
        {
            var vertices = new HashSet<int>(Enumerable.Range(0, n));
            var neighbours = new Dictionary<int, HashSet<int>>
            {
                { 0, new HashSet<int>() { 1 } }
            };
            for (int i = 0; i < n - 1; i += 1)
            {
                neighbours[i].Add(i + 1);
                neighbours.Add(i + 1, new HashSet<int>() { i });
            }
            neighbours[n - 1].Add(0);
            neighbours[0].Add(n - 1);

            return new Graph
            {
                VerticesKVPs = DictionaryToArray(neighbours)
            };
        }


        public static Graph GenerateRandom(int n, double density, int generatingSeed)
        {
            var random = new Random(generatingSeed);
            var neighbours = new Dictionary<int, HashSet<int>>();
            for (int i = 0; i < n - 1; i += 1)
            {
                if (!neighbours.ContainsKey(i))
                {
                    neighbours.Add(i, new HashSet<int>());
                }
                for (int j = i + 1; j < n; j += 1)
                {
                    if (random.NextDouble() < density)
                    {
                        // add undirected edge
                        if (neighbours.ContainsKey(i))
                        {
                            neighbours[i].Add(j);
                        }
                        else
                        {
                            neighbours.Add(i, new HashSet<int> { j });
                        }

                        if (neighbours.ContainsKey(j))
                        {
                            neighbours[j].Add(i);
                        }
                        else
                        {
                            neighbours.Add(j, new HashSet<int> { i });
                        }
                    }
                }
            }

            return new Graph
            {
                VerticesKVPs = DictionaryToArray(neighbours)
            };
        }

        public static Graph GenerateRandom2(int n, double density, int generatingSeed)
        {
            var random = new Random(generatingSeed);
            var neighbours = new Dictionary<int, HashSet<int>>();
            var targetEdges = Math.Floor(n * (n - 1) / 2 * density);
            var edges = 0;
            for (int i = 0; i < n; i++)
            {
                    neighbours.Add(i, new HashSet<int>());
            }
            while (edges < targetEdges)
            {
                var i = random.Next(0, n);
                var j = random.Next(0, n);
                if (j == i)
                {
                    continue;
                }
                if (!neighbours[i].Contains(j))
                {
                    edges += 1;

                    // add undirected edge
                    neighbours[i].Add(j);
                    neighbours[j].Add(i);
                }
            }

            return new Graph
            {
                VerticesKVPs = DictionaryToArray(neighbours)
            };
        }
    }
}
