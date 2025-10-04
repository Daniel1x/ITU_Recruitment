using System.Collections.Generic;
using static AStarScore;

/// <summary>
/// A priority queue for PathNode objects, implemented as a binary min-heap.
/// The priority is determined by the node's AStarScore (FCost, HCost, DirectionChangeCount)
/// using the AStarScore.IsScoreBetterForQueue comparison logic.
/// This queue is optimized for use in A* pathfinding on a grid.
/// </summary>
public class NodePriorityQueue
{
    /// <summary> Internal list representing the binary heap. </summary>
    private readonly List<PathNode> heap = new List<PathNode>();

    /// <summary> Gets the number of elements in the queue. </summary>
    public int Count => heap.Count;

    /// <summary> Removes all elements from the queue. </summary>
    public void Clear() => heap.Clear();

    /// <summary>
    /// Adds a PathNode to the queue.
    /// The node's priority is determined by its current Score.
    /// </summary>
    /// <param name="_node">The node to enqueue.</param>
    public void Enqueue(PathNode _node)
    {
        heap.Add(_node);
        heapifyUp(heap.Count - 1);
    }

    /// <summary>
    /// Removes and returns the node with the highest priority (lowest FCost).
    /// Returns null if the queue is empty.
    /// </summary>
    /// <returns>The node with the lowest FCost, or null if the queue is empty.</returns>
    public PathNode Dequeue()
    {
        // If the heap is empty, return null
        if (heap.Count == 0)
        {
            return null;
        }

        // If the heap has only one element, remove and return it
        PathNode _root = heap[0];
        int _last = heap.Count - 1;

        // If there's only one element, just remove it and return
        if (_last == 0)
        {
            heap.RemoveAt(0);
            return _root;
        }

        // Move the last element to the root and heapify down
        heap[0] = heap[_last];
        heap.RemoveAt(_last);
        heapifyDown(0);

        // Return the removed root node
        return _root;
    }

    /// <summary>
    /// Forces the heap to update the position of the specified node after its score has changed.
    /// If the node exists in the heap, it is moved up or down as needed to restore the heap property.
    /// Ensures that index calculations do not go out of bounds.
    /// </summary>
    /// <param name="_node">The node to update in the heap.</param>
    public void ForceHeapUpdate(PathNode _node)
    {
        int _index = heap.IndexOf(_node);

        if (_index < 0 || _index >= heap.Count)
        {
            return; // Node not found or index out of bounds
        }

        int _parent = (_index - 1) / 2;

        // Check parent index is valid before accessing
        if (_index > 0 && _parent >= 0 && _parent < heap.Count && IsScoreBetterForQueue(heap[_index].Score, heap[_parent].Score))
        {
            heapifyUp(_index);
        }
        else
        {
            heapifyDown(_index);
        }
    }

    /// <summary>
    /// Restores the heap property by moving the element at the given index up the tree.
    /// This is called after inserting a new element at the end of the heap.
    /// The method compares the inserted node with its parent and swaps them if the node has a higher priority
    /// (i.e., a better AStarScore as determined by IsScoreBetterForQueue). This process repeats until the node
    /// is in the correct position or becomes the root.
    /// </summary>
    /// <param name="_index">The index of the element to move up.</param>
    private void heapifyUp(int _index)
    {
        PathNode _item = heap[_index];

        // Traverse up the tree, swapping with the parent if the current node has higher priority
        while (_index > 0)
        {
            int _parent = (_index - 1) / 2;

            // If the current node is better than its parent, move the parent down
            if (IsScoreBetterForQueue(_item.Score, heap[_parent].Score))
            {
                heap[_index] = heap[_parent];
                _index = _parent;
            }
            else
            {
                // The node is in the correct position
                break;
            }
        }

        // Place the item in its final position
        heap[_index] = _item;
    }

    /// <summary>
    /// Restores the heap property by moving the element at the given index down the tree.
    /// This is called after removing the root and moving the last element to the root position.
    /// The method compares the node with its children and swaps it with the child that has a higher priority
    /// (i.e., a better AStarScore as determined by IsScoreBetterForQueue). This process repeats until the node
    /// is in the correct position or becomes a leaf.
    /// </summary>
    /// <param name="_index">The index of the element to move down.</param>
    private void heapifyDown(int _index)
    {
        int _count = heap.Count;
        PathNode _item = heap[_index];
        int half = _count / 2;

        // Traverse down the tree, swapping with the better child if necessary
        while (_index < half)
        {
            int _left = 2 * _index + 1;
            int _right = _left + 1;
            int _best = _left;

            // Select the child with the higher priority (better score)
            if (_right < _count && IsScoreBetterForQueue(heap[_right].Score, heap[_left].Score))
            {
                _best = _right;
            }

            // If the best child is better than the current node, move the child up
            if (IsScoreBetterForQueue(heap[_best].Score, _item.Score))
            {
                heap[_index] = heap[_best];
                _index = _best;
            }
            else
            {
                // The node is in the correct position
                break;
            }
        }

        // Place the item in its final position
        heap[_index] = _item;
    }
}
