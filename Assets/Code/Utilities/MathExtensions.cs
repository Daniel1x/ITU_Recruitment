using System;
using UnityEngine;

public static class MathExtensions
{
    /// <summary> The size of each cell in the grid, used for converting between grid and world positions. </summary>
    public const float GRID_CELL_SIZE = 1f;

    /// <summary> Represents the offsets for the four cardinal directions (North, East, South, and West) in a 2D grid, relative to a given position. </summary>
    public static readonly Vector2Int[] NEIGHBOUR_OFFSETS = new Vector2Int[]
    {
        new Vector2Int(0, 1),   // North
        new Vector2Int(1, 0),   // East  
        new Vector2Int(0, -1),  // South 
        new Vector2Int(-1, 0),  // West
    };

    /// <summary> Checks if a grid position is within the bounds of the grid. </summary>
    public static bool IsInGridBounds(this Vector2Int _pos, Vector2Int _gridSize)
    {
        return _pos.x >= 0
            && _pos.y >= 0
            && _pos.x < _gridSize.x
            && _pos.y < _gridSize.y;
    }

    /// <summary> Converts a grid position represented by a <see cref="Vector2Int"/> to a world position in 3D space. </summary>
    public static Vector3 GetWorldPosition(this Vector2Int _gridPosition)
    {
        return new Vector3(_gridPosition.x + 0.5f, 0f, _gridPosition.y + 0.5f) * GRID_CELL_SIZE;
    }

    /// <summary> Converts a world position in 3D space to a grid position represented by a <see cref="Vector2Int"/>. </summary>
    public static Vector2Int GetGridPosition(this Vector3 _worldPosition)
    {
        _worldPosition /= GRID_CELL_SIZE;
        return new Vector2Int(Mathf.FloorToInt(_worldPosition.x), Mathf.FloorToInt(_worldPosition.z));
    }

    /// <summary> Calculates the Manhattan distance between two grid positions. </summary>
    public static int GetDistance(this Vector2Int _a, Vector2Int _b)
    {
        return Math.Abs(_a.x - _b.x)
             + Math.Abs(_a.y - _b.y);
    }

    /// <summary> Converts a grid position to a 1D array index based on the grid size. </summary>
    public static int GetIndexFromGridPosition(this Vector2Int _gridPosition, Vector2Int _gridSize)
    {
        return _gridPosition.x + (_gridPosition.y * _gridSize.x);
    }
}
