using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP
{
    // Represents the Matrix at a particular Branch and Bound node.
    class Matrix
    {
        public Matrix(City[] cities, double[,] costs, double lowerBound )
        {
            if(cities.Length != costs.GetLength(0)) {
                throw new ArgumentOutOfRangeException();
            }

            if (cities.Length != costs.GetLength(1))
            {
                throw new ArgumentOutOfRangeException();
            }

            Cities = cities;
            Costs = costs;
            LowerBound = lowerBound;
            CitiesVisited = 0;
        }

        private Matrix(
            City[] cities,
            double[,] costs,
            double lowerBound,
            int citiesVisited) : this(cities, costs, lowerBound)
        {
            CitiesVisited = citiesVisited;
        }

        // Reduces a matrix.
        // Creates a new matrix, with the same values as the current
        // instance, with the change that each row and each column
        // is reduced so that there is at least 1 zero in each.
        // LowerBound is increased by the amount subtracted from
        // each row and column.
        public Matrix Reduce()
        {
            double lowerBound = LowerBound;
            double[,] costs = Costs.Clone() as double[,];

            for(int i = 0; i < costs.GetLength(0); i++)
            {
                int lowestJInd = 0;
                double lowestJ = costs[i, 0];

                // Find the lowest
                for(int j = 1; j < costs.GetLength(0); j++) {
                    if(costs[i,j] < lowestJ)
                    {
                        lowestJInd = j;
                        lowestJ = costs[i, j];
                    }
                }

                if (!Double.IsInfinity(lowestJ))
                {
                    // Increase lower bound and decrease each element in row
                    lowerBound += lowestJ;
                    for (int j = 0; j < costs.GetLength(0); j++)
                    {
                        costs[i, j] -= lowestJ;
                    }
                }
            }

            for (int j = 0; j < costs.GetLength(0); j++)
            {
                int lowestIInd = 0;
                double lowestI = costs[0, j];

                // Find lowest
                for (int i = 1; i < costs.GetLength(0); i++)
                {
                    if (costs[i, j] < lowestI)
                    {
                        lowestIInd = i;
                        lowestI = costs[i, j];
                    }
                }

                if (!Double.IsInfinity(lowestI))
                {
                    // Increase lower bound and decrease each element in row
                    lowerBound += lowestI;
                    for (int i = 0; i < costs.GetLength(0); i++)
                    {
                        costs[i, j] -= lowestI;
                    }
                }
            }

            // Return a new instance (don't change current in any way)
            return new Matrix(Cities, costs, lowerBound, CitiesVisited);
        }

        // Follows a route
        // Returns a new matrix with the same values, changed so that
        // the route from node i to node j has been followed.  Makes it
        // so that nothing from i or to j will be followed again.  Also
        // makes it so that the route from j to the beginning node can't
        // be followed.  Returns a reduced matrix.
        public Matrix FollowRoute(int i, int j)
        {
            double[,] costs = Costs.Clone() as double[,];

            for (int k = 0; k < Costs.GetLength(0); k++) {
                costs[i, k] = Double.PositiveInfinity;
                costs[k, j] = Double.PositiveInfinity;
            }

            // Make it so that it can't go to the start city (unless this is the last city
            if (CitiesVisited < Cities.Length - 2)
            {
                costs[j, 0] = Double.PositiveInfinity;
            }

            return new Matrix(Cities, costs, LowerBound, CitiesVisited + 1).Reduce();
        }

        public City[] Cities { get; }
        public double[,] Costs { get; }
        public double LowerBound { get; }
        private int CitiesVisited;
    }
}
