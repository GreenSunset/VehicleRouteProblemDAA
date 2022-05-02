
namespace VehicleRouteProblem
{
    /// <summary>
    /// Interface for local solution environments
    /// </summary>
    internal interface SolutionEnvironment
    {
        /// <summary>
        /// Difference in cost from the original solution
        /// </summary>
        public List<int> variance { get; }

        /// <summary>
        /// Transformation needed to change solution
        /// </summary>
        public List<SolutionTransformation> transformation { get; }

        /// <summary>
        /// Index of the best variant
        /// </summary>
        public int best { get; }

        /// <summary>
        /// Generates an environment in base of a solution
        /// </summary>
        /// <param name="solution">Solution</param>
        /// <param name="anxious">If true, generates only one better solution</param>
        /// <param name="onlyBest">Whether or not to skip non improvements</param>
        public void Build(PartialSolution solution, bool anxious = false, bool onlyBest = true);

    }

    /// <summary>
    /// Represents a reinsertion local environment
    /// </summary>
    internal class ReinsertionEnvironment : SolutionEnvironment
    {
        /// <summary>
        /// Difference in cost from the original solution
        /// </summary>
        public List<int> variance { get; private set; }
        /// <summary>
        /// Transformation needed to change solution
        /// </summary>
        public List<SolutionTransformation> transformation { get; private set; }
        /// <summary>
        /// Index of the best variant
        /// </summary>
        public int best { get; private set; }
        /// <summary>
        /// Type of environment (intra, inter, full)
        /// </summary>
        private string type;

        /// <summary>
        /// Sets up an environment
        /// </summary>
        /// <param name="type">Tipe of reinsert (intra, inter, full)</param>
        public ReinsertionEnvironment(string type = "full")
        {
            if (type != "full" && type != "intra" && type != "inter")
                throw new ArgumentException("Invalid type");
            variance = new List<int>() { 0 };
            transformation = new List<SolutionTransformation>() { new NoTransformation() };
            this.type = type;
            best = 0;
        }

        /// <summary>
        /// Generates an environment in base of a solution
        /// </summary>
        /// <param name="solution">Solution</param>
        /// <param name="anxious">If true, generates only one better solution</param>
        /// <param name="onlyBest">Whether or not to skip non improvements</param>
        public void Build(PartialSolution solution, bool anxious, bool onlyBest)
        {
            Problem problem = solution.problem;
            List<int>[] routes = solution.routes;
            int clientLimit = (int)(problem.clientCount() * 0.1) + problem.clientCount() / routes.Length - 2;
            variance = new List<int>() { 0 };
            transformation = new List<SolutionTransformation>() { new NoTransformation() };
            best = 0;
            for (int Route = 0; Route < routes.Length && (!anxious || variance.Count < 1); Route++)
            {
                for (int originIndex = 1; originIndex < routes[Route].Count - 1 && (!anxious || variance.Count < 1); originIndex++)
                {
                    if (type == "full" || type == "intra") IntraRoute(anxious, onlyBest, problem, routes, Route, originIndex);
                    if (type == "full" || type == "inter") InterRoute(anxious, onlyBest, problem, routes, clientLimit, Route, originIndex);
                }
            }
        }

