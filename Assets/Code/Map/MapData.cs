using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class MapData
{
    public event UnityAction OnMapDataChanged = null;

    [SerializeField] private Vector2Int mapSize = new Vector2Int(10, 10);
    [SerializeField] private TileType[] map = null;

    public Vector2Int Size => mapSize;
    public int Width => mapSize.x;
    public int Height => mapSize.y;
    public int TileCount => mapSize.x * mapSize.y;

    public bool IsValid => Width > 0
        && Height > 0
        && map != null
        && map.Length == TileCount;

    public TileType this[Vector2Int _gridPosition]
    {
        get
        {
            if (!IsValid || !_gridPosition.IsInGridBounds(Size))
            {
                return default;
            }

            return map[GetIndexFromGridPosition(_gridPosition)];
        }
        set
        {
            if (!IsValid || !_gridPosition.IsInGridBounds(Size))
            {
                return;
            }

            int _index = GetIndexFromGridPosition(_gridPosition);

            if (map[_index] != value)
            {
                map[_index] = value;
                OnMapDataChanged?.Invoke();
            }
        }
    }

    public MapData(int _width, int _height) : this(new Vector2Int(_width, _height)) { }

    public MapData(Vector2Int _size)
    {
        SetNewSize(_size, false);
    }

    public int GetIndexFromGridPosition(Vector2Int _gridPosition)
    {
        return _gridPosition.x + (_gridPosition.y * mapSize.x);
    }

    public bool SetNewSize(Vector2Int _newSize, bool _callEvent = true)
    {
        if (_newSize.x <= 0
            || _newSize.y <= 0
            || _newSize == mapSize)
        {
            return false;
        }

        Vector2Int _previousSize = mapSize;
        TileType[] _previousData = map;

        mapSize = _newSize;
        map = new TileType[TileCount];

        // Copy over existing data to the new map, if possible
        if (_previousData != null)
        {
            int _minWidth = Mathf.Min(_previousSize.x, mapSize.x);
            int _minHeight = Mathf.Min(_previousSize.y, mapSize.y);

            for (int x = 0; x < _minWidth; x++)
            {
                for (int y = 0; y < _minHeight; y++)
                {
                    int _oldIndex = x + (y * _previousSize.x);
                    int _newIndex = x + (y * mapSize.x);

                    map[_newIndex] = _previousData[_oldIndex];
                }
            }
        }

        if (_callEvent)
        {
            OnMapDataChanged?.Invoke();
        }

        return true;
    }
}
