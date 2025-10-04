public interface IPathfindingProvider
{
    public Pathfinding Pathfinding { get; }
    public WorldSpaceTile[,] PathfindingGrid { get; }
}
