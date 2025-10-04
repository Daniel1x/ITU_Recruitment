using System;
using UnityEngine;

public static class MathExtensions
{
    public const float GRID_CELL_SIZE = 1f; // Distance from center to center of adjacent cells

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

    public static Vector3 GetWorldPosition(this Vector2Int _gridPosition)
    {
        return new Vector3(_gridPosition.x + 0.5f, 0f, _gridPosition.y + 0.5f) * GRID_CELL_SIZE;
    }

    public static Vector2Int GetGridPosition(this Vector3 _worldPosition)
    {
        _worldPosition /= GRID_CELL_SIZE;
        return new Vector2Int(Mathf.FloorToInt(_worldPosition.x), Mathf.FloorToInt(_worldPosition.z));
    }

    public static int GetDistance(this Vector2Int _a, Vector2Int _b)
    {
        return Math.Abs(_a.x - _b.x)
             + Math.Abs(_a.y - _b.y);
    }
}
