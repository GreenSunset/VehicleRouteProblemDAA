
namespace VehicleRouteProblem
{
    /// <summary>
    /// Clase para el algoritmo GRASP (incompleto)
    /// </summary>
    internal class GRASPAlgorithm : Algorithm
    {
        /// <summary>
        /// Tamaño de la Lista restringida de candidatos (No usada por el momento)
        /// </summary>
        public int RCL_SIZE = 10;
        private string searchMethod;
        private bool anxious;
        private bool randomConstructive;
        private SolutionEnvironment[] searchEnvironments;

        public GRASPAlgorithm(string searchMethod = "reinsertion", bool anxiousSearch = false, bool randomConstructivePhase = true)
        {
            switch (searchMethod)
            {
                case "reinsertion":
                    searchEnvironments = new ReinsertionEnvironment[] { new("full") };
                    break;
                case "intra reinsertion":
                    searchEnvironments = new ReinsertionEnvironment[] { new("intra") };
                    break;
                case "inter reinsertion":
                    searchEnvironments = new ReinsertionEnvironment[] { new("inter") };
                    break;
                case "exchange":
                    searchEnvironments = new ExchangeEnvironment[] { new("full") };
                    break;
                case "intra exchange":
                    searchEnvironments = new ExchangeEnvironment[] { new("intra") };
                    break;
                case "inter exchange":
                    searchEnvironments = new ExchangeEnvironment[] { new("inter") };
                    break;
                case "2-opt":
                    searchEnvironments = new TwoOptEnvironment[] { new() };
                    break;
                case "gvns":
                    searchEnvironments = new SolutionEnvironment[] { new ReinsertionEnvironment("intra"), new ReinsertionEnvironment("inter"), new ExchangeEnvironment("intra"), new ExchangeEnvironment("inter"), new TwoOptEnvironment() };
                    break;
                default:
                    throw new ArgumentException("Loacal search method not recognized");
            }
            randomConstructive = randomConstructivePhase;
            this.searchMethod = searchMethod;
            anxious = anxiousSearch;
        }

        /// <summary>
        /// Resuelve un problema VRP aplicando el algoritmo GRASP (incompleto)
        /// </summary>
        /// <param name="problem">Problema</param>
        /// <returns>Solución</returns>
        public Solution Solve(Problem problem)
        {
            List<int> RestrictedCandidateList = PreProcessing(problem);
            PartialSolution bestSolution = new();
            int count = 0;
            do
            {
                PartialSolution currentSolution = new(problem);
                BuildSolution(currentSolution, RestrictedCandidateList);
                if (searchMethod == "gvns") GVNS(ref currentSolution);
                else SimpleSearch(currentSolution);
                UpdateBestSolution(currentSolution, ref bestSolution);
                count++;
            } while (count < 2000);
            return bestSolution.turnToSolution();
        }

        /// <summary>
        /// Procesos anteriores a la ejecución del algoritmo. Actualmente crea la RCL
        /// </summary>
        /// <param name="problem">Problema</param>
        /// <returns>Lista Restringida de Candidatos</returns>
        private List<int> PreProcessing(Problem problem)
        {
            List<int> RCL = new();
            List<int> available = new();
            for (int i = 1; i <= problem.clientCount(); i++) available.Add(i);
            int RCLLength = problem.vehicleCount * 2;
            if (RCLLength > available.Count) RCLLength = available.Count;
            RCL_SIZE = RCLLength;
            for (int i = 0; i < RCLLength; i++)
            {
                int candidate = available[0];
                int candidateDistance = problem.getDistance(0, candidate);
                for (int j = 1; j < available.Count; j++)
                {
                    int newDistance = problem.getDistance(0, available[j]);
                    if (candidateDistance > newDistance)
                    {
                        candidate = available[j];
                        candidateDistance = newDistance;
                    }
                }
                RCL.Add(candidate);
                available.Remove(candidate);
            }
            return RCL;
        }

        /// <summary>
        /// Fase constructiva de GRASP. Añade un nodo de la RCL a cada ruta y construye el resto con un algortimpo voraz
        /// </summary>
        /// <param name="solution">Solución Parcial</param>
        /// <param name="RestrictedCandidateList">RCL</param>
        private void BuildSolution(PartialSolution solution, List<int> RestrictedCandidateList)
        {
            Problem problem = solution.problem;
            List<int>[] routes = solution.routes;
            List<int> available = solution.available;
            int clientLimit = (int)(problem.clientCount() * 0.1) + problem.clientCount() / routes.Length - 2;
            int[,] bestNext = new int[routes.Length, 3];
            for (int i = 0; i < bestNext.GetLength(0); i++)
            {
                bestNext[i, 0] = 0;
                bestNext[i, 1] = int.MaxValue;
                bestNext[i, 2] = 0;
            }
            Random random = new();
            List<int> RCL = new List<int>(RestrictedCandidateList);
            for (int i = 0; i < routes.Length && RCL.Count > 0; i++)
            {
                int node = RCL[random.Next(RCL.Count)];
                routes[i].Add(node);
                available.Remove(node);
                RCL.Remove(node);
                ++bestNext[i, 2];
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
                if (randomConstructive)
                {
                    do grows = random.Next(0, routes.Length);
                    while (bestNext[grows, 1] == int.MaxValue);
                }
                else
                {
                    for (int i = 1; i < bestNext.GetLength(0); i++)
                        if (bestNext[grows, 1] > bestNext[i, 1]) grows = i;
                }
                int explored = bestNext[grows, 0];
                ++bestNext[grows, 2];
                routes[grows].Add(explored);
                available.Remove(explored);
                for (int i = 0; i < bestNext.GetLength(0); i++)
                    if (bestNext[i, 0] == explored) bestNext[i, 1] = int.MaxValue;
            }
            for (int i = 0; i < routes.Length; i++) routes[i].Add(0);
        }