        /// <summary>
        /// Adds interroute variants
        /// </summary>
        /// <param name="anxious"></param>
        /// <param name="onlyBest"></param>
        /// <param name="problem"></param>
        /// <param name="routes"></param>
        /// <param name="clientLimit"></param>
        /// <param name="originRoute"></param>
        /// <param name="originIndex"></param>
        private void InterRoute(bool anxious, bool onlyBest, Problem problem, List<int>[] routes, int clientLimit, int originRoute, int originIndex)
        {
            for (int destinationRoute = 0; destinationRoute < routes.Length && (!anxious || variance.Count < 1); destinationRoute++)
            {
                if (destinationRoute == originRoute || routes[destinationRoute].Count >= clientLimit) continue;
                for (int destinationIndex = 1; destinationIndex < routes[destinationRoute].Count && (!anxious || variance.Count < 1); destinationIndex++)
                {
                    int cost =
                        problem.getDistance(routes[originRoute][originIndex], routes[destinationRoute][destinationIndex]) +
                        problem.getDistance(routes[destinationRoute][destinationIndex - 1], routes[originRoute][originIndex]) +
                        problem.getDistance(routes[originRoute][originIndex - 1], routes[originRoute][originIndex + 1]) -
                        (problem.getDistance(routes[originRoute][originIndex - 1], routes[originRoute][originIndex]) +
                        problem.getDistance(routes[originRoute][originIndex], routes[originRoute][originIndex + 1]) +
                        problem.getDistance(routes[destinationRoute][destinationIndex - 1], routes[destinationRoute][destinationIndex]));
                    if ((anxious || onlyBest) && cost >= 0) continue;
                    if (variance[best] > cost) best = variance.Count;
                    variance.Add(cost);
                    transformation.Add(new ReinsertionTransformation(originIndex, originRoute, destinationIndex, destinationRoute));
                }
            }
        }

        /// <summary>
        /// Adds intraroute variants
        /// </summary>
        /// <param name="anxious"></param>
        /// <param name="onlyBest"></param>
        /// <param name="problem"></param>
        /// <param name="routes"></param>
        /// <param name="route"></param>
        /// <param name="originIndex"></param>
        private void IntraRoute(bool anxious, bool onlyBest, Problem problem, List<int>[] routes, int route, int originIndex)
        {
            for (int destinationIndex = 1; destinationIndex < routes[route].Count - 1 && (!anxious || variance.Count < 1); destinationIndex++)
            {
                if (destinationIndex == originIndex - 2 || destinationIndex == originIndex - 1 || destinationIndex == originIndex) continue;
                int cost =
                    problem.getDistance(routes[route][originIndex - 1], routes[route][originIndex + 1]) -
                    (problem.getDistance(routes[route][originIndex - 1], routes[route][originIndex]) +
                    problem.getDistance(routes[route][originIndex], routes[route][originIndex + 1]));
                if (destinationIndex < originIndex) cost +=
                        problem.getDistance(routes[route][destinationIndex - 1], routes[route][originIndex]) +
                        problem.getDistance(routes[route][originIndex], routes[route][destinationIndex]) -
                        problem.getDistance(routes[route][destinationIndex - 1], routes[route][destinationIndex]);
                else cost +=
                        problem.getDistance(routes[route][originIndex], routes[route][destinationIndex + 1]) +
                        problem.getDistance(routes[route][destinationIndex], routes[route][originIndex]) -
                        problem.getDistance(routes[route][destinationIndex], routes[route][destinationIndex + 1]);
                if ((anxious || onlyBest) && cost >= 0) continue;
                if (variance[best] > cost) best = variance.Count;
                variance.Add(cost);
                transformation.Add(new ReinsertionTransformation(originIndex, route, destinationIndex));
            }
        }
    }

    /// <summary>
    /// Represents an Exchange local environment
    /// </summary>
    internal class ExchangeEnvironment : SolutionEnvironment
    {
        /// <summary>
        /// Difference in cost from the original solution
        /// </summary>
        public List<int> variance { get; private set; }
        /// <summary>
        /// Transformation needed to change solution
        /// </summary>
        public List<SolutionTransformation> transformation { get; private set; }
        /// <summary>
        /// Index of the best variant
        /// </summary>
        public int best { get; private set; }
        /// <summary>
        /// Type of environment (intra, inter, full)
        /// </summary>
        private string type;

        /// <summary>
        /// Sets up an environment
        /// </summary>
        /// <param name="type">Tipe of exchange (intra, inter, full)</param>
        public ExchangeEnvironment(string type = "full")
        {
            if (type != "full" && type != "intra" && type != "inter")
                throw new ArgumentException("Invalid type");
            variance = new List<int>() { 0 };
            transformation = new List<SolutionTransformation>() { new NoTransformation() };
            this.type = type;
            best = 0;
        }

