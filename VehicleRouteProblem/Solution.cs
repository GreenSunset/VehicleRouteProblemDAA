
namespace VehicleRouteProblem
{
    /// <summary>
    /// Clase para representar una solución a un problema VRP
    /// </summary>
    internal class Solution
    {
        /// <summary>
        /// Rutas que componen la solución
        /// </summary>
        public List<int>[] routes { get; }

        /// <summary>
        /// Coste de cada ruta
        /// </summary>
        public int[] routeCosts { get; }

        /// <summary>
        /// Coste total de la solución
        /// </summary>
        public int totalCost { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="routes">Rutas</param>
        /// <param name="costs">Costes</param>
        public Solution(List<int>[] routes, int[] costs)
        {
            this.routes = routes;
            this.routeCosts = costs;
            totalCost = 0;
            for (int i = 0; i < routeCosts.Length; i++) totalCost += routeCosts[i];
        }

        /// <summary>
        /// Devuelve la información de la solución en formato String
        /// </summary>
        /// <returns>Solución formateada</returns>
        public override string ToString()
        {
            string output = $"Total Cost: {totalCost}\nNumber of routes: {routes.Length}\n";
            for (int i = 0; i < routes.Length; i++)
            {
                output += $"\t-Route {i + 1} cost: {routeCosts[i]}\n\t  Path: {{";
                for (int j = 0; j < routes[i].Count; j++)
                {
                    if (j > 0) output += ", ";
                    output += routes[i][j];
                }
                output += "}\n";
            }
            return output;
        }
    }
}
