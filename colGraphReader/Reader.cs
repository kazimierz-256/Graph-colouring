using Algorithms;
using Algorithms.GraphFactory;
using System;
using System.Collections.Generic;

namespace colGraphReader
{
    public static class Reader
    {
        private static string[] GetFullText(string graphName) => System.IO.File.ReadAllLines($@"../../../../{graphName}.col");
        private static string[] GetFullText2(string graphName) => System.IO.File.ReadAllLines($@"{graphName}.col");

        public static Graph ParseGraph(string graphName)
        {
            string[] lines;
            try
            {
                lines = GetFullText(graphName);
            }
            catch (Exception)
            {
                lines = GetFullText2(graphName);
            }

            var edges = new Dictionary<int, HashSet<int>>();

            foreach (var line in lines)
            {
                if (line.Length > 1 && line[0] == 'e')
                {
                    var splitted = line.Split(' ');
                    var vertex1 = int.Parse(splitted[1]) - 1;
                    var vertex2 = int.Parse(splitted[2]) - 1;

                    if (!edges.ContainsKey(vertex1))
                        edges.Add(vertex1, new HashSet<int>());
                    if (!edges.ContainsKey(vertex2))
                        edges.Add(vertex2, new HashSet<int>());

                    edges[vertex1].Add(vertex2);
                    edges[vertex2].Add(vertex1);
                }
            }

            return new Graph()
            {
                graphName = graphName,
                VerticesKVPs = GraphFactory.DictionaryToArray(edges)
            };
        }
    }
}
