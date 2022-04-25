
namespace VehicleRouteProblem
{
    /// <summary>
    /// Algorithm to solve a VRP buildig all the routes gradually from the start
    /// </summary>
    internal class ConstructiveGreedyAlgorithm : Algorithm
    {
        /// <summary>
        /// Solves a VRP problem
        /// </summary>
        /// <param name="problem">Problem</param>
        /// <returns>Solution</returns>
        public Solution Solve(Problem problem)
        {
            List<int> available = new List<int>();
            int clientLimit = (int)(problem.clientCount() * 0.1) + problem.clientCount() / problem.vehicleCount - 2;
            for (int i = 1; i <= problem.clientCount(); i++) available.Add(i);
            List<int>[] routes = new List<int>[problem.vehicleCount];
            for (int i = 0; i < routes.Length; i++) routes[i] = new List<int> { 0 };
            int[,] bestNext = new int[problem.vehicleCount, 3];
            for (int i = 0; i < bestNext.GetLength(0); i++)
            {
                bestNext[i, 0] = 0;
                bestNext[i, 1] = int.MaxValue;
                bestNext[i, 2] = 0;
            }
            while (available.Count > 0)
            {
                for (int i = 0; i < bestNext.GetLength(0); i++)
                {
                    if (bestNext[i, 1] == int.MaxValue && bestNext[i, 2] < clientLimit)
                    {
                        bestNext[i, 0] = available[0];
                        bestNext[i, 1] = problem.getDistance(routes[i][routes[i].Count - 1], bestNext[i, 0]);
                        for (int j = 0; j < available.Count; j++)
                        {
                            int candidateDistance = problem.getDistance(routes[i][routes[i].Count - 1], available[j]);
                            if (bestNext[i, 1] > candidateDistance)
                            {
                                bestNext[i, 0] = available[j];
                                bestNext[i, 1] = candidateDistance;
                            }
                        }
                    }
                }
                int grows = 0;
                for (int i = 1; i < bestNext.GetLength(0); i++)
                    if (bestNext[grows, 1] > bestNext[i, 1]) grows = i;
                int explored = bestNext[grows, 0];
                ++bestNext[grows, 2];
                routes[grows].Add(explored);
                available.Remove(explored);
                for (int i = 0; i < bestNext.GetLength(0); i++)
                    if (bestNext[i, 0] == explored) bestNext[i, 1] = int.MaxValue;
            }
            for (int i = 0; i < routes.Length; i++) routes[i].Add(0);
            int[] costs = new int[routes.Length];
            for (int i = 0; i < routes.Length; i++) costs[i] = problem.getRouteCost(routes[i]);
            return new Solution(routes, costs);
        }
    }
}
