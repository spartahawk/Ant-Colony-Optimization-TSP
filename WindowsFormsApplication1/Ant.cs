using System;
using System.Collections.Generic;

namespace TSP
{
    public class Ant
    {
        public double [,] Costs { get; }
        public double [,] Pheromones { get; set; }

        public Ant(double[,] costs, int randomSeed)
        {
            Costs = costs;
            Pheromones = new double[costs.GetLength(0), costs.GetLength(0)];
            random = new Random(randomSeed);
        }


        private Random random;
        public int[] FindRoute() {
            int[] path = new int[Costs.GetLength(0)];
            HashSet<int> visited = new HashSet<int>();

            // Choose a random city to start from
            path[0] = random.Next(path.Length);
            visited.Add(path[0]);

            // Is a path possible?
            bool success = FindRoute(0, ref path, visited);

            if (success)
            {
                // If so, return that path.
                return path;
            }
            else
            {
                return null;
            }
        }

        public static double[,] GetPheromonesForPath(int[] path, double[,] costs)
        {
            // Figure out the cost for the tour
            double cost = 0;
            for (int i = 0; i < path.Length - 1; i++)
            {
                cost += costs[path[i], path[i + 1]];
            }
            cost += costs[path[path.Length - 1], path[0]];

            // Pheromone added to each segment = 1/(tour cost)
            double pheromone = 1 / cost;
            double[,] ret = new double[path.Length, path.Length];

            // Make a matrix with pheromone added to each edge that was used.
            for(int i = 0; i < path.Length - 1; i++)
            {
                ret[path[i], path[i + 1]] = pheromone;
            }
            ret[path[path.Length - 1], path[0]] = pheromone;

            return ret;
        }

        private bool FindRoute(int index, ref int[] path, HashSet<int> visited)
        {
            // If it's gone through every city, make sure it can get back to the start.
            // If so, this is a valid cycle.
            if(index == path.Length - 1)
            {
                return !Double.IsPositiveInfinity(Costs[path[index],path[0]]);
            }

            // Keep a list of the neighboring cities we've tried that won't work.
            HashSet<int> wontWork = new HashSet<int>();
            while(wontWork.Count < path.Length)
            {
                double desirabilitySum = 0;
                int j = 0;
                bool infiniteDesirability = false;
                // Build a sum of the desirabilities of all the neighboring cities.  This
                // will be used to randomly choose one, weighted based on the desirabilities
                for (; j < path.Length; j++)
                {
                    if(wontWork.Contains(j)) { continue; }

                    double desirability = GetPathDesirability(path[index], j, visited);

                    // This means that this city is infinitely desirable. This can happen if two
                    // cities are on top of each other.
                    if(Double.IsPositiveInfinity(desirability))
                    {
                        infiniteDesirability = true;
                        break;
                    }
                    desirabilitySum += desirability;
                    if (desirability == 0)
                    {
                        wontWork.Add(j);
                    }
                }

                // This means there are no neighboring cities which are desirable, so the route up to this
                // point can't produce a valid, full cycle
                if(desirabilitySum == 0) { return false; }

                int next = 0;
                if (infiniteDesirability) // We found an infinitely good next city, so just use that one
                {
                    next = j;
                } else
                {
                    // Get a number somewhere in the range of possible desirabilities.
                    double choice = random.NextDouble() * desirabilitySum;
                    desirabilitySum = 0;

                    // Find which neighboring city corresponds to the choice we made.
                    // TODO figure out if there is a more efficient way of doing this.
                    for (; next < path.Length; next++)
                    {
                        desirabilitySum += GetPathDesirability(path[index], next, visited);
                        if (desirabilitySum > choice)
                        {
                            break;
                        }
                    }
                }

                // Could't figure out which neighboring city to go to based on the random number.
                if (next == path.Length) { throw new Exception("Something borked, and I'm not sure how..."); }

                // Try finding a path through the chosen city.  If it works, then return that. Otherwise, add
                // it to the list of cities that won't work.
                path[index+1] = next;
                visited.Add(next);
                if (!FindRoute(index+1, ref path, visited))
                {
                    visited.Remove(next);
                    wontWork.Add(next);
                    continue;
                }

                return true;
            }

            return false;
        }

        // Get the desirability of a certain path.  Right now, only takes into account
        // cost, but eventually will take into account pheromones.
        private double GetPathDesirability(int from, int to, HashSet<int> visited) {
            if(visited.Contains(to)) { return 0; }
            if(from == to) { return 0; }

            return 1 / Costs[from, to];
        }
    }
}
