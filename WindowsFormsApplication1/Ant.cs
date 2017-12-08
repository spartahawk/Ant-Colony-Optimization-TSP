using System;
using System.Collections.Generic;

namespace TSP
{
    public class Ant
    {
        public double [,] Costs { get; }
        public double [,] Trails { get; set; }

        public Ant(double[,] costs, int randomSeed)
        {
            Costs = costs;
            Trails = new double[costs.GetLength(0), costs.GetLength(0)];
            random = new Random(randomSeed);
        }


        private Random random;
        // TODO rewrite, goes into infinite loop sometimes...
        public double[,] FindRoute() {
            while(true) {
                findroute:
                int[] path = new int[Costs.GetLength(0)];
                HashSet<int> visited = new HashSet<int>();

                path[0] = random.Next(path.Length);
                visited.Add(path[0]);
            
                for(int i = 1; i < path.Length; i++) {
                    double desirabilitySum = 0;
                    for( int j = 0; j < path.Length; j++) {
                        desirabilitySum += GetPathDesirability(path[i-1], j, visited);
                    }
	
                    double choice = random.NextDouble() * desirabilitySum;
                    desirabilitySum = 0;
                
                    int next = 0;
                    for(; next < path.Length; next++) {
                        desirabilitySum += GetPathDesirability(path[i-1], next, visited);
                        if(desirabilitySum > choice) {
                            break;
                        }
                    }
	
                    if(next == path.Length) { goto findroute; }
	
                    path[i] = next;
                    visited.Add(next);
                }
	
                if(Double.IsPositiveInfinity(Costs[path[path.Length-1],path[0]])) {
                    continue;
                }

                Console.WriteLine(string.Join(",", (int[])path));
                return null;
            }
        }

        private double GetPathDesirability(int from, int to, HashSet<int> visited) {
            if(visited.Contains(to)) { return 0; }
            if(from == to) { return 0; }

            return 1 / Costs[from, to];
        }
    }
}
