using System;
using System.Collections.Generic;

namespace TSP
{
    public class Ant
    {
        public double [,] Costs { get; }
        public int [] AntRoute { get; set; }
        public double RouteCost { get; set; }

        public Ant(ref double[,] COSTS, int randomSeed)
        {
            Costs = COSTS;
            random = new Random(randomSeed);
        }

        private Random random;
        public void FindRoute(ref double[,] existingPheromones) {
            int[] path = new int[Costs.GetLength(0)];
            HashSet<int> visited = new HashSet<int>();

            // Choose a random city to start from
            path[0] = random.Next(path.Length);
            visited.Add(path[0]);

            // Is a path possible?
            bool success = FindRoute(0, ref path, visited, ref existingPheromones);

            if (success)
            {
                // If so, store that path.
                AntRoute = path;
                // Find and store the route cost
                StoreRouteCost();
            }
            else
            {
                Console.WriteLine("Setting Route to null. Edit this output to debug.");
                AntRoute = null;
            }
        }

        public void StoreRouteCost()
        {
            // Figure out the cost for the tour
            // Since we're computing this, we might as well save it
            // so as to not need to compute it again later.
            RouteCost = 0;
            for (int i = 0; i < AntRoute.Length - 1; i++)
            {
                RouteCost += Costs[AntRoute[i], AntRoute[i + 1]];
            }
            RouteCost += Costs[AntRoute[AntRoute.Length - 1], AntRoute[0]];
        }

        // This is being replaced by DepositPheromones()
        //public static double[,] GetPheromonesForPath()
        //{
            //// Pheromone added to each segment = 1/(tour cost)
            //double pheromoneStrength = 1 / RouteCost;
            //double[,] result = new double[AntRoute.Length, AntRoute.Length];

            // Make a matrix with pheromone added to each edge that was used.
            //for(int i = 0; i < AntRoute.Length - 1; i++)
            //{
                //result[AntRoute[i], AntRoute[i + 1]] = pheromoneStrength;
            //}
            //result[AntRoute[AntRoute.Length - 1], AntRoute[0]] = pheromoneStrength;

            //return result;
        //}

        public void DepositPheromones(ref double[,] existingPheromones)
        {
            double pheromoneStrength = 1 / RouteCost;
            for (int i = 0; i < AntRoute.Length - 1; i++)
            {
                existingPheromones[AntRoute[i], AntRoute[i + 1]] += pheromoneStrength;
            }
            // deposit pheramone on the edge from the last city to the first city too
            existingPheromones[AntRoute[AntRoute.Length - 1], AntRoute[0]] += pheromoneStrength;
        }

        private bool FindRoute(int index, ref int[] path, HashSet<int> visited, ref double[,] existingPheromones)
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

                    double desirability = GetEdgeDesirability(path[index], j, visited, ref existingPheromones);

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
                        if (!wontWork.Contains(next))
                        {
                            desirabilitySum += GetEdgeDesirability(path[index], next, visited, ref existingPheromones);
                            if (desirabilitySum > choice)
                            {
                                break;
                            }
                        }
                    }
                }

                // Could't figure out which neighboring city to go to based on the random number.
                if (next == path.Length) { throw new Exception("Something borked, and I'm not sure how..."); }

                // Try finding a path through the chosen city.  If it works, then return that. Otherwise, add
                // it to the list of cities that won't work.
                path[index+1] = next;
                visited.Add(next);
                if (!FindRoute(index+1, ref path, visited, ref existingPheromones))
                {
                    visited.Remove(next);
                    wontWork.Add(next);
                    continue;
                }

                return true;
            }

            return false;
        }

        // Get the desirability of a certain edge. Based on an inverse relationship with cost,
        // and a positive relationship with existingPheramones, normalized to 1.
        private double GetEdgeDesirability(int from, int to, HashSet<int> visited, ref double[,] existingPheromones)
        {
            if(visited.Contains(to)) { return 0; }
            if(from == to) { return 0; }

            return 1 / ( Costs[from, to] / existingPheromones[from, to] );
        }
    }
}
