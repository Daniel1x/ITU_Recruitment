using System.Collections.Generic;
using UnityEngine;

public class WorldSpaceTile : MonoBehaviour
{
    public static bool AnyVisualizationChanged { get; private set; } = false;

    private static readonly List<WorldSpaceTile> activeTiles = new List<WorldSpaceTile>();

    [field: SerializeField] public Vector2Int GridPosition { get; private set; }
    [field: SerializeField] public TileType Type { get; private set; }
    [field: SerializeField] public PathNode Node { get; set; } = null;
    [field: SerializeField] public GridObject Occupant { get; set; } = null;

    public bool IsInMovementRange { get; private set; } = false;
    public bool IsInAttackRange { get; private set; } = false;

    public Vector3 WorldPosition => transform.position;

    private void OnEnable()
    {
        if (activeTiles.Contains(this) == false)
        {
            activeTiles.Add(this);
        }
    }

    private void OnDisable()
    {
        activeTiles.Remove(this);
    }

    private void OnDestroy()
    {
        Node = null;
    }

    public void UpdateVisualization(bool _isInMovementRange = false, bool _isInAttackRange = false, bool _force = false)
    {
        if (_force == false
            && IsInMovementRange == _isInMovementRange
            && IsInAttackRange == _isInAttackRange)
        {
            return; // No change
        }

        IsInMovementRange = _isInMovementRange;
        IsInAttackRange = _isInAttackRange;
        AnyVisualizationChanged = true;
    }

    public void SetIsInMovementRange(bool _isInMovementRange) => UpdateVisualization(_isInMovementRange, IsInAttackRange);
    public void SetIsInAttackRange(bool _isInAttackRange) => UpdateVisualization(IsInMovementRange, _isInAttackRange);

    public static WorldSpaceTile SetAsWorldSpaceTile(GameObject _object, Vector2Int _gridPosition, TileType _tileType)
    {
        if (_object == null)
        {
            return null; // Invalid object
        }

        WorldSpaceTile _tile = _object.GetComponent<WorldSpaceTile>();

        if (_tile == null) //If component is not available, create a new one.
        {
            _tile = _object.AddComponent<WorldSpaceTile>();
        }

        // Assign setup data
        _tile.GridPosition = _gridPosition;
        _tile.Type = _tileType;
        _tile.UpdateVisualization(_force: true);

        return _tile;
    }

    public static void PerformActionOnActiveTiles(System.Action<WorldSpaceTile> _action, bool _resetFlag)
    {
        if (_action == null)
        {
            return;
        }

        for (int i = 0; i < activeTiles.Count; i++)
        {
            _action(activeTiles[i]);
        }

        if (_resetFlag)
        {
            AnyVisualizationChanged = false;
        }
    }
}
