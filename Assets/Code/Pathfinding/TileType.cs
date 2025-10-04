[System.Serializable]
public enum TileType
{
    Traversable = 0,
    Obstacle = 1,
    Cover = 2,
}

public static class TileTypeExtensions
{
    public readonly static int TileTypeCount = System.Enum.GetValues(typeof(TileType)).Length;

    public static TileType Next(this TileType _tileType)
    {
        return (TileType)(((int)_tileType + 1) % TileTypeCount);
    }

    public static TileType Previous(this TileType _tileType)
    {
        int _previousValue = (int)_tileType - 1;

        if (_previousValue < 0)
        {
            _previousValue = TileTypeCount - 1;
        }

        return (TileType)_previousValue;
    }
}
