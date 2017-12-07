using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSP
{
    interface IPriorityQueue<T, K> where K : IComparable<K>
    {
        int Size { get; }

        /**
         * Inserts item with the given priority.  Lower priorities will
         * be removed first.
         */
        void Insert(T item, K priority);

        /**
         * Gets and removes the item with the lowest priority.
         */
        Tuple<T, K> GetLowest();

        /**
         * Checks if the queue is empty.
         */
        bool IsEmpty();

        /**
         * Empties the queue.
         */
        void Clear();

        /**
         * Gets the priority of a the given item if it is in the queue.
         */
        K GetPriority(T item);

        int DeleteElementsHigherThan(K priority);
    }
}
