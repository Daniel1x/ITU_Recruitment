using UnityEngine;

public class TileGrid : System.IDisposable
{
    public int Width { get; private set; } = default;
    public int Height { get; private set; } = default;
    public Vector2Int Size { get; private set; } = default;

    private readonly PathNode[,] grid;

    public TileGrid(WorldSpaceTile[,] _tiles)
    {
        bool _areTilesValid = _tiles != null;
        int _width = _areTilesValid ? _tiles.GetLength(0) : 0;
        int _height = _areTilesValid ? _tiles.GetLength(1) : 0;

        Width = _width;
        Height = _height;
        Size = new Vector2Int(_width, _height);
        grid = new PathNode[_width, _height];

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                grid[x, y] = new PathNode(_tiles[x, y]);
            }
        }

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                grid[x, y].Initialize(this);
            }
        }
    }

    public void ResetPathfindingData()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                grid[x, y].ResetPathfindingData();
            }
        }
    }

    public PathNode Get(Vector2Int _position)
    {
        if (_position.IsInGridBounds(Size) == false)
        {
            return null;
        }

        return grid[_position.x, _position.y];
    }

    public void Dispose()
    {
        if (grid == null)
        {
            return;
        }

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                grid[x, y].Dispose();
                grid[x, y] = null;
            }
        }

        System.Array.Clear(grid, 0, grid.Length);
    }
}
