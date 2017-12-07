using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSP
{
    class HeapPriorityQueue<T, K> : AbstractPriorityQueue<T, K> where K : IComparable<K>
    {
        private int size;
        public override int Size
        {
            get { return size; }
        }

        // Stores all of the elements.
        private Element[] elements;

        // Provides a way to get the current index of an item without
        // iterating over all of the elements.
        private Dictionary<T, int> indices = new Dictionary<T, int>();

        public HeapPriorityQueue()
        {
            elements = new Element[20];
        }

        public HeapPriorityQueue(int suggestedSize)
        {
            elements = new Element[suggestedSize];
        }

        public override Tuple<T, K> GetLowest()
        {
            // Temporarily hold on to the root of the heap,
            // the item which will be returned.
            Element ret = elements[0];

            // Put the element at the end of the array into the spot
            // for the root of the heap.
            size--;
            elements[0] = elements[size];
            elements[size] = null;

            // Update the indices dictionary.
            if (size != 0)
                indices[elements[0].Item] = 0;
            indices[ret.Item] = -1;

            // Bubble the new root down, if necessary.
            BubbleIndexDown(0);

            if(size < elements.Length / 2)
            {
                ShrinkArray();
            }

            if (indices.Count > size * 1.2)
            {
                CleanIndices();
            }

            return new Tuple<T, K>(ret.Item, ret.Priority);
        }

        /**
         * If a node is greater than either of it's children,
         * this will swap it with the lower of them, and then
         * continue the downward bubbling.
         */
        private void BubbleIndexDown(int index)
        {
            if(DoesIndexHaveChildren(index))
            {
                int lower = GetLowestChildOfIndex(index);
                if(elements[index].Priority.CompareTo(elements[lower].Priority) > 0)
                {
                    // Swap the current node with the child
                    Element temp = elements[index];
                    elements[index] = elements[lower];
                    elements[lower] = temp;

                    // update indices
                    indices[elements[index].Item] = index;
                    indices[elements[lower].Item] = lower;

                    // continue downward!
                    BubbleIndexDown(lower);
                }
            }
        }

        /**
         * Checks if a node actually has children. Since we're using
         * a complete binary tree, this node has children if there are
         * enough that it should.
         */ 
        private bool DoesIndexHaveChildren(int index)
        {
            return (index * 2 + 1) < size;
        }

        /**
         * Gets the child with the lower priority. This should only
         * be called if the node has at least one child.
         */
        private int GetLowestChildOfIndex(int index)
        {
            int left = index * 2 + 1;
            int right = index * 2 + 2;
            if (right >= size || elements[left].Priority.CompareTo(elements[right].Priority) < 0)
            {
                return left;
            }

            return right;
        }

        /**
         * Inserts the item at the given priority.  It will append
         * it to the end of the heap, and then bubble up as necessary
         */
        public override void Insert(T item, K priority)
        {
            elements[size] = new Element(item, priority);
            indices[item] = size;
            BubbleIndexUp(size);
            size++;

            if(size == elements.Length)
            {
                GrowArray();
            }

            if(indices.Count > size * 1.2)
            {
                CleanIndices();
            }
        }

        /**
         * Bubbles the given node up.  If the node's priority is
         * lower than its parent, it will swap it with its parent
         * and then continue bubbling up.
         */
        private void BubbleIndexUp(int index)
        {
            if (index == 0) { return; }

            int parent = (index - 1) / 2;
            if (elements[parent].Priority.CompareTo(elements[index].Priority) > 0)
            {
                // Swap with the parent.
                Element temp = elements[index];
                elements[index] = elements[parent];
                elements[parent] = temp;

                // Update indices
                indices[elements[index].Item] = index;
                indices[elements[parent].Item] = parent;

                // Continue bubbling up
                BubbleIndexUp(parent);
            }
        }

        public override bool IsEmpty()
        {
            return size == 0;
        }

        public override void Clear()
        {
            size = 0;
            elements = new Element[elements.Length];
            indices = new Dictionary<T, int>();
        }

        protected override Element GetElement(T item)
        {
            return elements[indices[item]];
        }

        private void GrowArray()
        {
            Element[] newElements = new Element[elements.Length * 2];
            Array.Copy(elements, newElements, elements.Length);
            elements = newElements;
        }

        private void ShrinkArray()
        {
            Element[] newElements = new Element[elements.Length / 2];
            Array.Copy(elements, newElements, newElements.Length);
            elements = newElements;
        }

        private void CleanIndices()
        {
            Dictionary<T, int> newIndices = new Dictionary<T, int>((int)(elements.Length * 1.1));
            foreach(KeyValuePair<T, int> pair in indices)
            {
                if(pair.Value != -1)
                {
                    newIndices[pair.Key] = pair.Value;
                }
            }
            indices = newIndices;
        }

        public override int DeleteElementsHigherThan(K priority)
        {
            int origSize = size;
            Element[] newElements = new Element[size];
            int next = 0;
            for(int i = 0; i < size; i++)
            {
                Element el = elements[i];
                if(el.Priority.CompareTo(priority) < 0)
                {
                    newElements[next] = el;
                    next++;
                }
            }

            Array.Sort(newElements, (a, b) => a == null ? 1 : b == null ? -1 : (a.Priority.CompareTo(b.Priority)));

            Dictionary<T, int> newIndices = new Dictionary<T, int>((int)(next * 1.1));
            for(int i = 0; i < next; i++) {
                newIndices[newElements[i].Item] = i;
            }

            elements = newElements;
            indices = newIndices;
            size = next;

            return origSize - size;
        }
    }
}

