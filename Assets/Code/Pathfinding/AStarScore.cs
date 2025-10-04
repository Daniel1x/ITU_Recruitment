public struct AStarScore
{
    ///<summary>Grid distance to current tile</summary>
    public float GCost;

    ///<summary>Grid distance to target tile</summary>
    public float HCost;

    ///<summary>Sum of GScore and HScore</summary>
    public float FCost { get; private set; }

    ///<summary>Number of directions changed in the path</summary>
    public int DirectionChangeCount;

    public AStarScore(float _g, float _h, int _dirChange)
    {
        GCost = _g;
        HCost = _h;
        FCost = _g + _h;
        DirectionChangeCount = _dirChange;
    }

    /// <summary> Checks if the current score is better than another score. </summary>
    /// <param name="_isInOpen">Indicates if the node is already in the open list.</param>
    /// <param name="_newScore">The score to compare against.</param>
    /// <param name="_node">The node to compare with.</param>
    /// <returns>True if the calculated score is better, false otherwise.</returns>
    /// <remarks> This method determines if the calculated score is better than the existing score of the node.
    /// It considers whether the node is already in the open list and compares the FCost, HCost, and direction change count.
    /// A node can be replaced if it is not in the open list, has no previous score, or if the calculated score has a lower FCost.
    /// If the FCosts are equal, it prefers the score with a lower HCost (closer to target) or fewer direction changes (straighter path).</remarks>
    public static bool IsCalculatedScoreBetter(bool _isInOpen, AStarScore _newScore, PathNode _node)
    {
        if (_isInOpen == false)
        {
            return true; //New node or not in open list, can replace
        }

        bool _hasOldScore = _node.PreviousNode != null;

        if (_hasOldScore == false)
        {
            return true; //No old score, can replace
        }

        AStarScore _oldScore = _node.Score;

        if (_newScore.FCost > _oldScore.FCost)
        {
            return false; //Is not better
        }

        if (_newScore.FCost == _oldScore.FCost) //The same FCost, check other
        {
            return _newScore.HCost < _oldScore.HCost //Closer to target
                || _newScore.DirectionChangeCount < _oldScore.DirectionChangeCount; //Less direction changes, straight path is better
        }

        return true; //Lower FCost, it is better, so can replace
    }

    /// <summary>
    /// Determines whether the first score is considered better than the second score for priority queue purposes.
    /// </summary>
    /// <remarks>The comparison prioritizes lower F-cost values, followed by lower H-cost values, and finally
    /// lower direction change counts. If all values are equal, the method returns <see langword="false"/>.</remarks>
    /// <param name="_scoreA">The first <see cref="AStarScore"/> to compare.</param>
    /// <param name="_scoreB">The second <see cref="AStarScore"/> to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="_scoreA"/> is better than <paramref name="_scoreB"/> based on the
    /// comparison of F-cost, H-cost, and direction change count; otherwise, <see langword="false"/>.</returns>
    public static bool IsScoreBetterForQueue(AStarScore _scoreA, AStarScore _scoreB)
    {
        if (_scoreA.FCost != _scoreB.FCost)
        {
            return _scoreA.FCost < _scoreB.FCost; // Lower FCost is better
        }

        if (_scoreA.HCost != _scoreB.HCost)
        {
            return _scoreA.HCost < _scoreB.HCost; // Lower HCost is better
        }

        if (_scoreA.DirectionChangeCount != _scoreB.DirectionChangeCount)
        {
            return _scoreA.DirectionChangeCount < _scoreB.DirectionChangeCount; // Lower direction change count is better
        }

        // If all scores are equal, return false (no preference)
        return false;
    }
}