        /// <summary>
        /// Generates an environment in base of a solution
        /// </summary>
        /// <param name="solution">Solution</param>
        /// <param name="anxious">If true, generates only one better solution</param>
        /// <param name="onlyBest">Whether or not to skip non improvements</param>
        public void Build(PartialSolution solution, bool anxious, bool onlyBest)
        {
            Problem problem = solution.problem;
            List<int>[] routes = solution.routes;
            variance = new List<int>() { 0 };
            transformation = new List<SolutionTransformation>() { new NoTransformation() };
            best = 0;
            for (int route = 0; route < routes.Length && (!anxious || variance.Count < 1); route++)
            {
                for (int originIndex = 1; originIndex < routes[route].Count - 1 && (!anxious || variance.Count < 1); originIndex++)
                {
                    if (type == "full" || type == "intra") IntraRoute(anxious, onlyBest, problem, routes, route, originIndex);
                    if (type == "full" || type == "inter") InterRoute(anxious, onlyBest, problem, routes, route, originIndex);
                }
            }
        }

        /// <summary>
        /// Adds interroute variants
        /// </summary>
        /// <param name="anxious"></param>
        /// <param name="onlyBest"></param>
        /// <param name="problem"></param>
        /// <param name="routes"></param>
        /// <param name="clientLimit"></param>
        /// <param name="originRoute"></param>
        /// <param name="originIndex"></param>
        private void InterRoute(bool anxious, bool onlyBest, Problem problem, List<int>[] routes, int originRoute, int originIndex)
        {
            for (int destinationRoute = originRoute + 1; destinationRoute < routes.Length && (!anxious || variance.Count < 1); destinationRoute++)
            {
                for (int destinationIndex = 1; destinationIndex < routes[destinationRoute].Count - 1 && (!anxious || variance.Count < 1); destinationIndex++)
                {
                    int cost =
                        problem.getDistance(routes[originRoute][originIndex], routes[destinationRoute][destinationIndex + 1]) +
                        problem.getDistance(routes[destinationRoute][destinationIndex - 1], routes[originRoute][originIndex]) +
                        problem.getDistance(routes[originRoute][originIndex - 1], routes[destinationRoute][destinationIndex]) +
                        problem.getDistance(routes[destinationRoute][destinationIndex], routes[originRoute][originIndex + 1]) -
                        (problem.getDistance(routes[originRoute][originIndex - 1], routes[originRoute][originIndex]) +
                        problem.getDistance(routes[originRoute][originIndex], routes[originRoute][originIndex + 1]) +
                        problem.getDistance(routes[destinationRoute][destinationIndex], routes[destinationRoute][destinationIndex + 1]) +
                        problem.getDistance(routes[destinationRoute][destinationIndex - 1], routes[destinationRoute][destinationIndex]));
                    if ((anxious || onlyBest) && cost >= 0) continue;
                    if (variance[best] > cost) best = variance.Count;
                    variance.Add(cost);
                    transformation.Add(new ExchangeTransformation(originIndex, originRoute, destinationIndex, destinationRoute));
                }
            }
        }

