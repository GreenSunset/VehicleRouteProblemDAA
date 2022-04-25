
namespace VehicleRouteProblem
{
    /// <summary>
    /// Interfaz para algoritmos aproximados que resuelven problemas VRP
    /// </summary>
    internal interface Algorithm
    {
        /// <summary>
        /// Resuelve un problema VRP
        /// </summary>
        /// <param name="problem">Problema</param>
        /// <returns>Solución alcanzada</returns>
        public Solution Solve(Problem problem);
    }
}
