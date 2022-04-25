
namespace VehicleRouteProblem
{
    /// <summary>
    /// Interface for local transformations of a solution
    /// </summary>
    internal interface SolutionTransformation
    {
        /// <summary>
        /// Applies the transformation to a solution
        /// </summary>
        /// <param name="solution">Solution</param>
        /// <returns>Whether the transformation has been applied</returns>
        public bool Transform(PartialSolution solution);
    }

    /// <summary>
    /// Represents a null transformation
    /// </summary>
    internal class NoTransformation : SolutionTransformation
    {
        /// <summary>
        /// Returns false, as no alteration has been made
        /// </summary>
        /// <param name="solution">Solution</param>
        /// <returns>False</returns>
        public bool Transform(PartialSolution solution)
        {
            return false;
        }
    }

    /// <summary>
    /// Represents a posibility in a reinsertion local environment
    /// </summary>
    internal class ReinsertionTransformation : SolutionTransformation
    {
        /// <summary>
        /// Route of the element to be moved
        /// </summary>
        private int originRoute;
        /// <summary>
        /// Route where to move the element
        /// </summary>
        private int destinationRoute;
        /// <summary>
        /// Index of the element to be moved
        /// </summary>
        private int originIndex;
        /// <summary>
        /// Index where the element will be inserted
        /// </summary>
        private int destinationIndex;

        /// <summary>
        /// Constructor for an intra route reinsertion
        /// </summary>
        /// <param name="originIndex">Index of the element to move</param>
        /// <param name="route">Route where the movement takes place</param>
        /// <param name="destinationIndex">Index where to insert the element</param>
        public ReinsertionTransformation(int originIndex, int route, int destinationIndex)
        {
            originRoute = route;
            destinationRoute = route;
            this.originIndex = originIndex;
            this.destinationIndex = destinationIndex;
        }

        /// <summary>
        /// Constructor for an inter route reinsertion
        /// </summary>
        /// <param name="originIndex"> Index of the element to be moved</param>
        /// <param name="originRoute"> Route of the element to be moved</param>
        /// <param name="destinationIndex"> Index where the element will be inserted</param>
        /// <param name="destinationRoute"> Route where to move the element</param>
        public ReinsertionTransformation(int originIndex, int originRoute, int destinationIndex, int destinationRoute)
        {
            this.originRoute = originRoute;
            this.destinationRoute = destinationRoute;
            this.originIndex = originIndex;
            this.destinationIndex = destinationIndex;
        }

        /// <summary>
        /// Aplies the reinsertion
        /// </summary>
        /// <param name="solution">Solution</param>
        /// <returns>True</returns>
        public bool Transform(PartialSolution solution)
        {
            int swap = solution.routes[originRoute][originIndex];
            if (originRoute == destinationRoute)
            {
                if (originIndex < destinationIndex) for (int i = originIndex; i < destinationIndex; i++) solution.routes[originRoute][i] = solution.routes[originRoute][i + 1];
                else for (int i = originIndex; i > destinationIndex; i--) solution.routes[originRoute][i] = solution.routes[originRoute][i - 1];
                solution.routes[originRoute][destinationIndex] = swap;
            }
            else
            {
                solution.routes[originRoute].Remove(swap);
                solution.routes[destinationRoute].Insert(destinationIndex, swap);
            }
            return true;
        }
    }

    /// <summary>
    /// Represents a posibility in an exchange local environment
    /// </summary>
    internal class ExchangeTransformation : SolutionTransformation
    {
        /// <summary>
        /// Route of the first element to be exchanged
        /// </summary>
        private int originRoute;
        /// <summary>
        /// Route of the second element to be exchanged
        /// </summary>
        private int destinationRoute;
        /// <summary>
        /// Index of the first element to be exchanged
        /// </summary>
        private int originIndex;
        /// <summary>
        /// Index of the second element to be exchanged
        /// </summary>
        private int destinationIndex;

        /// <summary>
        /// Constructor for an intra route exchange
        /// </summary>
        /// <param name="originIndex">Index of the first element to exchange</param>
        /// <param name="route">Route where the movement takes place</param>
        /// <param name="destinationIndex">Index of the second element to exchange</param>
        public ExchangeTransformation(int originIndex, int route, int destinationIndex)
        {
            originRoute = route;
            destinationRoute = route;
            this.originIndex = originIndex;
            this.destinationIndex = destinationIndex;
        }

        /// <summary>
        /// Constructor for an inter route exchange
        /// </summary>
        /// <param name="originIndex">Index of the first element to exchange</param>
        /// <param name="originRoute">Route of the first element</param>
        /// <param name="destinationIndex">Index of the second element to exchange</param>
        /// <param name="destinationRoute">Route of the second element</param>
        public ExchangeTransformation(int originIndex, int originRoute, int destinationIndex, int destinationRoute)
        {
            this.originRoute = originRoute;
            this.destinationRoute = destinationRoute;
            this.originIndex = originIndex;
            this.destinationIndex = destinationIndex;
        }

        /// <summary>
        /// Aplies the exchange
        /// </summary>
        /// <param name="solution">Solution</param>
        /// <returns>True</returns>
        public bool Transform(PartialSolution solution)
        {
            int swap = solution.routes[originRoute][originIndex];
            solution.routes[originRoute][originIndex] = solution.routes[destinationRoute][destinationIndex];
            solution.routes[destinationRoute][destinationIndex] = swap;
            return true;
        }
    }

    /// <summary>
    /// Represents a posibility in a 2opt local environment
    /// </summary>
    internal class TwoOptTransformation : SolutionTransformation
    {
        /// <summary>
        /// Route where the movement takes place
        /// </summary>
        private int route;
        /// <summary>
        /// Start index for the inversion
        /// </summary>
        private int startSwap;
        /// <summary>
        /// End index for the inversion
        /// </summary>
        private int endSwap;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="route">Route where the movement takes place</param>
        /// <param name="start">Start index for the inversion</param>
        /// <param name="end">End index for the inversion</param>
        public TwoOptTransformation(int route, int start, int end)
        {
            this.route = route;
            this.startSwap = start;
            this.endSwap = end;
        }

        /// <summary>
        /// Aplies the inversion
        /// </summary>
        /// <param name="solution">Solution</param>
        /// <returns>True</returns>
        public bool Transform(PartialSolution solution)
        {
            for (int i = startSwap, j = endSwap; i < j; i++, j--)
            {
                int swap = solution.routes[route][j];
                solution.routes[route][j] = solution.routes[route][i];
                solution.routes[route][i] = swap;
            }
            return true;
        }

    }
}
