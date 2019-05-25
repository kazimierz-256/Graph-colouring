using Algorithms;
using System;

namespace colGraphReader
{
    public static class Loader
    {
        public static string[] GetFullText(string graphName) => System.IO.File.ReadAllLines($@"../../../../LoaderParser/Problems/{graphName}.vrp");
        public static string[] GetFullText2(string graphName) => System.IO.File.ReadAllLines($@"{graphName}.vrp");

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
            var problem = new SolvedProblem();
            var listedPoints = 0;
            var analyzingLineNumber = 0;
            var coordSection = 0;
            var demandSection = 0;
            foreach (var dirtyLine in lines)
            {
                analyzingLineNumber += 1;

                var line = dirtyLine.Trim();
                if (line.StartsWith("COMMENT"))
                {
                    var lastColonIndex = line.LastIndexOf(":");
                    problem.optimalSolution = double.Parse(
                        line.Substring(
                            lastColonIndex + 1,
                            line.LastIndexOf(")") - (lastColonIndex + 1)
                        ).Trim()
                        );
                    var firstColon = line.IndexOf(":");
                    var lineToParseTrucks = line.Substring(firstColon + 1, lastColonIndex - (firstColon + 1));
                    problem.optimalVehicleCount = int.Parse(
                        lineToParseTrucks.Substring(
                            lineToParseTrucks.LastIndexOf(":") + 1,
                            lineToParseTrucks.LastIndexOf(",") - (lineToParseTrucks.LastIndexOf(":") + 1)
                        ).Trim()
                        );

                }
                else if (line.StartsWith("DIMENSION"))
                {
                    listedPoints = int.Parse(
                        line.Substring(
                            line.LastIndexOf(":") + 1
                        ).Trim()
                        );
                }
                else if (line.StartsWith("CAPACITY"))
                {
                    problem.vehicleCapacity = int.Parse(
                        line.Substring(
                            line.LastIndexOf(":") + 1
                        ).Trim()
                        );
                }
                else if (line.StartsWith("NODE_COORD_SECTION"))
                {
                    coordSection = analyzingLineNumber;
                }
                else if (line.StartsWith("DEMAND_SECTION"))
                {
                    demandSection = analyzingLineNumber;
                }
            }

            if (lines[coordSection].TrimStart().StartsWith("1 "))
            {
                var cleanedBaseLine = lines[coordSection].Trim().Split(' ');
                problem.baseXcoordinate = int.Parse(cleanedBaseLine[1]);
                problem.baseYcoordinate = int.Parse(cleanedBaseLine[2]);
                problem.demandPointsWithoutBase = new List<Problem.DemandPoint>();
                for (int i = 1; i < listedPoints; i++)
                {
                    var cleanedLine = lines[coordSection + i].Trim().Split(' ');
                    var demandLine = lines[demandSection + i].Trim().Split(' ');
                    if (cleanedLine[0] == demandLine[0] && int.Parse(cleanedLine[0]) == i + 1)
                    {

                        problem.demandPointsWithoutBase.Add(new Problem.DemandPoint()
                        {
                            x = int.Parse(cleanedLine[1]),
                            y = int.Parse(cleanedLine[2]),
                            demand = int.Parse(demandLine[1])
                        });
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            else
            {
                return null;
            }

            return problem;
        }
    }
}
