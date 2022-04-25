
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

        public GRASPAlgorithm(string searchMethod = "reinsertion", bool anxiousSearch = false)
        {
            if (searchMethod != "reinsertion" && searchMethod != "intra reinsertion" && searchMethod != "inter reinsertion" &&
                searchMethod != "exchange" &&  searchMethod != "intra exchange" && searchMethod != "inter exchange" && searchMethod != "2-opt")
                throw new ArgumentException("Loacal search method not recognized");
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
                PostProcessing(currentSolution);
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
            Random random = new Random();
            List<int> RCL = new List<int>(RestrictedCandidateList);
            for (int i = 0; i < routes.Length && RCL.Count > 0; i++)
            {
                int node = RCL[random.Next(RCL.Count)];
                routes[i].Add(node);
                available.Remove(node);
                RCL.Remove(node);
                ++bestNext[i, 2];
            }
            while(available.Count > 0)
            {
                for (int i = 0; i < bestNext.GetLength(0); i++)
                {
                    if (bestNext[i,1] == int.MaxValue && bestNext[i, 2] < clientLimit)
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
        }

        /// <summary>
        /// Fase de exploración local del GRASP
        /// </summary>
        /// <param name="solution">Solución Parcial</param>
        private void PostProcessing(PartialSolution solution)
        {
            int bestVariance;
            int bestLocalSolution;
            int maxRepeats = 100;
            int repeats = 0;
            SolutionEnvironment localEnvironment;
            do
            {
                bestVariance = 0;
                bestLocalSolution = 0;
                switch (searchMethod)
                {
                    case "reinsertion":
                        localEnvironment = new ReinsertionFullEnvironment(solution);
                        break;
                    case "intra reinsertion":
                        localEnvironment = new ReinsertionIntraEnvironment(solution);
                        break;
                    case "inter reinsertion":
                        localEnvironment = new ReinsertionInterEnvironment(solution);
                        break;
                    case "exchange":
                        localEnvironment = new ExchangeFullEnvironment(solution);
                        break;
                    case "intra exchange":
                        localEnvironment = new ExchangeIntraEnvironment(solution);
                        break;
                    case "inter exchange":
                        localEnvironment = new ExchangeInterEnvironment(solution);
                        break;
                    case "2-opt":
                        localEnvironment = new TwoOptEnvironment(solution);
                        break;
                    default:
                        throw new ArgumentException("An error ocurred");
                }
                for (int i = 0; i < localEnvironment.variance.Count; ++i)
                {
                    if (localEnvironment.variance[i] < bestVariance)
                    {
                        bestVariance = localEnvironment.variance[i];
                        bestLocalSolution = i;
                    }
                }
                PartialSolution debug = new(solution);
                localEnvironment.transformation[bestLocalSolution].Transform(debug);
                if (debug.totalCost() - solution.totalCost() != bestVariance) Console.WriteLine("ERROR in " + searchMethod);
                /*Console.WriteLine("Previous: " + solution.totalCost());
                Console.WriteLine("Next: " + debug.totalCost());
                Console.WriteLine("Difference: " + bestVariance);/**/
            } while (++repeats < maxRepeats && localEnvironment.transformation[bestLocalSolution].Transform(solution));
        }

        /// <summary>
        /// Comparación de la calidad de la solución (no implementada)
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
            if (available.Count > 0) throw new Exception("Can't build a non-valid solution");
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
