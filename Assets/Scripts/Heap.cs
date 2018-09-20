/*
 # Info About Heap Data Structure. Delete this if you do not 
 # need any more explanations.
 *
 * Heap Data Structure is the most efficient implementation of 
 * a priority queue. Priority Queues are abstract data type just like
 * stack or queue but each elements in it has associated a value called 
 * "priority". Element with high priority are served first. A common 
 * implementation is the binary heap which has two constrains: 
 *
 *  - An Heap Property: the key (the value) stored in each node is either greater 
 *                      than or equal to (≥) or less than or equal to (≤) 
 *                      the keys in the node's children.
 * 
 *  - The Shape       : A binary heap is a complety binary tree: every node 
 *                      must can have 0, 1 or 2 child and every node is father
 * 						of his own children.
 */

using System.Collections;
using System.Collections.Generic;
using System;

namespace BornFrustrated.Pathfinding
{
    public class Heap<T> where T : IHeapElement<T>
    {
		T[] m_items;
		int m_count;

		/// <summary>
		/// Total elements in Heap.
		/// </summary>
		/// <value></value>
		public int Count {	get { return m_count; } }

		/// <summary>
		/// Create an empty
		/// </summary>
		/// <param name="_maxSize">Max heap size.</param>
		public Heap(int _maxSize)
		{
			m_items = new T[_maxSize];
		}

		/// <summary>
		/// Check if an item exists in list
		/// </summary>
		/// <param name="item">Item to check</param>
		/// <returns>return true if elements exists or false if not.</returns>
		public bool Contains(T item) 
		{
			return Equals(m_items[item.HeapIndex], item);
		}

		/// <summary>
		/// Add a new key at the bottom of the heap data structure 
		/// and perform an "Heap Up" operation, using SortUp().
		/// </summary>
		/// <param name="_item">Item to add</param>
		public void Add(T _item)
		{
			_item.HeapIndex 	 = m_count;
			m_items[m_count] = _item;
			SortUp(_item);

			m_count ++;
		}

		/// <summary>
		/// Remove First element and replace it with last element
		/// in the structure. Then sort the heap data structure and
		/// perform a Down-Heap operation by using SortDown(). 
		/// </summary>
		/// <return>New First Element</return>
		public T RemoveFirst()
		{
			T _firstElement = m_items[0];
			m_count --;
			m_items[0] = m_items[m_count];
			m_items[0].HeapIndex = 0;
			SortDown(m_items[0]);

			return _firstElement;
		}

		/// <summary>
		/// Sort Up the position of element in input.
		/// </summary>
		/// <param name="item">Item to sort</param>
		public void UpdateItem(T item) 
		{
			SortUp(item);
		}

		/// <summary>
		/// Heap operation, sorts the input element. Compare its value 
		/// with the parent and if it is greater, exchange it. Repeat 
		/// the operation until the input element has a lower value 
		/// than that of the parent node.
		/// </summary>
		/// <param name="item">Item to sort.</param>
		private void SortUp(T item)
		{
			// By starting from left or right child node, parent is always
			// the child index - 1 divided by 2. Floor approximation is needed
			// because if item is an "right node child" input, the forumla should 
			// be (item.HeapIndex - 2) / 2.
			int	_parentIndex = UnityEngine.Mathf.FloorToInt((item.HeapIndex - 1) / 2);

			T 	_parentNode  = m_items[_parentIndex];

			while(item.CompareTo(_parentNode) > 0)
			{
				Swap(item, _parentNode);

				_parentIndex = (item.HeapIndex - 1) / 2;
				_parentNode  = m_items[_parentIndex]; 
			}
		}

		/// <summary>
		/// Compare element with its children. In a Max-Heap (root is > than every other node)
		/// if one of element child is greater than the element itself, then swap it and sort every father.
		/// In a Min-Heap (root is < than every other node), if one of element child is less  
		/// than the element itself, then swap and check again.
		/// 
		/// </summary>
		/// <param name="_item">Item to check</param>
        void SortDown(T _item)
        {
			int _childIndexLeft = _item.HeapIndex * 2 + 1;
            int _childIndexRight = _item.HeapIndex * 2 + 2;
            int swapIndex = 0;

            while (true)
            {
				/// If the left child is >= of total element count, it means that
				/// heap is finished.
				if(_childIndexLeft >= m_count)
					return;

                swapIndex = _childIndexLeft;

                if (_childIndexRight < m_count)
                {
                	if (m_items[_childIndexLeft].CompareTo(m_items[_childIndexRight]) < 0)
                    {
                        swapIndex = _childIndexRight;
                    }
                }

                if (_item.CompareTo(m_items[swapIndex]) >= 0)
					return;

                Swap(_item, m_items[swapIndex]);

				_childIndexLeft = _item.HeapIndex * 2 + 1;
            	_childIndexRight = _item.HeapIndex * 2 + 2;
            	swapIndex = 0;
            }
        }

		/// <summary>
		/// Swap Two elements
		/// </summary>
		/// <param name="_itemA">Item To Swap</param>
		/// <param name="_itemB">Item swapped</param>
		private void Swap(T _itemA, T _itemB)
		{
			m_items[_itemA.HeapIndex] = _itemB;
			m_items[_itemB.HeapIndex] = _itemA;
			int _itemAIndex 		  = _itemA.HeapIndex;
			_itemA.HeapIndex          = _itemB.HeapIndex;
			_itemB.HeapIndex          = _itemAIndex;
		}
    }

	public interface IHeapElement<T> : IComparable<T>
	{
		int HeapIndex { get; set; }
	}
}
