using System;
using System.Collections.Generic;
using System.Text;

namespace Algorithms.Solver
{
    interface ISolver
    {
        Dictionary<int, int> ColourGraph(Graph graph);
    }
}
