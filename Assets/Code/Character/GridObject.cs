using UnityEngine;

/// <summary> Base class for objects that occupy tiles in a grid-based system. </summary>
public class GridObject : MonoBehaviour
{
    public Vector2Int GridPosition { get; protected set; } = Vector2Int.zero;

    protected WorldSpaceTile occupiedTile = null;

    protected virtual void OnDestroy()
    {
        SetOccuipiedTile(null); // Clear tile reference on destroy
    }

    /// <summary> Sets the tile currently occupied by this object, updating references accordingly. </summary>
    public virtual void SetOccuipiedTile(WorldSpaceTile _newTile)
    {
        if (occupiedTile != null && occupiedTile != _newTile)
        {
            occupiedTile.Occupant = null; // Clear previous tile's occupant reference
            GridPosition = -Vector2Int.one; // Reset grid position
        }

        occupiedTile = _newTile;

        if (occupiedTile != null)
        {
            occupiedTile.Occupant = this; // Set new tile's occupant reference
            GridPosition = occupiedTile.GridPosition; // Update grid position
        }
    }
}
