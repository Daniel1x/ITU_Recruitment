using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : System.IDisposable
{
    private TileGrid grid = null;

    private NodePriorityQueue openQueue = new NodePriorityQueue();
    private HashSet<PathNode> openSet = new HashSet<PathNode>();
    private bool[,] closed = null;

    public Vector2Int GridSize => grid.Size;

    public Pathfinding(WorldSpaceTile[,] _tiles)
    {
        grid = new TileGrid(_tiles);
    }

    /// <summary> Releases all resources used by the current instance of the class. </summary>
    public void Dispose()
    {
        if (grid != null)
        {
            grid.Dispose();
            grid = null;
        }

        openQueue.Clear();
        openSet.Clear();
        closed = null; // Release memory for closed array
    }

    public List<PathNode> GetPath(Vector2Int _startGridPosition, Vector2Int _targetGridPosition, List<PathNode> _path, bool _allowMovementOverCovers, int _movementRange = -1)
    {
        // Get the start and target nodes from the grid
        PathNode _startNode = grid.Get(_startGridPosition);
        PathNode _targetNode = grid.Get(_targetGridPosition);

        // If either node is invalid or the target is blocked, return null (no path)
        if (_startNode == null || _targetNode == null)
        {
            return null; // Invalid nodes
        }

        // Clear all data structures used for pathfinding
        openQueue.Clear();
        openSet.Clear();
        grid.ResetPathfindingData();

        // Ensure the closed array is allocated and matches the grid size; clear it if reused
        if (closed == null || closed.GetLength(0) != grid.Width || closed.GetLength(1) != grid.Height)
        {
            closed = new bool[grid.Width, grid.Height];
        }
        else
        {
            System.Array.Clear(closed, 0, closed.Length);
        }

        // Add the start node to the open queue and set
        openQueue.Enqueue(_startNode);
        openSet.Add(_startNode);

        // Initialize the start node's score (G = 0, H = distance to target, 0 direction changes)
        _startNode.Score = new AStarScore(0, MathExtensions.GetDistance(_startNode.GridPosition, _targetNode.GridPosition), 0);

        // Main A* loop: process nodes until the open queue is empty
        while (openQueue.Count > 0)
        {
            // Get the node with the lowest FCost from the open queue
            PathNode _currentNode = openQueue.Dequeue();
            openSet.Remove(_currentNode);

            // If we've reached the target, reconstruct and return the path
            if (_currentNode == _targetNode)
            {
                return retracePath(_startNode, _targetNode, _path);
            }

            // Mark the current node as closed (visited)
            closed[_currentNode.GridPosition.x, _currentNode.GridPosition.y] = true;
            AStarScore _currentNodeScore = _currentNode.Score;

            // Examine all valid neighbors of the current node
            foreach (PathNode _neighbour in _currentNode.Neighbours)
            {
                TileType _type = _neighbour.Tile.Type;

                if (_type is TileType.Obstacle)
                {
                    continue; // Skip blocked tiles
                }

                if (_type is TileType.Cover && _allowMovementOverCovers == false)
                {
                    continue; // Skip cover tiles if movement over covers is not allowed
                }

                // Skip neighbors that are blocked or already closed
                if (closed[_neighbour.GridPosition.x, _neighbour.GridPosition.y])
                {
                    continue;
                }

                // Check if the neighbor is already in the open set
                bool _isInOpen = openSet.Contains(_neighbour);

                // Calculate the new score for this neighbor
                AStarScore _newScore = new AStarScore(
                    _g: _currentNodeScore.GCost + 1, // All neighbors are 1 step away
                    _h: MathExtensions.GetDistance(_neighbour.GridPosition, _targetNode.GridPosition),
                    _dirChange: _currentNodeScore.DirectionChangeCount + calculateDirectionChange(_currentNode, _neighbour, out int _directionAngle));

                if (_movementRange >= 0 && _newScore.GCost > _movementRange)
                {
                    continue; // Exceeds movement range
                }

                // If the new score is not better, skip this neighbor
                if (!AStarScore.IsCalculatedScoreBetter(_isInOpen, _newScore, _neighbour))
                {
                    continue;
                }

                // Update the neighbor's pathfinding data
                _neighbour.Score = _newScore;
                _neighbour.PreviousNode = _currentNode;
                _neighbour.EnterDirectionAngle = _directionAngle;

                // If the neighbor is not in the open set, add it for future processing
                if (!_isInOpen)
                {
                    openQueue.Enqueue(_neighbour);
                    openSet.Add(_neighbour);
                }
                else
                {
                    // If the neighbor is already in the open set, update its position in the priority queue
                    openQueue.ForceHeapUpdate(_neighbour);
                }
            }
        }

        // If the open queue is empty and the target was not reached, return null (no path found)
        return null;
    }

    /// <summary> Reconstructs the path from the target node back to the start node by following the PreviousNode links.
    /// The resulting path is stored in the provided list (or a new one if null), and is ordered from start to target. </summary>
    private List<PathNode> retracePath(PathNode _startNode, PathNode _targetNode, List<PathNode> _path, bool _includeStartNode = true)
    {
        // If the provided path list is null, create a new one; otherwise, clear the existing list
        if (_path == null)
        {
            _path = new List<PathNode>();
        }
        else
        {
            _path.Clear();
        }

        // Start from the target node and follow PreviousNode links backwards
        PathNode _currentNode = _targetNode;

        // Add each node to the path until reaching the start node (which has PreviousNode == null)
        while (_currentNode.PreviousNode != null)
        {
            _path.Add(_currentNode);
            _currentNode = _currentNode.PreviousNode;
        }

        // Optionally include the start node in the path
        if (_includeStartNode)
        {
            _path.Add(_startNode);
        }

        // The path was built from target to start, so reverse it to get start-to-target order
        _path.Reverse();

        return _path;
    }

    /// <summary> Determines whether a direction change occurs between two hexagonal path nodes and calculates the resulting direction angle. </summary>
    private int calculateDirectionChange(PathNode _from, PathNode _to, out int _directionAngle)
    {
        if (_from.EnterDirectionAngle < 0f)
        {
            _directionAngle = 999; // Invalid angle, will not count as a direction change
            return 0; // No direction change on the first node
        }

        Vector3 _dir = _to.Tile.transform.position - _from.Tile.transform.position;
        float _angle = Mathf.Atan2(_dir.x, _dir.z) * Mathf.Rad2Deg;

        _directionAngle = Mathf.RoundToInt(_angle);

        //Normalize angle to be between 0 and 360 degrees
        if (_directionAngle < 0)
        {
            _directionAngle += 360;
        }

        //Round to nearest 5 degrees to reduce direction changes
        _directionAngle = Mathf.RoundToInt(_directionAngle / 5f) * 5;

        return _from.EnterDirectionAngle == _directionAngle ? 0 : 1; // Count as a direction change if angles differ
    }
}
