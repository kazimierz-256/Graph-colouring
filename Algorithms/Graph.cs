using System;
using System.Collections.Generic;
using System.Linq;

namespace Algorithms
{
    // undirected graph without loops
    public class Graph
    {
        public int[][] VerticesKVPs;



        public Graph CloneWithoutSmallestVertex(Func<int, bool> proceedRemovingDegree)
        {
            var smallestDegree = int.MaxValue;
            var smallestId = -1;
            for (int i = 0; i < VerticesKVPs.Length; i++)
            {
                if (VerticesKVPs[i].Length < smallestDegree)
                {
                    smallestDegree = VerticesKVPs[i].Length;
                    smallestId = i;
                }
            }
            if (proceedRemovingDegree(smallestDegree))
            {
                return CloneWithoutVertex(smallestId).CloneWithoutSmallestVertex(proceedRemovingDegree);
            }
            else
            {
                return this;
            }
        }

        public Graph CloneWithoutVertex(int vertex)
        {
            var newGraph = new Graph();
            newGraph.VerticesKVPs = new int[VerticesKVPs.Length - 1][];

            for (int i = 0; i < VerticesKVPs.Length; i++)
            {
                if (i == vertex)
                {
                    continue;
                }
                var targetIndex = i > vertex ? i - 1 : i;
                newGraph.VerticesKVPs[targetIndex] = VerticesKVPs[i].Where(v => v != vertex).Select(v => v > vertex ? v - 1 : v).ToArray().Clone() as int[];
            }
            return newGraph;
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