
namespace VehicleRouteProblem
{
    /// <summary>
    /// Algoritmo Voraz de medias rutas (construye simultáneamente rutas de ida y de vuelta y las conecta al final)
    /// </summary>
    internal class HalfRoutesGreedyAlgorithm : Algorithm
    {
        /// <summary>
        /// Resuelve un problema VRP
        /// </summary>
        /// <param name="problem">Problema</param>
        /// <returns>Solución</returns>
        public Solution Solve(Problem problem)
        {
            List<int>[] halfRoutes = createHalfRoutes(problem);
            List<int>[] routes = new List<int>[problem.vehicleCount];
            int[] costs = new int[problem.vehicleCount];
            bool[] available = new bool[problem.vehicleCount];
            for (int i = 0; i < problem.vehicleCount; i++) available[i] = true;
            for (int i = 0; i < problem.vehicleCount; i++)
            {
                routes[i] = new List<int>(halfRoutes[i * 2]);
                bool first = true;
                int distance = int.MaxValue;
                int candidate = 1;
                for (int j = 0; j < problem.vehicleCount; j++)
                {
                    int newDistance = problem.getRouteCost(routes[i]) + problem.getRouteCost(halfRoutes[j * 2 + 1]) + problem.getDistance(routes[i][routes[i].Count - 1], halfRoutes[j * 2 + 1][0]);
                    if (available[j] && (first || distance < newDistance))
                    {
                        distance = newDistance;
                        candidate = j * 2 + 1;
                        first = false;
                    }
                }
                costs[i] = distance;
                for (int j = 0; j < halfRoutes[candidate].Count; j++)
                    routes[i].Add(halfRoutes[candidate][j]);
                available[(candidate - 1) / 2] = false;
            }
            return new Solution(routes, costs);
        }

        /// <summary>
        /// Crea las medias rutas a partir del problema
        /// </summary>
        /// <param name="problem">Problema</param>
        /// <returns>Rutas parciales (pares de ida, impares de vuelta)</returns>
        private List<int>[] createHalfRoutes(Problem problem)
        {
            List<int>[] halfRoutes = new List<int>[problem.vehicleCount * 2];
            for (int i = 0; i < halfRoutes.Length; i++) {
                halfRoutes[i] = new List<int>();
                halfRoutes[i].Add(0);
            }
            List<int> clients = new List<int>();
            for (int i = 1; i <= problem.clientCount(); i++) {
                clients.Add(i);
            }
            for (int i = 0; clients.Count > 0; i++) {
                for (int j = 0; j < halfRoutes.Length && clients.Count > 0; j++)
                {
                    int candidate = clients[0];
                    int currentDistance = j % 2 == 0 ? 
                        problem.getDistance(halfRoutes[j][i], candidate) : problem.getDistance(candidate, halfRoutes[j][i]);
                    for (int k = 1; k < clients.Count; k++)
                    {
                        int newDistance = j % 2 == 0 ? 
                            problem.getDistance(halfRoutes[j][i], clients[k]) : problem.getDistance(clients[k], halfRoutes[j][i]);
                        if (newDistance < currentDistance)
                        {
                            candidate = clients[k];
                            currentDistance = newDistance;
                        }
                    }
                    clients.Remove(candidate);
                    if (j % 2 == 0) halfRoutes[j].Add(candidate);
                    else halfRoutes[j].Insert(0, candidate);
                }
            }
            return halfRoutes;
        }
    }
}
