using System.Diagnostics;

namespace VehicleRouteProblem
{
    /// <summary>
    /// Programa principal
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Returns data as a string organized in a table
        /// </summary>
        /// <param name="data">Contents of the table</param>
        /// <param name="rowNames">First column content</param>
        /// <param name="columnNames">Top row content</param>
        /// <returns>Table</returns>
        static public string PrintTable(string[,] data, string[] rowNames, string[] columnNames)
        {
            int maxtablelength = 0;
            for (int i = 0; i < data.GetLength(0); ++i)
                for (int j = 0; j < data.GetLength(1); ++j)
                    if (data[i, j].Length > maxtablelength) maxtablelength = data[i, j].Length;
            for (int i = 0; i < rowNames.Length; ++i)
                if (rowNames[i].Length > maxtablelength) maxtablelength = rowNames[i].Length;
            for (int i = 0; i < columnNames.Length; ++i)
                if (columnNames[i].Length > maxtablelength) maxtablelength = columnNames[i].Length;
            ++maxtablelength;
            string table = new string(' ', maxtablelength);
            for (int i = 0; i < columnNames.Length; ++i)
                table += new string(' ', maxtablelength - columnNames[i].Length) + columnNames[i];
            for (int i = 0; i < data.GetLength(0); ++i)
            {
                table += '\n';
                if (i < rowNames.Length) table += rowNames[i] + new string(' ', maxtablelength - rowNames[i].Length);
                else table += new string(' ', maxtablelength);
                for (int j = 0; j < data.GetLength(1); ++j)
                    table += new string(' ', maxtablelength - data[i, j].Length) + data[i, j];
            }
            return table;
        }

        /// <summary>
        /// Programa principal
        /// </summary>
        public static void Main()
        {
            string[] files =
            {
                "I40j_2m_S1_1.txt",
                "I40j_4m_S1_1.txt",
                "I40j_6m_S1_1.txt",
                "I40j_8m_S1_1.txt"
            };
            Problem[] problems = new Problem[files.Length];
            for (int i = 0; i < problems.Length; i++) problems[i] = new Problem(files[i]);
            Stopwatch timer = new Stopwatch();

            Algorithm[] greedyAlgorithms = new Algorithm[] {
                new HalfRoutesGreedyAlgorithm(),
                new ConstructiveGreedyAlgorithm(),
            };
            string[][,] greedyData = new string[][,] { new string[problems.Length, 3], new string[problems.Length, 3] };
            string[] greedyColumns = new string[] { "Size", "Solution Cost", "Solution Time" };
            for (int i = 0; i < greedyAlgorithms.Length; i++)
                for (int j = 0; j < problems.Length; j++)
                {
                    timer.Restart();
                    Solution solution = greedyAlgorithms[i].Solve(problems[j]);
                    timer.Stop();
                    greedyData[i][j, 0] = $"{problems[j].clientCount()} ({problems[j].vehicleCount})";
                    greedyData[i][j, 1] = $"{solution.totalCost}";
                    greedyData[i][j, 2] = $"{timer.ElapsedMilliseconds} ms";
                }
            Console.WriteLine("\n  GREEDY ALGORITHMS: ");
            Console.WriteLine("\n\nHalfRoutes Greedy: \n" + PrintTable(greedyData[0], files, greedyColumns));
            Console.WriteLine("\n\nConstructive Greedy: \n" + PrintTable(greedyData[1], files, greedyColumns));

            GRASPAlgorithm[] GRASPAlgorithms = new GRASPAlgorithm[] {
                new GRASPAlgorithm("reinsertion"),
                new GRASPAlgorithm("intra reinsertion"),
                new GRASPAlgorithm("inter reinsertion"),
                new GRASPAlgorithm("exchange"),
                new GRASPAlgorithm("intra exchange"),
                new GRASPAlgorithm("inter exchange"),
                new GRASPAlgorithm("2-opt"),
                new GRASPAlgorithm("gvns"),
            };
            string[][,] GRASPData = new string[GRASPAlgorithms.Length][,];
            for (int i = 0; i < GRASPData.Length; i++) GRASPData[i] = new string[problems.Length, 4];
            string[] GRASPColumns = new string[] { "Size", "LRC Size", "Solution Cost", "Solution Time" };
            for (int i = 0; i < GRASPAlgorithms.Length; i++)
                for (int j = 0; j < problems.Length; j++)
                {
                    timer.Restart();
                    Solution solution = GRASPAlgorithms[i].Solve(problems[j]);
                    timer.Stop();
                    GRASPData[i][j, 0] = $"{problems[j].clientCount()} ({problems[j].vehicleCount})";
                    GRASPData[i][j, 1] = $"{GRASPAlgorithms[i].RCL_SIZE}";
                    GRASPData[i][j, 2] = $"{solution.totalCost}";
                    GRASPData[i][j, 3] = $"{timer.ElapsedMilliseconds} ms";
                }
            Console.WriteLine("\n\n\n  GRASP ALGORITHMS:");
            Console.WriteLine("\n\nGRASP (reinsertion): \n" + PrintTable(GRASPData[0], files, GRASPColumns));
            Console.WriteLine("\n\nGRASP (intra reinsertion): \n" + PrintTable(GRASPData[1], files, GRASPColumns));
            Console.WriteLine("\n\nGRASP (inter reinsertion): \n" + PrintTable(GRASPData[2], files, GRASPColumns));
            Console.WriteLine("\n\nGRASP (exchange): \n" + PrintTable(GRASPData[3], files, GRASPColumns));
            Console.WriteLine("\n\nGRASP (intra exchange): \n" + PrintTable(GRASPData[4], files, GRASPColumns));
            Console.WriteLine("\n\nGRASP (inter exchange): \n" + PrintTable(GRASPData[5], files, GRASPColumns));
            Console.WriteLine("\n\nGRASP (2-opt): \n" + PrintTable(GRASPData[6], files, GRASPColumns));
            Console.WriteLine("\n\nGRASP (gvns): \n" + PrintTable(GRASPData[7], files, GRASPColumns));
        }
    }
}