        /// <summary>
        /// Fase de exploración del GRASP (exploración local Simple)
        /// </summary>
        /// <param name="solution">Solución Parcial</param>
        private bool SimpleSearch(PartialSolution solution, int environment = 0)
        {
            int count = 0;
            do
            {
                searchEnvironments[environment].Build(solution, anxious);
                count++;
            } while (searchEnvironments[environment].transformation[searchEnvironments[environment].best].Transform(solution));
            return count > 1;
        }

        /// <summary>
        /// Fase de exploración del GRASP mediante General Variable Neighborhood Search
        /// </summary>
        /// <param name="solution">Solución Parcial</param>
        private void GVNS(ref PartialSolution solution)
        {
            bool upgraded;
            Random random = new();
            do
            {
                upgraded = false;
                for (int i = 0; i < searchEnvironments.Length; i++)
                {
                    searchEnvironments[i].Build(solution, false, false);
                    PartialSolution randomStart = new(solution);

                    if (searchEnvironments[i].variance.Count > 1 && searchEnvironments[i].transformation[random.Next(1, searchEnvironments[i].variance.Count)].Transform(randomStart))
                    {
                        for (int j = 0; j < searchEnvironments.Length; j++)
                        {
                            if (SimpleSearch(randomStart, j)) j = -1;
                        }
                        if (solution.totalCost() > randomStart.totalCost())
                        {
                            solution = randomStart;
                            i = -1;
                            upgraded = true;
                        }
                    }
                }
            } while (upgraded);
        }

        /// <summary>
        /// Comparación de la calidad de la solución
        /// </summary>
        /// <param name="candidate">Solución candidata</param>
        /// <param name="lastBest">Mejor solución hasta el momento</param>
        private void UpdateBestSolution(PartialSolution candidate, ref PartialSolution lastBest)
        {
            if (lastBest.available.Count > 0 || lastBest.totalCost() > candidate.totalCost()) lastBest = candidate;
        }
    }

    /// <summary>
    /// Clase auxiliar de GRASPAlgorithm para representar soluciones parciales.
    /// </summary>
    internal class PartialSolution
    {
        /// <summary>
        /// Problema a resolver
        /// </summary>
        public Problem problem { get; }

        /// <summary>
        /// Rutas
        /// </summary>
        public List<int>[] routes;

        /// <summary>
        /// Nodos por explorar
        /// </summary>
        public List<int> available;

        /// <summary>
        /// Constructor de copia
        /// </summary>
        /// <param name="solution">Solución parcial a copiar</param>
        public PartialSolution(PartialSolution solution)
        {
            problem = solution.problem;
            routes = new List<int>[solution.routes.Length];
            for (int i = 0; i < routes.Length; i++) routes[i] = new List<int>(solution.routes[i]);
            available = new List<int>(solution.available);
        }

        /// <summary>
        /// Constructor por defecto (crea una solución parcial no válida)
        /// </summary>
        public PartialSolution()
        {
            problem = new Problem();
            routes = new List<int>[0];
            available = new List<int>() { 0 };
        }

        /// <summary>
        /// Constructor. Prepara una solución parcial vacía a partir de un problema
        /// </summary>
        /// <param name="problem">Problema</param>
        public PartialSolution(Problem problem)
        {
            this.problem = problem;
            routes = new List<int>[problem.vehicleCount];
            for (int i = 0; i < routes.Length; i++) routes[i] = new List<int> { 0 };
            available = new List<int>();
            for (int i = 1; i <= problem.clientCount(); i++) available.Add(i);
        }

        /// <summary>
        /// Devuelve la solución final si la solución parcial está completa
        /// </summary>
        /// <returns>Solución final</returns>
        /// <exception cref="Exception">No se puede construir una solución incompleta</exception>
        public Solution turnToSolution()
        {
            int count = 0;
            for (int i = 0; i < routes.Length; i++) count += routes[i].Count - 2;
            if (available.Count > 0 || count != problem.clientCount()) throw new Exception("Can't build a non-valid solution");
            int[] costs = new int[routes.Length];
            for (int i = 0; i < routes.Length; i++) costs[i] = problem.getRouteCost(routes[i]);
            return new Solution(routes, costs);
        }

        /// <summary>
        /// Coste total actual de la solución parcial
        /// </summary>
        /// <returns>Coste</returns>
        public int totalCost()
        {
            int sum = 0;
            for (int i = 0; i < routes.Length; i++) sum += problem.getRouteCost(routes[i]);
            return sum;
        }
    }
}
