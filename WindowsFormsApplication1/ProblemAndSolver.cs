using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Diagnostics;


namespace TSP
{

    class ProblemAndSolver
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
        /// These three constants are used for convenience/clarity in populating and accessing the 
        /// results array that is passed back to the calling Form
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
                return -1D; 
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
        /// This is the entry point for the default solver
        /// which just finds a valid random tour 
        /// </summary>
        /// <returns>results array for GUI that contains three ints: cost of solution, 
        /// time spent to find solution, number of solutions found during search 
        /// (not counting initial BSSF estimate)</returns>
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
                for (i = 0; i < perm.Length; i++)  // create a random permutation template
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
                for (i = 0; i < Cities.Length; i++) // Now build the route using the random permutation 
                {
                    Route.Add(Cities[perm[i]]);
                }
                bssf = new TSPSolution(Route);
                count++;
            } while (costOfBssf() == double.PositiveInfinity); // until a valid route is found
            timer.Stop();

            results[COST] = costOfBssf().ToString();  // load results array
            results[TIME] = timer.Elapsed.ToString();
            results[COUNT] = count.ToString();

            return results;
        }

        /// <summary>
        /// performs a Branch and Bound search of the state space of partial tours
        /// stops when time limit expires and uses BSSF as solution
        /// </summary>
        /// <returns>results array for GUI that contains three ints: cost of solution, time spent to find solution, number of solutions found during search (not counting initial BSSF estimate)</returns>
        public string[] bBSolveProblem()
        {

            Stopwatch timer = new Stopwatch();
            timer.Start();

            // TODO: Add your implementation for a branch and bound solver here.
            int childStatesGenerated = 0;
            int statesPruned = 0;
            int solutionsFound = 0;
            
            greedySolveProblem(); // the greedy solution
            double greedyCost = costOfBssf();

            BBNode rootNode = makeRootNode();
            PQueue pq = new PQueue();

            pq.makeQueue(Cities.Length);
            pq.insert(rootNode);

            //Console.WriteLine(rootNode.lowerBound.ToString());
            //Console.WriteLine(bssf.costOfRoute());
            //Console.WriteLine("time limit: " + time_limit);

            while (timer.Elapsed < TimeSpan.FromSeconds(time_limit / 1000)
                   && !pq.isEmpty()
                   && pq.getMinLB() != costOfBssf())
            //while (!pq.isEmpty() && pq.getMinLB() != costOfBssf())
            {

                BBNode currentNode = pq.deleteMin();
                double currentBestCost = costOfBssf(); // for debug
                if (currentNode.lowerBound >= costOfBssf())
                {
                    statesPruned++;
                    continue; // discarded since it can't beat the bssf, and skip to loop
                }

                // if the node pulled off the queue is a leaf/potential solution
                if (currentNode.path.Count == Cities.Length)
                {
                    double costBackToFirstCity = ((City)currentNode.path[currentNode.path.Count - 1])
                                                  .costToGetTo((City)rootNode.path[0]);

                    if (costBackToFirstCity == double.PositiveInfinity)
                    {
                        statesPruned++;
                        continue; // discarded since it can't complete the cycle back to first city
                    }
                    else // cost back to first city is FINITE so completes a loop
                    {
                        TSPSolution potentialBssf = new TSPSolution(currentNode.path);
                        double candidateCost = potentialBssf.costOfRoute(); // debug
                        double bssfCost = costOfBssf(); // debug
                        if (potentialBssf.costOfRoute() < costOfBssf())
                        {
                            // new best solution found
                            bssf = potentialBssf;
                            // increment number of solutions found
                            solutionsFound++;
                        }
                    }
                    // Done with currentNode. On to the next node on the queue.
                    continue;
                }

                for (int childCityIndex = 0; childCityIndex < Cities.Length; childCityIndex++)
                {
                    // This ensures that we skip cities already in the path,
                    if (currentNode.path.Contains(Cities[childCityIndex])) continue;

                    // for this potential child state, if the cost to get to that city (from the last city in the
                    // current state's path) is infinite, skip making
                    // it in the first place and just increment statesPruned.
                    City cityAtEndOfCurrentNodePath = (City) currentNode.path[currentNode.path.Count - 1];
                    double costToGetToChildCity = cityAtEndOfCurrentNodePath.costToGetTo(Cities[childCityIndex]);
                    if (costToGetToChildCity == double.PositiveInfinity)
                    {
                        // counts as a creation and a pruning
                        childStatesGenerated++;
                        statesPruned++;
                        continue;
                    }
                    // Okay we're creating a child (even if the cost to that city is infinity - the lowerbound will be too)
                    int currentCityIndex = Array.IndexOf(Cities, cityAtEndOfCurrentNodePath);
                    BBNode childNode = makeChildNode(ref currentNode, currentCityIndex, childCityIndex);

                    // increment childStatesGenerated
                    childStatesGenerated++;

                    // if the child node lower bound is >= the best solution so far, don't add it to the queue
                    if (childNode.lowerBound >= costOfBssf())
                    {
                        statesPruned++;
                        continue;
                    }
                    // otherwise it's going on the queue
                    pq.insert(childNode);

                }

            }

            timer.Stop();

            Console.WriteLine("Child states generated: " + childStatesGenerated);
            Console.WriteLine("Pruned states: " + statesPruned);
            Console.WriteLine("Largest queue size: " + pq.getMaxNumOfItems());

            string[] results = new string[3];
            results[COST] = costOfBssf().ToString();
            results[TIME] = timer.Elapsed.ToString();
            results[COUNT] = solutionsFound.ToString();

            return results;
        }

        private void findInitialBssf()
        {
            Route = new ArrayList();
            for (int startingCityIndex = 0; startingCityIndex < Cities.Length; startingCityIndex++)
            {
                Route.Clear();
                double smallestCost = double.MaxValue;
                int smallestCostCityIndex = -1;
                int fromCityIndex = startingCityIndex;
                while (Route.Count < Cities.Length)
                {
                    for (int c = 0; c < Cities.Length; c++)
                    {
                        // don't allow to self, and don't allow cities already established in Route
                        if (c == fromCityIndex || Route.Contains(Cities[c]))
                        {
                            // skip to the next city (c) since this one is no good
                            continue;
                        }
                        double cCost = Cities[fromCityIndex].costToGetTo(Cities[c]);
                        if (cCost < smallestCost)
                        {
                            smallestCost = cCost;
                            smallestCostCityIndex = c;
                        }
                    }
                    if (smallestCost < double.MaxValue)
                    {
                        // add the city with the smallest cost to the route and make it the next from-city
                        Route.Add(Cities[smallestCostCityIndex]);
                        fromCityIndex = smallestCostCityIndex;
                        // reset smallestCost to maxValue
                        smallestCost = double.MaxValue;
                    }
                    else
                    {
                        // path stops here
                        Console.WriteLine("No path found from city " + smallestCostCityIndex);
                        break;
                    }
                }
                if (Route.Count < Cities.Length)
                {
                    // no path found. Try the next starting city
                    continue;
                }
                else if ( ((City)Route[Route.Count - 1]).costToGetTo(Cities[startingCityIndex]) == double.PositiveInfinity)
                {
                    // no path from last city to starting city, start over again from a different city
                    continue;
                }
                else
                {
                    // cycle complete with a legitimate path
                    Console.WriteLine("Complete cycle found.");
                    bssf = new TSPSolution(Route);
                    return;
                }
            }
        }

        private BBNode makeRootNode()
        {
            ArrayList initialPath = new ArrayList();
            initialPath.Add(Cities[0]);

            int length = Cities.Length;
            double[,] redCostMatrix = new double[length, length];

            for (int orig = 0; orig < length; orig++)
            {
                for (int dest = 0; dest < length; dest++)
                {
                    if (dest == orig)
                    {
                        redCostMatrix[orig, dest] = double.PositiveInfinity;
                    }
                    else
                    {
                        redCostMatrix[orig, dest] = Cities[orig].costToGetTo(Cities[dest]);
                    }
                }
            }

            double lowerBound = calcLowerBound(ref redCostMatrix);
            return new BBNode(ref initialPath, ref redCostMatrix, lowerBound);
        }

        private BBNode makeChildNode(ref BBNode parentNode, int parentCityIndex, int thisCityIndex)
        {
            ArrayList newPath = new ArrayList();
            foreach (City city in parentNode.path)
            {
                newPath.Add(city);
            }
            newPath.Add(Cities[thisCityIndex]);

            int length = Cities.Length;
            double[,] thisCostMatrix = new double[length, length];

            // copy parent cost matrix to this matrix (it will be missing the newly needed changes)
            for (int orig = 0; orig < length; orig++)
            {
                for (int dest = 0; dest < length; dest++)
                {
                    thisCostMatrix[orig, dest] = parentNode.redCostMatrix[orig,dest];
                }
            }

            // The function calcLowerBound also updates the matrix to the reduced cost
            double newLowerBound = calcLowerBound(ref thisCostMatrix, parentCityIndex, thisCityIndex, parentNode.lowerBound);
            return new BBNode(ref newPath, ref thisCostMatrix, newLowerBound);
        }

        // implements the IComparable interface so nodes can be sorted in the priorityqueue
        private class BBNode : IComparable<BBNode>
        {
            public ArrayList path;
            public double[,] redCostMatrix;
            public double lowerBound;
            //public double costSoFar;

            public BBNode(ref ArrayList initialPath, ref double[,] redCostMatrix, double lowerBound)
            {
                this.path = initialPath;
                this.redCostMatrix = redCostMatrix;
                this.lowerBound = lowerBound;

            }

            public int CompareTo(BBNode that)
            {
                ////previous sort:
                //if ((this.lowerBound / this.path.Count) < (that.lowerBound / that.path.Count)) return -1;
                //if ((this.lowerBound / this.path.Count) > (that.lowerBound / that.path.Count)) return 1;
                ////equal gives tie to the former
                //return -1;

                if (this.path.Count > that.path.Count) return -1;
                if (this.path.Count < that.path.Count) return 1;
                if (this.path.Count == that.path.Count)
                {
                    if (this.lowerBound < that.lowerBound) return -1;
                    if (this.lowerBound > that.lowerBound) return 1;
                }
                return -1;


                // TODO: they're the same (unlikely), so it doesn't matter much. But test for this to see if it works.
                // For now I'm letting equal-to allow priority above.
                //return 0;

                //throw new NotImplementedException();
            }

            //public BBNode(double[,] oldRedCostMatrix, int[] parentPath, int fromCity, int toCity,
            //              double prevLowerBound, int prevCostSoFar)
            //{
            //    // this function must also update the value added to the lowerbound somehow
            //    Tuple<double[,], double> redCostMatrixResult = makeRedCostMatrix(oldRedCostMatrix, fromCity, toCity);
            //    redCostMatrix = redCostMatrixResult.Item1;
            //    lowerBound = prevLowerBound + redCostMatrixResult.Item2;
            //    costSoFar = prevCostSoFar + redCostMatrixResult.Item2;

            //    path = new int[parentPath.Length];
            //    for (int i = 0; i < parentPath.Length; i++)
            //    {
            //        path[i] = parentPath[i];
            //    }    

            //}
            //public ArrayList path = new ArrayList();
            //int[,] redCostMatrix = new int[]

        }

        //private static Tuple<double[,], double> makeRedCostMatrix(double[,] oldRedCostMatrix, int fromCity, int toCity)
        //{
        //    //placeholder for now
        //    int citieslength = 4;
        //    double[,] newRedCostMatrix = new double[citieslength, citieslength];
        //    double addedCost = 5;

        //    return new Tuple<double[,], double>(newRedCostMatrix, addedCost);
        //}

        // This is the first of two similar functions. This one finds the reduced-cost matrix and
        // the new lower bound for a CHILD NODE/STATE.
        private double calcLowerBound(ref double[,] costMatrix, int origIndex, int destIndex, double prevLowerbound)
        {
            double addedCost = 0;
            int length = Cities.Length;

            // update the matrix based on the new origin and destination cities.
            // Infinite cost
            if (costMatrix[origIndex, destIndex] == double.PositiveInfinity)
            {
                return double.PositiveInfinity;
            }

            // add the added cost found for picking that edge (which will not be equal to costToGetTo)
            addedCost += costMatrix[origIndex, destIndex];

            // Then fill in infinities
            // infinities accross that row
            for (int dest = 0; dest < length; dest++)
            {
                costMatrix[origIndex, dest] = double.PositiveInfinity;
            }
            // infinities accross that column
            for (int orig = 0; orig < length; orig++)
            {
                costMatrix[orig, destIndex] = double.PositiveInfinity;
            }
            // infinity for the reverse direction (since the destination can't return to the origin)
            costMatrix[destIndex, origIndex] = double.PositiveInfinity;
            
            // check rows for zeroes (or all infinities - for which nothing will change)
            for (int orig = 0; orig < length; orig++)
            {
                double min = double.MaxValue;
                for (int dest = 0; dest < length; dest++)
                {
                    if (costMatrix[orig, dest] == 0)
                    {
                        // zero found so we're done for this row
                        break;
                    }
                    if (costMatrix[orig, dest] < min)
                    {
                        min = costMatrix[orig, dest];
                        // There were no zeroes, so before loop completes, subtract min from all.
                        // Any infinities (including if they're all infinities) will remain infinity.
                        if (dest == length - 1)
                        {
                            for (int j = 0; j < length; j++)
                            {
                                costMatrix[orig, j] -= min;
                            }
                            addedCost += min;
                        }
                    }
                }
            }
            // Check columns for zeroes
            for (int dest = 0; dest < length; dest++)
            {
                double min = double.MaxValue;
                for (int orig = 0; orig < length; orig++)
                {
                    if (costMatrix[orig, dest] == 0)
                    {
                        // zero found so we're done for this column
                        break;
                    }
                    if (costMatrix[orig, dest] < min)
                    {
                        min = costMatrix[orig, dest];
                        // There were no zeroes, so before loop completes, subtract min from all.
                        // Any infinities (including if they're all infinities) will remain infinity.
                        if (orig == length - 1)
                        {
                            for (int i = 0; i < length; i++)
                            {
                                costMatrix[i, dest] -= min;
                            }
                            addedCost += min;
                        }
                    }
                }
            }
            return prevLowerbound + addedCost;
        }

        // An overloaded function for the initial node cost matrix: this one is for just the ROOT node/state
        private double calcLowerBound(ref double[,] costMatrix)
        {
            double addedCost = 0;

            int length = Cities.Length;
            // check rows for zeroes
            for (int orig = 0; orig < length; orig++)
            {
                double min = double.MaxValue;
                for (int dest = 0; dest < length; dest++)
                {
                    if (costMatrix[orig, dest] == 0)
                    {
                        // zero found so we're done for this row
                        break;
                    }
                    if (costMatrix[orig, dest] < min)
                    {
                        min = costMatrix[orig, dest];
                    }
                    // there were no zeroes, so before loop completes, subtract min from all
                    if (dest == length - 1)
                    {
                        for (int j = 0; j < length; j++)
                        {
                            costMatrix[orig, j] -= min;
                        }
                        addedCost += min;
                    }
                }
            }
            // Check columns for zeroes
            for (int dest = 0; dest < length; dest++)
            {
                double min = double.MaxValue;
                for (int orig = 0; orig < length; orig++)
                {
                    if (costMatrix[orig, dest] == 0)
                    {
                        // zero found so we're done for this column
                        break;
                    }
                    if (costMatrix[orig, dest] < min)
                    {
                        min = costMatrix[orig, dest];
                    }
                    // there were no zeroes, so before loop completes, subtract min from all
                    if (orig == length - 1)
                    {
                        for (int i = 0; i < length; i++)
                        {
                            costMatrix[i, dest] -= min;
                        }
                        addedCost += min;
                    }
                }
            }
            return addedCost;
        }

        private class PQueue
        {
            private int capacity;
            private int count;
            private int maxNum;
            private BBNode[] queuedNodes;

            public PQueue() { }

            /**
            * This function returns whether the queue is empty or not.
            * Time and Space = O(1) as it only involves an int comparison
            */
            public bool isEmpty()
            {
                return count == 0;
            }

            public double getMinLB()
            {
                return queuedNodes[1].lowerBound;
            }

            /**
            * This function returns the number of items in the queue
            */
            public int getSize()
            {
                return count;
            }

            /**
            * This method creates an array to implement the queue. Time and Space Complexities are both O(|V|) where |V| is
            * the number of nodes. This is because you create an array of the same size and specify the value for each item by 
            * iterating over the entire array.
            */
            public void makeQueue(int numOfNodes)
            {
                //Starting the array on the small side and will grow it as needed
                int arrayStartingSize = 128;

                queuedNodes = new BBNode[1000000];
                capacity = numOfNodes;
                count = 0;
                maxNum = 0;
            }

            /**
            * This method returns the index of the element with the minimum value and removes it from the queue. 
            * Time Complexity: O(log(|V|)) because removing a node is constant time as we have its position in
            * the queue, then to readjust the heap we just bubble up the min value which takes as long as 
            * the depth of the tree which is log(|V|), where |V| is the number of nodes
            * Space Complexity: O(1) because we don't create any extra variables that vary with the size of the input.
            */
            public BBNode deleteMin()
            {
                // grab the node with min value which will be at the root
                BBNode minValue = queuedNodes[1];
                //queuedNodes[1].setPriority(double.MaxValue);
                queuedNodes[1] = queuedNodes[count];
                count--;
                // fix the heap
                int indexIterator = 1;
                while (indexIterator <= count)
                {
                    // grab left child
                    int smallerElementIndex = 2 * indexIterator;

                    // if child does not exist, break
                    if (smallerElementIndex > count)
                        break;

                    // if right child exists and is of smaller value, pick it
                    if (smallerElementIndex + 1 <= count
                        && queuedNodes[smallerElementIndex + 1].CompareTo(queuedNodes[smallerElementIndex]) < 0)
                    {
                        smallerElementIndex++;
                    }

                    if (queuedNodes[indexIterator].CompareTo(queuedNodes[smallerElementIndex]) > 0)
                    {
                        // set the node's value to that of its smaller child
                        BBNode temp = queuedNodes[smallerElementIndex];
                        queuedNodes[smallerElementIndex] = queuedNodes[indexIterator];
                        queuedNodes[indexIterator] = temp;
                    }

                    indexIterator = smallerElementIndex;
                }
                // return the min value
                return minValue;
            }

            /**
            * This function returns the maximum number of items ever put in the queue
            */
            public int getMaxNumOfItems()
            {
                return maxNum;
            }
            /**
            * This method updates the nodes in the queue after inserting a new node
            * Time Complexity: O(log(|V|)) as reording the heap works by bubbling up the min value to the top
            * which takes as long as the depth of the tree which is log|V|.
            * Space Complexity: O(1) as it does not create any extra variables that vary with the size of the input.
            */
            public void insert(BBNode newBBNode)
            {
                // update the count and if necessary increase the size of the array
                count++;

                //if (count == queuedNodes.Length - 1)
                //{
                //    BBNode[] tempArray = new BBNode[count * 2];
                //    for (int i = 1; i < count; i++)
                //    {
                //        tempArray[i] = queuedNodes[i];
                //    }
                //    queuedNodes = tempArray;
                //    Console.WriteLine("new array of size" + count * 2);
                //}

                queuedNodes[count] = newBBNode;
                if (count > maxNum) maxNum = count;

                // as long as its parent has a larger value and have not hit the root
                int indexIterator = count;
                while (indexIterator > 1 && queuedNodes[indexIterator / 2].CompareTo(queuedNodes[indexIterator]) > 0)
                {
                    // swap the two nodes
                    BBNode temp = queuedNodes[indexIterator / 2];
                    queuedNodes[indexIterator / 2] = queuedNodes[indexIterator];
                    queuedNodes[indexIterator] = temp;

                    indexIterator /= 2;
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
