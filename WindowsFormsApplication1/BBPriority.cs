using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP
{
    // Represents the priority of a node in the Priority queue
    // for Branch and Bound TSP
    class BBPriority : IComparable<BBPriority>
    {
        public const int DEPTH_NONE = -1;

        public double Cost { get; }
        public int Depth { get; }

        public BBPriority(int depth, double cost)
        {
            Depth = depth;
            Cost = cost;
        }

        public int CompareTo(BBPriority obj)
        {
            // DEPTH_NONE is used to signal that depth is
            // unimportant, and only cost should be used.
            // This is used when clearing elements above a
            // certain cost.
            if(Depth != -1 && obj.Depth != DEPTH_NONE)
            {
                // When comparing priorities, if there is a
                // difference of more than two in depth, the
                // deeper node is given precedence.
                if(Depth > obj.Depth + 2)
                {
                    return -1;
                }
                if(Depth < obj.Depth - 2)
                {
                    return 1;
                }
            }
            // If neither node is more than two deeper than the
            // other (or depth is not important), just compare
            // costs.
            return Cost.CompareTo(obj.Cost);
        }
    }
}
