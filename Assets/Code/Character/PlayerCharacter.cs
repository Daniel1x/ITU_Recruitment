using System.Collections.Generic;
using UnityEngine;
using static MathExtensions;

[RequireComponent(typeof(CharacterMovementController))]
public class PlayerCharacter : GridObject
{
    /// <summary> Represents a node used in a breadth-first search (BFS) algorithm, containing a position and its distance from the starting point. </summary>
    private struct BfsNode
    {
        public Vector2Int Position;
        public int Distance;

        public BfsNode(Vector2Int _pos, int _dist)
        {
            Position = _pos;
            Distance = _dist;
        }
    }

    public static PlayerCharacter Instance { get; private set; } = null;

    /// <summary> Represents the collection of tiles available for movement in the world space. </summary>
    public readonly List<WorldSpaceTile> MovementAvailableTiles = new();

    /// <summary> Represents the collection of tiles available for attack in the world space. </summary>
    public readonly List<WorldSpaceTile> AttackAvailableTiles = new();

    public int MovementRange { get; private set; } = 5;
    public int AttackRange { get; private set; } = 3;
    public MapData CurrentMap { get; set; } = null;
    public IPathfindingProvider PathfindingProvider { get; set; } = null;
    public CharacterMovementController MovementController { get; private set; } = null;

    // Reusable buffers for BFS
    private readonly Queue<BfsNode> bfsQueue = new(128);
    private readonly HashSet<Vector2Int> visitedPositionsBuffer = new();
    private readonly HashSet<Vector2Int> movementPositionsBuffer = new();
    private readonly HashSet<Vector2Int> attackPositionsBuffer = new();

    private void Awake()
    {
        Instance = this;
        MovementController = GetComponent<CharacterMovementController>();
    }

    public override void SetOccuipiedTile(WorldSpaceTile _newTile)
    {
        base.SetOccuipiedTile(_newTile);

        if (occupiedTile != null)
        {
            RecalculateAvailableTiles();
        }
    }

    public void SetRanges(int _attackRange, int _movementRange, bool _recalculateTiles = true)
    {
        if (AttackRange == _attackRange && MovementRange == _movementRange)
        {
            return;
        }

        AttackRange = _attackRange;
        MovementRange = _movementRange;

        if (_recalculateTiles)
        {
            RecalculateAvailableTiles();
        }
    }

    public void RecalculateAvailableTiles()
    {
        if (CurrentMap == null
            || CurrentMap.IsValid == false
            || PathfindingProvider == null
            || PathfindingProvider.PathfindingGrid is not WorldSpaceTile[,] _grid
            || _grid.GetLength(0) != CurrentMap.Width
            || _grid.GetLength(1) != CurrentMap.Height
            || occupiedTile == null)
        {
            return; // Cannot calculate without valid map, grid, and occupied tile
        }

        // Clear previous visualization
        clearVisualization(MovementAvailableTiles);
        clearVisualization(AttackAvailableTiles);

        // Clear buffers
        movementPositionsBuffer.Clear();
        attackPositionsBuffer.Clear();

        // 1) Compute movement positions (covers blocked, obstacles blocked), starting from occupied position
        computePositions(
            _output: movementPositionsBuffer,
            _sources: singleSource(occupiedTile.GridPosition),
            _maxRange: MovementRange,
            _blockCovers: true,
            _includeSources: true,
            _grid: _grid
        );

        // Fill MovementAvailableTiles
        foreach (Vector2Int _pos in movementPositionsBuffer)
        {
            WorldSpaceTile _tile = _grid[_pos.x, _pos.y];

            if (_tile != null)
            {
                MovementAvailableTiles.Add(_tile);
            }
        }

        // 2) Compute attack positions from all reachable movement positions (covers allowed, obstacles blocked)
        computePositions(
            _output: attackPositionsBuffer,
            _sources: movementPositionsBuffer, // multi-source BFS
            _maxRange: AttackRange,
            _blockCovers: false,
            _includeSources: true, // include movement tiles as part of the attack set
            _grid: _grid
        );

        // Fill AttackAvailableTiles directly from positionsBufferB (unique positions)
        foreach (Vector2Int _pos in attackPositionsBuffer)
        {
            WorldSpaceTile _tile = _grid[_pos.x, _pos.y];

            if (_tile != null)
            {
                AttackAvailableTiles.Add(_tile);
            }
        }

        // Update visualization flags
        for (int i = 0; i < MovementAvailableTiles.Count; i++)
        {
            MovementAvailableTiles[i].SetIsInMovementRange(true);
        }

        for (int i = 0; i < AttackAvailableTiles.Count; i++)
        {
            AttackAvailableTiles[i].SetIsInAttackRange(true);
        }
    }

    private void computePositions(HashSet<Vector2Int> _output, IEnumerable<Vector2Int> _sources, int _maxRange, bool _blockCovers, bool _includeSources, WorldSpaceTile[,] _grid)
    {
        _output.Clear();
        visitedPositionsBuffer.Clear();
        bfsQueue.Clear();

        // Initialize BFS queue with source positions
        foreach (Vector2Int _pos in _sources)
        {
            if (!_pos.IsInGridBounds(CurrentMap.Size))
            {
                continue; // Skip out-of-bounds sources
            }

            if (!visitedPositionsBuffer.Add(_pos))
            {
                continue; // Skip already visited sources
            }

            if (_includeSources)
            {
                _output.Add(_pos); // Include source positions in output if specified
            }

            bfsQueue.Enqueue(new BfsNode(_pos, 0));
        }

        // Perform BFS to find all reachable positions within max range
        while (bfsQueue.Count > 0)
        {
            BfsNode _node = bfsQueue.Dequeue();

            if (_node.Distance >= _maxRange)
            {
                continue; // Reached max range, do not expand further
            }

            for (int i = 0; i < NEIGHBOUR_OFFSETS.Length; i++)
            {
                Vector2Int _neighbourPos = _node.Position + NEIGHBOUR_OFFSETS[i];

                if (!_neighbourPos.IsInGridBounds(CurrentMap.Size)
                    || visitedPositionsBuffer.Contains(_neighbourPos))
                {
                    continue; // Out of bounds or already visited
                }

                WorldSpaceTile _tile = _grid[_neighbourPos.x, _neighbourPos.y];

                if (_tile == null)
                {
                    continue; // Invalid tile
                }

                if (_tile.Type is TileType.Obstacle
                    || _blockCovers && _tile.Type is TileType.Cover)
                {
                    continue; // Cannot traverse obstacles or covers if blocking is enabled
                }

                visitedPositionsBuffer.Add(_neighbourPos);
                _output.Add(_neighbourPos);
                bfsQueue.Enqueue(new BfsNode(_neighbourPos, _node.Distance + 1));
            }
        }
    }

    /// <summary> Funny little enumerable that yields a single source position. Used to avoid allocations in single-source BFS. </summary>
    private IEnumerable<Vector2Int> singleSource(Vector2Int _s)
    {
        yield return _s;
    }

    private void clearVisualization(List<WorldSpaceTile> _tiles)
    {
        if (_tiles == null)
        {
            return;
        }

        for (int i = 0; i < _tiles.Count; i++)
        {
            if (_tiles[i] != null)
            {
                _tiles[i].UpdateVisualization(); // Reset visualization
            }
        }

        _tiles.Clear();
    }
}
