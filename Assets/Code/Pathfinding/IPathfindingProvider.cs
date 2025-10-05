/// <summary> Provides access to pathfinding functionality and the underlying grid used for navigation. </summary>
public interface IPathfindingProvider
{
    public Pathfinding Pathfinding { get; }
    public WorldSpaceTile[,] PathfindingGrid { get; }
}
