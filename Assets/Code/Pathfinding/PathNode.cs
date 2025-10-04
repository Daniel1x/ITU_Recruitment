using System.Collections.Generic;
using UnityEngine;
using static MathExtensions;

public class PathNode : System.IDisposable
{
    public PathNode PreviousNode = null;

    public WorldSpaceTile Tile { get; private set; } = null;

    private Vector2Int gridPosition = default;
    private readonly List<PathNode> neighbours = new List<PathNode>();
    public AStarScore Score { get; set; } = default;
    public float EnterDirectionAngle { get; set; } = 0f;
    public Vector2Int GridPosition => gridPosition;
    public List<PathNode> Neighbours => neighbours;

    public PathNode(WorldSpaceTile _assignedTile)
    {
        gridPosition = _assignedTile.GridPosition;
        Tile = _assignedTile;

        _assignedTile.Node = this;
    }

    public void Dispose()
    {
        if (Tile != null)
        {
            Tile.Node = null;
            Tile = null;
        }

        ResetPathfindingData();

        gridPosition = default;
        neighbours.Clear();
    }

    public void ResetPathfindingData()
    {
        Score = new AStarScore(int.MaxValue, 0, 0);
        EnterDirectionAngle = -1;
        PreviousNode = null;
    }

    public void Initialize(TileGrid _grid)
    {
        ResetPathfindingData();

        neighbours.Clear();

        for (int i = 0; i < NEIGHBOUR_OFFSETS.Length; i++)
        {
            Vector2Int _neighbourPosition = gridPosition + NEIGHBOUR_OFFSETS[i];

            if (_neighbourPosition.IsInGridBounds(_grid.Size) == false)
            {
                continue;
            }

            PathNode _neighbourNode = _grid.Get(_neighbourPosition);

            if (_neighbourNode != null)
            {
                neighbours.Add(_neighbourNode);
            }
        }
    }
}