        /// <summary>
        /// Adds intraroute variants
        /// </summary>
        /// <param name="anxious"></param>
        /// <param name="onlyBest"></param>
        /// <param name="problem"></param>
        /// <param name="routes"></param>
        /// <param name="route"></param>
        /// <param name="originIndex"></param>
        private void IntraRoute(bool anxious, bool onlyBest, Problem problem, List<int>[] routes, int route, int originIndex)
        {
            for (int destinationIndex = originIndex + 1; destinationIndex < routes[route].Count - 1 && (!anxious || variance.Count < 1); destinationIndex++)
            {
                int cost;
                if (destinationIndex == originIndex + 1)
                {
                    cost = problem.getDistance(routes[route][originIndex], routes[route][destinationIndex + 1]) +
                        problem.getDistance(routes[route][originIndex - 1], routes[route][destinationIndex]) +
                        problem.getDistance(routes[route][destinationIndex], routes[route][originIndex]) -
                        (problem.getDistance(routes[route][originIndex - 1], routes[route][originIndex]) +
                        problem.getDistance(routes[route][originIndex], routes[route][destinationIndex]) +
                        problem.getDistance(routes[route][destinationIndex], routes[route][destinationIndex + 1]));
                }
                else
                {
                    cost =
                        problem.getDistance(routes[route][originIndex], routes[route][destinationIndex + 1]) +
                        problem.getDistance(routes[route][destinationIndex - 1], routes[route][originIndex]) +
                        problem.getDistance(routes[route][originIndex - 1], routes[route][destinationIndex]) +
                        problem.getDistance(routes[route][destinationIndex], routes[route][originIndex + 1]) -
                        (problem.getDistance(routes[route][originIndex - 1], routes[route][originIndex]) +
                        problem.getDistance(routes[route][originIndex], routes[route][originIndex + 1]) +
                        problem.getDistance(routes[route][destinationIndex], routes[route][destinationIndex + 1]) +
                        problem.getDistance(routes[route][destinationIndex - 1], routes[route][destinationIndex]));
                }
                if ((anxious || onlyBest) && cost >= 0) continue;
                if (variance[best] > cost) best = variance.Count;
                variance.Add(cost);
                transformation.Add(new ExchangeTransformation(originIndex, route, destinationIndex));
            }
        }
    }

    /// <summary>
    /// Represents a 2opt local environment
    /// </summary>
    internal class TwoOptEnvironment : SolutionEnvironment
    {
        /// <summary>
        /// Difference in cost from the original solution
        /// </summary>
        public List<int> variance { get; private set; }
        /// <summary>
        /// Transformation needed to change solution
        /// </summary>
        public List<SolutionTransformation> transformation { get; private set; }
        /// <summary>
        /// Index of the best variant
        /// </summary>
        public int best { get; private set; }

        /// <summary>
        /// Sets up an environment
        /// </summary>
        public TwoOptEnvironment()
        {
            variance = new List<int>() { 0 };
            transformation = new List<SolutionTransformation>() { new NoTransformation() };
            best = 0;
        }

        /// <summary>
        /// Generates an environment in base of a solution
        /// </summary>
        /// <param name="solution">Solution</param>
        /// <param name="anxious">If true, generates only one better solution</param>
        /// <param name="onlyBest">Whether or not to skip non improvements</param>
        public void Build(PartialSolution solution, bool anxious, bool onlyBest)
        {
            Problem problem = solution.problem;
            List<int>[] routes = solution.routes;
            variance = new List<int>() { 0 };
            transformation = new List<SolutionTransformation>() { new NoTransformation() };
            best = 0;
            for (int route = 0; route < routes.Length && (!anxious || variance.Count < 1); route++)
            {
                for (int startSwap = 1; startSwap < routes[route].Count - 1 && (!anxious || variance.Count < 1); startSwap++)
                {
                    for (int endSwap = startSwap + 1; endSwap < routes[route].Count - 1 && (!anxious || variance.Count < 1); endSwap++)
                    {
                        int cost = problem.getDistance(routes[route][startSwap - 1], routes[route][endSwap]) +
                            problem.getDistance(routes[route][startSwap], routes[route][endSwap + 1]) -
                            (problem.getDistance(routes[route][startSwap - 1], routes[route][startSwap]) +
                            problem.getDistance(routes[route][endSwap], routes[route][endSwap + 1]));
                        for (int i = startSwap; i < endSwap; i++)
                        {
                            cost += problem.getDistance(routes[route][i + 1], routes[route][i]) -
                                problem.getDistance(routes[route][i], routes[route][i + 1]);
                        }
                        if ((anxious || onlyBest) && cost >= 0) continue;
                        if (variance[best] > cost) best = variance.Count;
                        variance.Add(cost);
                        transformation.Add(new TwoOptTransformation(route, startSwap, endSwap));
                    }
                }
            }
        }
    }
}
