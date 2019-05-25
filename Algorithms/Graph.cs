using System;
using System.Collections.Generic;
using System.Linq;

namespace Algorithms
{
    // undirected graph without loops
    public class Graph
    {
        public string graphName = string.Empty;
        public int[][] VerticesKVPs;


        public override string ToString()
        {
            if (graphName == string.Empty)
            {
                return $"unnamed-{VerticesKVPs.Length}-{VerticesKVPs.Sum(vertex => vertex.Count()) / 2}";
            }
            else
            {
                return $"{graphName}-{VerticesKVPs.Length}-{VerticesKVPs.Sum(vertex => vertex.Count()) / 2}";
            }
        }

        public Graph CloneNeighboursSorted(Func<int, int> evaluator)
        {
            var newGraph = new Graph();
            newGraph.VerticesKVPs = new int[VerticesKVPs.Length][];
            for (int i = 0; i < VerticesKVPs.Length; i++)
            {
                newGraph.VerticesKVPs[i] = VerticesKVPs[i].Clone() as int[];
                var valuation = new int[VerticesKVPs[i].Length];
                for (int j = 0; j < VerticesKVPs[i].Length; j++)
                {
                    valuation[j] = evaluator(VerticesKVPs[i][j]);
                }
                Array.Sort(valuation, newGraph.VerticesKVPs[i]);
            }
            return newGraph;
        }

        public Graph CloneNeighboursRandomly()
        {
            var random = new Random(0);
            var newGraph = new Graph();
            newGraph.VerticesKVPs = new int[VerticesKVPs.Length][];
            for (int i = 0; i < VerticesKVPs.Length; i++)
            {
                newGraph.VerticesKVPs[i] = VerticesKVPs[i].Clone() as int[];
                var valuation = new int[VerticesKVPs[i].Length];
                for (int j = 0; j < VerticesKVPs[i].Length; j++)
                {
                    valuation[j] = random.Next();
                }
                Array.Sort(valuation, newGraph.VerticesKVPs[i]);
            }
            return newGraph;
        }
    }
}