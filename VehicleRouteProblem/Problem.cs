
namespace VehicleRouteProblem
{
    /// <summary>
    /// Clase para representar un problema VRP
    /// </summary>
    internal class Problem
    {
        /// <summary>
        /// Matriz de distancias
        /// </summary>
        private int[,] distanceMatrix;

        /// <summary>
        /// Vehículos disponibles
        /// </summary>
        public int vehicleCount { get; }

        /// <summary>
        /// Constructor de un problema vacío
        /// </summary>
        public Problem()
        {
            distanceMatrix = new int[0, 0];
            vehicleCount = 0;
        }

        /// <summary>
        /// Constructor de un problema a partir de un fichero
        /// </summary>
        /// <param name="filename">Nombre del fichero</param>
        /// <exception cref="Exception">El fichero debe tener el firnato apropiado</exception>
        public Problem(string filename)
        {
            string[] lines = File.ReadAllLines(filename);
            int clientCount;
            if (lines[0].StartsWith("n_clientes:"))
            {
                clientCount = int.Parse(lines[0].Split('\t')[1]);
            }
            else throw new Exception("input file format error");
            if (lines[1].StartsWith("n_vehiculos:"))
            {
                vehicleCount = int.Parse(lines[1].Split('\t')[1]);
            }
            else throw new Exception("input file format error");
            if (!lines[2].StartsWith("Distancia_entre_cada_par_de_clientes"))
            {
                throw new Exception("input file format error");
            }
            if (lines.Length < clientCount + 4) throw new Exception("input file format error");

            distanceMatrix = new int[clientCount + 1, clientCount + 1];
            for (int i = 3; i < lines.Length; i++)
            {
                string[] words = lines[i].Split('\t');
                if (words.Length != clientCount + 1) throw new Exception("Wrong number of distances");
                for (int j = 0; j < words.Length; j++)
                {
                    distanceMatrix[i - 3,j] = int.Parse(words[j]);
                }
            }
        }

        /// <summary>
        /// Devuelve la distancia de un nodo a otro
        /// </summary>
        /// <param name="start">Nodo de partida</param>
        /// <param name="end">Nodo de llegada</param>
        /// <returns>Distancia</returns>
        /// <exception cref="ArgumentException">Nodos deben estar entre los nodos disponibles</exception>
        public int getDistance(int start, int end)
        {
            if (start < 0 || start >= distanceMatrix.GetLength(0) ||
                end < 0 || end >= distanceMatrix.GetLength(1))
                throw new ArgumentException();
            return distanceMatrix[start, end];
        }

        /// <summary>
        /// Devuelve el coste total de una ruta hipotética
        /// </summary>
        /// <param name="route">Ruta</param>
        /// <returns>Coste de ruta</returns>
        public int getRouteCost(List<int> route)
        {
            int cost = 0;
            for (int i = 0; i < route.Count - 1; i++)
            {
                cost += getDistance(route[i], route[i + 1]);
            }
            return cost;
        }

        /// <summary>
        /// Devuelve el número de clientes o nodos objetivo del problema
        /// </summary>
        /// <returns>Número de clientes</returns>
        public int clientCount()
        {
            return distanceMatrix.GetLength(0) - 1;
        }
    }
}
