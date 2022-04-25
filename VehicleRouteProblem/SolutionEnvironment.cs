
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

    }

    /// <summary>
    /// Represents a reinsertion local environment
    /// </summary>
    internal class ReinsertionFullEnvironment : SolutionEnvironment
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
        /// Generates an environment in base of a solution
        /// </summary>
        /// <param name="solution">Solution</param>
        /// <param name="skipPositives">Whether or not to skip non improvements</param>
        public ReinsertionFullEnvironment(PartialSolution solution, bool anxious = false, bool skipPositives = true)
        {
            Problem problem = solution.problem;
            List<int>[] routes = solution.routes;
            int clientLimit = (int)(problem.clientCount() * 0.1) + problem.clientCount() / routes.Length - 2;
            variance = new List<int>() { 0 };
            transformation = new List<SolutionTransformation>() { new NoTransformation() };
            for (int originRoute = 0; originRoute < routes.Length && (!anxious || variance.Count < 1); originRoute++)
            {
                for (int originIndex = 1; originIndex < routes[originRoute].Count - 1 && (!anxious || variance.Count < 1); originIndex++)
                {
                    for (int destinationIndex = 1; destinationIndex < routes[originRoute].Count - 1 && (!anxious || variance.Count < 1); destinationIndex++)
                    {
                        if (destinationIndex == originIndex - 2 || destinationIndex == originIndex - 1 || destinationIndex == originIndex) continue;
                        int cost = 
                            problem.getDistance(routes[originRoute][originIndex - 1], routes[originRoute][originIndex + 1]) -
                            (problem.getDistance(routes[originRoute][originIndex - 1], routes[originRoute][originIndex]) +
                            problem.getDistance(routes[originRoute][originIndex], routes[originRoute][originIndex + 1]));
                        if (destinationIndex < originIndex) cost +=
                                problem.getDistance(routes[originRoute][destinationIndex - 1], routes[originRoute][originIndex]) +
                                problem.getDistance(routes[originRoute][originIndex], routes[originRoute][destinationIndex]) -
                                problem.getDistance(routes[originRoute][destinationIndex - 1], routes[originRoute][destinationIndex]);
                        else cost += 
                                problem.getDistance(routes[originRoute][originIndex], routes[originRoute][destinationIndex + 1]) +
                                problem.getDistance(routes[originRoute][destinationIndex], routes[originRoute][originIndex]) -
                                problem.getDistance(routes[originRoute][destinationIndex], routes[originRoute][destinationIndex + 1]);
                        if (skipPositives && cost >= 0) continue;
                        variance.Add(cost);
                        transformation.Add(new ReinsertionTransformation(originIndex, originRoute, destinationIndex));
                    }
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
                            if (skipPositives && cost >= 0) continue;
                            variance.Add(cost); 
                            transformation.Add(new ReinsertionTransformation(originIndex, originRoute, destinationIndex, destinationRoute));
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Represents an inter route reinsertion local environment
    /// </summary>
    internal class ReinsertionInterEnvironment : SolutionEnvironment
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
        /// Generates an environment in base of a solution
        /// </summary>
        /// <param name="solution">Solution</param>
        /// <param name="skipPositives">Whether or not to skip non improvements</param>
        public ReinsertionInterEnvironment(PartialSolution solution, bool anxious = false, bool skipPositives = true)
        {
            Problem problem = solution.problem;
            List<int>[] routes = solution.routes;
            int clientLimit = (int)(problem.clientCount() * 0.1) + problem.clientCount() / routes.Length - 2;
            variance = new List<int>() { 0 };
            transformation = new List<SolutionTransformation>() { new NoTransformation() };
            for (int originRoute = 0; originRoute < routes.Length && (!anxious || variance.Count < 1); originRoute++)
            {
                for (int originIndex = 1; originIndex < routes[originRoute].Count - 1 && (!anxious || variance.Count < 1); originIndex++)
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
                            if (skipPositives && cost >= 0) continue;
                            variance.Add(cost);
                            transformation.Add(new ReinsertionTransformation(originIndex, originRoute, destinationIndex, destinationRoute));
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Represents am intra route reinsertion local environment
    /// </summary>
    internal class ReinsertionIntraEnvironment : SolutionEnvironment
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
        /// Generates an environment in base of a solution
        /// </summary>
        /// <param name="solution">Solution</param>
        /// <param name="skipPositives">Whether or not to skip non improvements</param>
        public ReinsertionIntraEnvironment(PartialSolution solution, bool anxious = false, bool skipPositives = true)
        {
            Problem problem = solution.problem;
            List<int>[] routes = solution.routes;
            int clientLimit = (int)(problem.clientCount() * 0.1) + problem.clientCount() / routes.Length - 2;
            variance = new List<int>() { 0 };
            transformation = new List<SolutionTransformation>() { new NoTransformation() };
            for (int route = 0; route < routes.Length && (!anxious || variance.Count < 1); route++)
            {
                for (int originIndex = 1; originIndex < routes[route].Count - 1 && (!anxious || variance.Count < 1); originIndex++)
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
                        if (skipPositives && cost >= 0) continue;
                        variance.Add(cost);
                        transformation.Add(new ReinsertionTransformation(originIndex, route, destinationIndex));
                    }
                }
            }
        }
    }

    /// <summary>
    /// Represents an Exchange local environment
    /// </summary>
    internal class ExchangeFullEnvironment : SolutionEnvironment
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
        /// Generates an environment in base of a solution
        /// </summary>
        /// <param name="solution">Solution</param>
        /// <param name="skipPositives">Whether or not to skip non improvements</param>
        public ExchangeFullEnvironment(PartialSolution solution, bool anxious = false, bool skipPositives = true)
        {
            Problem problem = solution.problem;
            List<int>[] routes = solution.routes;
            variance = new List<int>() { 0 };
            transformation = new List<SolutionTransformation>() { new NoTransformation() };
            for (int originRoute = 0; originRoute < routes.Length && (!anxious || variance.Count < 1); originRoute++)
            {
                for (int originIndex = 1; originIndex < routes[originRoute].Count - 1 && (!anxious || variance.Count < 1); originIndex++)
                {
                    for (int destinationIndex = originIndex + 1; destinationIndex < routes[originRoute].Count - 1 && (!anxious || variance.Count < 1); destinationIndex++)
                    {
                        int cost;
                        if (destinationIndex == originIndex + 1)
                        {
                            cost = problem.getDistance(routes[originRoute][originIndex], routes[originRoute][destinationIndex + 1]) +
                                problem.getDistance(routes[originRoute][originIndex - 1], routes[originRoute][destinationIndex]) +
                                problem.getDistance(routes[originRoute][destinationIndex], routes[originRoute][originIndex]) -
                                (problem.getDistance(routes[originRoute][originIndex - 1], routes[originRoute][originIndex]) +
                                problem.getDistance(routes[originRoute][originIndex], routes[originRoute][destinationIndex]) +
                                problem.getDistance(routes[originRoute][destinationIndex], routes[originRoute][destinationIndex + 1]));
                        }
                        else
                        {
                            cost =
                                problem.getDistance(routes[originRoute][originIndex], routes[originRoute][destinationIndex + 1]) +
                                problem.getDistance(routes[originRoute][destinationIndex - 1], routes[originRoute][originIndex]) +
                                problem.getDistance(routes[originRoute][originIndex - 1], routes[originRoute][destinationIndex]) +
                                problem.getDistance(routes[originRoute][destinationIndex], routes[originRoute][originIndex + 1]) -
                                (problem.getDistance(routes[originRoute][originIndex - 1], routes[originRoute][originIndex]) +
                                problem.getDistance(routes[originRoute][originIndex], routes[originRoute][originIndex + 1]) +
                                problem.getDistance(routes[originRoute][destinationIndex], routes[originRoute][destinationIndex + 1]) +
                                problem.getDistance(routes[originRoute][destinationIndex - 1], routes[originRoute][destinationIndex]));
                        }
                        if (skipPositives && cost >= 0) continue;
                        variance.Add(cost);
                        transformation.Add(new ExchangeTransformation(originIndex, originRoute, destinationIndex));
                    }
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
                            if (skipPositives && cost >= 0) continue;
                            variance.Add(cost);
                            transformation.Add(new ExchangeTransformation(originIndex, originRoute, destinationIndex, destinationRoute));
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Represents an intraRoute Exchange local environment
    /// </summary>
    internal class ExchangeIntraEnvironment : SolutionEnvironment
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
        /// Generates an environment in base of a solution
        /// </summary>
        /// <param name="solution">Solution</param>
        /// <param name="skipPositives">Whether or not to skip non improvements</param>
        public ExchangeIntraEnvironment(PartialSolution solution, bool anxious = false, bool skipPositives = true)
        {
            Problem problem = solution.problem;
            List<int>[] routes = solution.routes;
            variance = new List<int>() { 0 };
            transformation = new List<SolutionTransformation>() { new NoTransformation() };
            for (int route = 0; route < routes.Length && (!anxious || variance.Count < 1); route++)
            {
                for (int originIndex = 1; originIndex < routes[route].Count - 1 && (!anxious || variance.Count < 1); originIndex++)
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
                        if (skipPositives && cost >= 0) continue;
                        variance.Add(cost);
                        transformation.Add(new ExchangeTransformation(originIndex, route, destinationIndex));
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Represents an interRoute Exchange local environment
    /// </summary>
    internal class ExchangeInterEnvironment : SolutionEnvironment
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
        /// Generates an environment in base of a solution
        /// </summary>
        /// <param name="solution">Solution</param>
        /// <param name="skipPositives">Whether or not to skip non improvements</param>
        public ExchangeInterEnvironment(PartialSolution solution, bool anxious = false, bool skipPositives = true)
        {
            Problem problem = solution.problem;
            List<int>[] routes = solution.routes;
            variance = new List<int>() { 0 };
            transformation = new List<SolutionTransformation>() { new NoTransformation() };
            for (int originRoute = 0; originRoute < routes.Length && (!anxious || variance.Count < 1); originRoute++)
            {
                for (int originIndex = 1; originIndex < routes[originRoute].Count - 1 && (!anxious || variance.Count < 1); originIndex++)
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
                            if (skipPositives && cost >= 0) continue;
                            variance.Add(cost);
                            transformation.Add(new ExchangeTransformation(originIndex, originRoute, destinationIndex, destinationRoute));
                        }
                    }
                }
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
        public List<int> variance { get; }
        /// <summary>
        /// Transformation needed to change solution
        /// </summary>
        public List<SolutionTransformation> transformation { get; }

        /// <summary>
        /// Generates an environment in base of a solution
        /// </summary>
        /// <param name="solution">Solution</param>
        /// <param name="skipPositives">Whether or not to skip non improvements</param>
        public TwoOptEnvironment(PartialSolution solution, bool anxious = false, bool skipPositives = true)
        {
            Problem problem = solution.problem;
            List<int>[] routes = solution.routes;
            variance = new List<int>() { 0 };
            transformation = new List<SolutionTransformation>() { new NoTransformation() };
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
                        if (skipPositives && cost >= 0) continue;
                        variance.Add(cost);
                        transformation.Add(new TwoOptTransformation(route, startSwap, endSwap));
                    }
                }
            }
        }
    }

}
