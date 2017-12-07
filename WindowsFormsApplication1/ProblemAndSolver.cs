using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace TSP
{
    partial class ProblemAndSolver
    {

        private class TSPSolution
        {
            /// <summary>
            /// we use the representation [cityB,cityA,cityC] 
            /// to mean that cityB is the first city in the solution, cityA is the second, cityC is the third 
            /// and the edge from cityC to cityB is the final edge in the path.  
            /// You are, of course, free to use a different representation if it would be more convenient or efficient 
            /// for your data structure(s) and search algorithm. 
            /// </summary>
            public ArrayList
                Route;

            /// <summary>
            /// constructor
            /// </summary>
            /// <param name="iroute">a (hopefully) valid tour</param>
            public TSPSolution(ArrayList iroute)
            {
                Route = new ArrayList(iroute);
            }

            /// <summary>
            /// Compute the cost of the current route.  
            /// Note: This does not check that the route is complete.
            /// It assumes that the route passes from the last city back to the first city. 
            /// </summary>
            /// <returns></returns>
            public double costOfRoute()
            {
                // go through each edge in the route and add up the cost. 
                int x;
                City here;
                double cost = 0D;

                for (x = 0; x < Route.Count - 1; x++)
                {
                    here = Route[x] as City;
                    cost += here.costToGetTo(Route[x + 1] as City);
                }

                // go from the last city to the first. 
                here = Route[Route.Count - 1] as City;
                cost += here.costToGetTo(Route[0] as City);
                return cost;
            }
        }

        #region Private members 

        /// <summary>
        /// Default number of cities (unused -- to set defaults, change the values in the GUI form)
        /// </summary>
        // (This is no longer used -- to set default values, edit the form directly.  Open Form1.cs,
        // click on the Problem Size text box, go to the Properties window (lower right corner), 
        // and change the "Text" value.)
        private const int DEFAULT_SIZE = 25;

        /// <summary>
        /// Default time limit (unused -- to set defaults, change the values in the GUI form)
        /// </summary>
        // (This is no longer used -- to set default values, edit the form directly.  Open Form1.cs,
        // click on the Time text box, go to the Properties window (lower right corner), 
        // and change the "Text" value.)
        private const int TIME_LIMIT = 60;        //in seconds

        private const int CITY_ICON_SIZE = 5;


        // For normal and hard modes:
        // hard mode only
        private const double FRACTION_OF_PATHS_TO_REMOVE = 0.20;

        /// <summary>
        /// the cities in the current problem.
        /// </summary>
        private City[] Cities;
        /// <summary>
        /// a route through the current problem, useful as a temporary variable. 
        /// </summary>
        private ArrayList Route;
        /// <summary>
        /// best solution so far. 
        /// </summary>
        private TSPSolution bssf; 

        /// <summary>
        /// how to color various things. 
        /// </summary>
        private Brush cityBrushStartStyle;
        private Brush cityBrushStyle;
        private Pen routePenStyle;


        /// <summary>
        /// keep track of the seed value so that the same sequence of problems can be 
        /// regenerated next time the generator is run. 
        /// </summary>
        private int _seed;
        /// <summary>
        /// number of cities to include in a problem. 
        /// </summary>
        private int _size;

        /// <summary>
        /// Difficulty level
        /// </summary>
        private HardMode.Modes _mode;

        /// <summary>
        /// random number generator. 
        /// </summary>
        private Random rnd;

        /// <summary>
        /// time limit in milliseconds for state space search
        /// can be used by any solver method to truncate the search and return the BSSF
        /// </summary>
        private int time_limit;
        #endregion

        #region Public members

        /// <summary>
        /// These three constants are used for convenience/clarity in populating and accessing the results array that is passed back to the calling Form
        /// </summary>
        public const int COST = 0;           
        public const int TIME = 1;
        public const int COUNT = 2;
        
        public int Size
        {
            get { return _size; }
        }

        public int Seed
        {
            get { return _seed; }
        }
        #endregion

        #region Constructors
        public ProblemAndSolver()
        {
            this._seed = 1; 
            rnd = new Random(1);
            this._size = DEFAULT_SIZE;
            this.time_limit = TIME_LIMIT * 1000;                  // TIME_LIMIT is in seconds, but timer wants it in milliseconds

            this.resetData();
        }

        public ProblemAndSolver(int seed)
        {
            this._seed = seed;
            rnd = new Random(seed);
            this._size = DEFAULT_SIZE;
            this.time_limit = TIME_LIMIT * 1000;                  // TIME_LIMIT is in seconds, but timer wants it in milliseconds

            this.resetData();
        }

        public ProblemAndSolver(int seed, int size)
        {
            this._seed = seed;
            this._size = size;
            rnd = new Random(seed);
            this.time_limit = TIME_LIMIT * 1000;                        // TIME_LIMIT is in seconds, but timer wants it in milliseconds

            this.resetData();
        }
        public ProblemAndSolver(int seed, int size, int time)
        {
            this._seed = seed;
            this._size = size;
            rnd = new Random(seed);
            this.time_limit = time*1000;                        // time is entered in the GUI in seconds, but timer wants it in milliseconds

            this.resetData();
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Reset the problem instance.
        /// </summary>
        private void resetData()
        {

            Cities = new City[_size];
            Route = new ArrayList(_size);
            bssf = null;

            if (_mode == HardMode.Modes.Easy)
            {
                for (int i = 0; i < _size; i++)
                    Cities[i] = new City(rnd.NextDouble(), rnd.NextDouble());
            }
            else // Medium and hard
            {
                for (int i = 0; i < _size; i++)
                    Cities[i] = new City(rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble() * City.MAX_ELEVATION);
            }

            HardMode mm = new HardMode(this._mode, this.rnd, Cities);
            if (_mode == HardMode.Modes.Hard)
            {
                int edgesToRemove = (int)(_size * FRACTION_OF_PATHS_TO_REMOVE);
                mm.removePaths(edgesToRemove);
            }
            City.setModeManager(mm);

            cityBrushStyle = new SolidBrush(Color.Black);
            cityBrushStartStyle = new SolidBrush(Color.Red);
            routePenStyle = new Pen(Color.Blue,1);
            routePenStyle.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// make a new problem with the given size.
        /// </summary>
        /// <param name="size">number of cities</param>
        public void GenerateProblem(int size, HardMode.Modes mode)
        {
            this._size = size;
            this._mode = mode;
            resetData();
        }

        /// <summary>
        /// make a new problem with the given size, now including timelimit paremeter that was added to form.
        /// </summary>
        /// <param name="size">number of cities</param>
        public void GenerateProblem(int size, HardMode.Modes mode, int timelimit)
        {
            this._size = size;
            this._mode = mode;
            this.time_limit = timelimit*1000;                                   //convert seconds to milliseconds
            resetData();
        }

        /// <summary>
        /// return a copy of the cities in this problem. 
        /// </summary>
        /// <returns>array of cities</returns>
        public City[] GetCities()
        {
            City[] retCities = new City[Cities.Length];
            Array.Copy(Cities, retCities, Cities.Length);
            return retCities;
        }

        /// <summary>
        /// draw the cities in the problem.  if the bssf member is defined, then
        /// draw that too. 
        /// </summary>
        /// <param name="g">where to draw the stuff</param>
        public void Draw(Graphics g)
        {
            float width  = g.VisibleClipBounds.Width-45F;
            float height = g.VisibleClipBounds.Height-45F;
            Font labelFont = new Font("Arial", 10);

            // Draw lines
            if (bssf != null)
            {
                // make a list of points. 
                Point[] ps = new Point[bssf.Route.Count];
                int index = 0;
                foreach (City c in bssf.Route)
                {
                    if (index < bssf.Route.Count -1)
                        g.DrawString(" " + index +"("+c.costToGetTo(bssf.Route[index+1]as City)+")", labelFont, cityBrushStartStyle, new PointF((float)c.X * width + 3F, (float)c.Y * height));
                    else 
                        g.DrawString(" " + index +"("+c.costToGetTo(bssf.Route[0]as City)+")", labelFont, cityBrushStartStyle, new PointF((float)c.X * width + 3F, (float)c.Y * height));
                    ps[index++] = new Point((int)(c.X * width) + CITY_ICON_SIZE / 2, (int)(c.Y * height) + CITY_ICON_SIZE / 2);
                }

                if (ps.Length > 0)
                {
                    g.DrawLines(routePenStyle, ps);
                    g.FillEllipse(cityBrushStartStyle, (float)Cities[0].X * width - 1, (float)Cities[0].Y * height - 1, CITY_ICON_SIZE + 2, CITY_ICON_SIZE + 2);
                }

                // draw the last line. 
                g.DrawLine(routePenStyle, ps[0], ps[ps.Length - 1]);
            }

            // Draw city dots
            foreach (City c in Cities)
            {
                g.FillEllipse(cityBrushStyle, (float)c.X * width, (float)c.Y * height, CITY_ICON_SIZE, CITY_ICON_SIZE);
            }

        }

        /// <summary>
        ///  return the cost of the best solution so far. 
        /// </summary>
        /// <returns></returns>
        public double costOfBssf ()
        {
            if (bssf != null)
                return (bssf.costOfRoute());
            else
                return Double.PositiveInfinity; 
        }

        /// <summary>
        /// This is the entry point for the default solver
        /// which just finds a valid random tour 
        /// </summary>
        /// <returns>results array for GUI that contains three ints: cost of solution, time spent to find solution, number of solutions found during search (not counting initial BSSF estimate)</returns>
        public string[] defaultSolveProblem()
        {
            int i, swap, temp, count=0;
            string[] results = new string[3];
            int[] perm = new int[Cities.Length];
            Route = new ArrayList();
            Random rnd = new Random();
            Stopwatch timer = new Stopwatch();

            timer.Start();

            do
            {
                for (i = 0; i < perm.Length; i++)                                 // create a random permutation template
                    perm[i] = i;
                for (i = 0; i < perm.Length; i++)
                {
                    swap = i;
                    while (swap == i)
                        swap = rnd.Next(0, Cities.Length);
                    temp = perm[i];
                    perm[i] = perm[swap];
                    perm[swap] = temp;
                }
                Route.Clear();
                for (i = 0; i < Cities.Length; i++)                            // Now build the route using the random permutation 
                {
                    Route.Add(Cities[perm[i]]);
                }
                bssf = new TSPSolution(Route);
                count++;
            } while (costOfBssf() == double.PositiveInfinity);                // until a valid route is found
            timer.Stop();

            results[COST] = costOfBssf().ToString();                          // load results array
            results[TIME] = timer.Elapsed.ToString();
            results[COUNT] = count.ToString();

            return results;
        }

        // Gets a nice 2d array of costs.
        private double[,] GetCosts()
        {
            double[,] costs = new double[Cities.Length, Cities.Length];
            for (int i = 0; i < Cities.Length; i++)
            {
                for (int j = 0; j < Cities.Length; j++)
                {
                    if (i == j)
                    {
                        costs[i, j] = Double.PositiveInfinity;
                    }
                    else
                    {
                        costs[i, j] = Cities[i].costToGetTo(Cities[j]);
                        costs[j, i] = Cities[j].costToGetTo(Cities[i]);
                    }
                }
            }
            return costs;
        }

        /// <summary>
        /// performs a Branch and Bound search of the state space of partial tours
        /// stops when time limit expires and uses BSSF as solution
        /// </summary>
        /// <returns>results array for GUI that contains three ints: cost of solution, time spent to find solution, number of solutions found during search (not counting initial BSSF estimate)</returns>
        public string[] bBSolveProblem()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            string[] results = new string[3];

            // Get a default (random) solution
            greedySolveProblem();

            double[,] costs = GetCosts();

            Matrix reduced = new Matrix(Cities, costs, 0).Reduce();

            // Initialize the count of solutions, and the current upper bound
            // Both of these are passed by reference to ProcessBBNode which can
            // update them.
            int count = 0;
            double upper = bssf.costOfRoute();

            // Initialize the total number of nodes, those pruned and the highest
            // queue size. Both are passed to the ProcessBBNode by reference.
            int totalStates = 0;
            int pruned = 0;
            int maxQueueSize = 1;

            IPriorityQueue<BBNode, BBPriority> queue = new HeapPriorityQueue<BBNode, BBPriority>(Cities.Length * 2);

            queue.Insert(new BBNode(new List<int> { 0 }, reduced, 0), new BBPriority(1, reduced.LowerBound));

            while (stopwatch.Elapsed.TotalMilliseconds < time_limit && !queue.IsEmpty())
            {
                // Get the best candidate node.  This is a combination of depth and cost.
                Tuple<BBNode,BBPriority> el = queue.GetLowest();

                // If its cost is higher than the current upper bound, we're done!
                if (el.Item1.Cost > upper)
                {
                    break;
                }

                ProcessBBNode(queue, el, ref count, ref upper, ref totalStates, ref pruned, ref maxQueueSize);
            }

            pruned += queue.Size;

            results[COST] = costOfBssf().ToString();
            results[TIME] = stopwatch.Elapsed.ToString();
            results[COUNT] = count.ToString();

            return results;
        }

        // Processes a node in the branch and bound tree
        // If it is a leaf node and is better than the current best solution,
        // set the bssf to this node.  If it is not a leaf, add nodes for each
        // unvisited city which is lower than the current upper bound.
        private void ProcessBBNode(
            IPriorityQueue<BBNode, BBPriority> queue,
            Tuple<BBNode,BBPriority> el,
            ref int count,
            ref double upper,
            ref int totalStates,
            ref int pruned,
            ref int maxQueueSize
            )
        {
            BBNode top = el.Item1;

            // Check if the route has passed through every city
            if (top.Cities.Count == Cities.Length)
            {
                // Add the trip back to the starting node.
                double cost = top.Cost + top.Matrix.Costs[top.Cities[top.Cities.Count-1], 0];
                if (cost < upper)
                {
                    upper = cost;
                    count++;

                    // CLear any nodes with higher cost than the new upper. DEPTH_NONE indicates
                    // that only cost should be considered, and not depth.  Add the pruned nodes
                    // to the pruned count.
                    pruned += queue.DeleteElementsHigherThan(new BBPriority(BBPriority.DEPTH_NONE, upper));

                    ArrayList cities = new ArrayList();
                    foreach (int city in top.Cities)
                    {
                        cities.Add(Cities[city]);
                    }


                    bssf = new TSPSolution(cities);
                }
            }
            else
            {
                // Add nodes to the queue for each potential route out of the current city
                int lastCity = top.Cities[top.Cities.Count - 1];
                for (int i = 0; i < top.Matrix.Costs.GetLength(0); i++)
                {
                    if (!Double.IsInfinity(top.Matrix.Costs[lastCity, i]))
                    {
                        Matrix newMatrix = top.Matrix.FollowRoute(lastCity, i);
                        double newCost = el.Item2.Cost
                                            + top.Matrix.Costs[lastCity, i]
                                            + (newMatrix.LowerBound - top.Matrix.LowerBound);

                        totalStates++;

                        // If the prospective state is lower cost than the bssf, add it to the queue
                        if (newCost < upper)
                        {
                            List<int> newCities = top.Cities.Concat(new List<int> { i }).ToList();
                            queue.Insert(
                                new BBNode(newCities, newMatrix, newCost),
                                new BBPriority(newCities.Count, newCost)
                            );
                        } else
                        {
                            pruned++;
                        }
                    }
                }

                if(queue.Size > maxQueueSize)
                {
                    maxQueueSize = queue.Size;
                }
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////////
        // These additional solver methods will be implemented as part of the group project.
        ////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// finds the greedy tour starting from each city and keeps the best (valid) one
        /// </summary>
        /// <returns>results array for GUI that contains three ints: cost of solution, time spent to find solution, number of solutions found during search (not counting initial BSSF estimate)</returns>
        public string[] greedySolveProblem()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            bssf = null;

            string[] results = new string[3];

            double[,] costs = GetCosts();

            int count = 0;
            int[] route = new int[Cities.Length];

            for(int i = 0; i < Cities.Length; i++) {
                findGreedyFromCity:
                route[0] = i;

                HashSet<int> visited = new HashSet<int>();
                visited.Add(i);

                for(int j = 1; j < Cities.Length; j++) {
                    int lowest = -1;
                    int from = route[j-1];
                    for(int k = 0; k < Cities.Length; k++) {
                        if(!visited.Contains(k)) {
                            if(lowest == -1 || costs[from,k] < costs[from,lowest]) {
                                lowest = k;
                            }
                        }
                    }
                    if(costs[from,lowest] == Double.PositiveInfinity) {
                        goto findGreedyFromCity;
                    }
                    route[j] = lowest;
                    visited.Add(lowest);
                }

                ArrayList cities = new ArrayList();
                foreach (int city in route)
                {
                    cities.Add(Cities[city]);
                }

                TSPSolution sol = new TSPSolution(new ArrayList(cities));
                if(bssf == null || sol.costOfRoute() < bssf.costOfRoute()) {
                    bssf = sol;
                    count++;
                }
            }

            results[COST] = costOfBssf().ToString();
            results[TIME] = stopwatch.Elapsed.ToString();
            results[COUNT] = count.ToString();

            return results;
        }

        public string[] fancySolveProblem()
        {
            string[] results = new string[3];

            // TODO: Add your implementation for your advanced solver here.

            results[COST] = "not implemented";    // load results into array here, replacing these dummy values
            results[TIME] = "-1";
            results[COUNT] = "-1";

            return results;
        }
        #endregion
    }

}
