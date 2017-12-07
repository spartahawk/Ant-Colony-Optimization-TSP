using System.Collections.Generic;

namespace TSP
{
    class BBNode
    {
        public BBNode(List<int> cities, Matrix matrix, double cost)
        {
            Cities = cities;
            Matrix = matrix;
            Cost = cost;
        }

        public List<int> Cities { get; }
        public Matrix Matrix { get; }
        public double Cost { get; }
    }
}
