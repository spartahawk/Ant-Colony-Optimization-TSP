using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSP
{
    /**
     * Provides some default methods for the PriorityQueue interface
     * and contains the Element inner class
     */
    abstract class AbstractPriorityQueue<T, K> : IPriorityQueue<T, K> where K : IComparable<K>
    {
        public abstract int Size { get; }

        public abstract void Clear();
        public abstract Tuple<T, K> GetLowest();
        public abstract void Insert(T item, K priority);

        public abstract bool IsEmpty();

        public abstract int DeleteElementsHigherThan(K priority);

        /**
         * This is not defined in the interface, but in this abstract class.
         * Subclasses should return the Element that corresponds the the given
         * item.
         */
        protected abstract Element GetElement(T item);
        
        public K GetPriority(T item)
        {
            return GetElement(item).Priority;
        }

        protected class Element : IComparable<Element>
        {
            public Element(T item, K priority)
            {
                this.item = item;
                this.priority = priority;
            }

            public int CompareTo(Element o) {
                return priority.CompareTo(o.priority);
            }

            private K priority;
            public K Priority {
                get { return priority; }
                set { priority = value; }
            }

            private T item;
            public T Item
            {
                get { return item; }
            }
        }
    }
}
