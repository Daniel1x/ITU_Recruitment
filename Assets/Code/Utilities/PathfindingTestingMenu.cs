using System.Collections.Generic;
using UnityEngine;

public class PathfindingTestingMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FreeCamera freeCamera = null;
    [SerializeField] private MapRenderer mapRenderer = null;
    [SerializeField] private PathVisualization pathVisualization = null;
    [SerializeField] private PathVisualization attackPathVisualization = null;
    [SerializeField] private OutOfRangePopup outOfRangePopup = null;

    private List<PathNode> currentPath = new();
    private List<PathNode> attackValidationPath = new();
    private PlayerCharacter playerCharacter = null;
    private WorldSpaceTile targetTile = null;
    private WorldSpaceTile pendingAttackMoveDestination = null;

    public bool IsMovementInProgress => playerCharacter != null
        && playerCharacter.MovementController != null
        && playerCharacter.MovementController.IsMoving;

    private void OnEnable()
    {
        if (freeCamera != null)
        {
            freeCamera.OnMouseClickAtGroundPosition += handlePathfindingClickAtPosition;
        }
    }

    private void OnDisable()
    {
        if (freeCamera != null)
        {
            freeCamera.OnMouseClickAtGroundPosition -= handlePathfindingClickAtPosition;
        }

        if (pathVisualization != null)
        {
            pathVisualization.SetPath(null);
        }

        if (attackPathVisualization != null)
        {
            attackPathVisualization.SetPath(null);
        }
    }

    private void handlePathfindingClickAtPosition(bool _leftClick, Vector3 _position)
    {
        if (IsMovementInProgress)
        {
            return; // Ignore clicks while movement is in progress
        }

        if (mapRenderer == null
            || mapRenderer.Map == null
            || mapRenderer.Map.IsValid == false
            || mapRenderer.Pathfinding == null
            || mapRenderer.PathfindingGrid == null)
        {
            return;
        }

        Vector2Int _targetPosition = _position.GetGridPosition();

        if (_targetPosition.IsInGridBounds(mapRenderer.Map.Size) == false)
        {
            return;
        }

        playerCharacter = PlayerCharacter.Instance;

        if (playerCharacter == null)
        {
            return;
        }

        targetTile = mapRenderer.GetTileAtPosition(_targetPosition);

        if (targetTile == null || targetTile.Type is TileType.Obstacle)
        {
            return;
        }

        Vector2Int _playerPosition = playerCharacter.GridPosition;

        if (targetTile.Occupant != null && targetTile.Occupant != playerCharacter)
        {
            if (playerCharacter.AttackAvailableTiles.Contains(targetTile))
            {
                attackEnemyOnTargetTile();
                return;
            }
            else
            {
                showOutOfRange(_position);
                return;
            }
        }

        if (playerCharacter.MovementAvailableTiles.Contains(targetTile))
        {
            movePlayerToTargetTile();
        }
        else
        {
            showOutOfRange(_position);
        }
    }

    private void showOutOfRange(Vector3 _position)
    {
        if (pathVisualization != null)
        {
            pathVisualization.SetPath(null);
        }

        if (attackPathVisualization != null)
        {
            attackPathVisualization.SetPath(null);
        }

        if (outOfRangePopup != null)
        {
            outOfRangePopup.Show(_position, freeCamera);
        }
    }

    private void movePlayerToTargetTile()
    {
        // hide any previous attack path when doing a pure move
        if (attackPathVisualization != null)
        {
            attackPathVisualization.SetPath(null);
        }

        currentPath = mapRenderer.Pathfinding.GetPath(playerCharacter.GridPosition, targetTile.GridPosition, currentPath, false, playerCharacter.MovementRange);

        if (currentPath == null || currentPath.Count == 0)
        {
            return;
        }

        pathVisualization.SetPath(currentPath);
        playerCharacter.MovementController.MoveAlongPath(currentPath, onMovementEnd);
    }

    private void onMovementEnd()
    {
        if (playerCharacter != null && targetTile != null)
        {
            playerCharacter.SetOccuipiedTile(targetTile);
        }
    }

    private void attackEnemyOnTargetTile()
    {
        if (mapRenderer == null
            || mapRenderer.Pathfinding == null
            || playerCharacter == null
            || targetTile == null)
        {
            return;
        }

        // Choose a movement tile such that there exists a valid attack path (<= AttackRange) from that tile to the enemy
        WorldSpaceTile _bestTile = null;
        int _bestAttackLen = int.MaxValue; // steps count from chosen tile to enemy
        int _bestDist = int.MaxValue;      // tie-breaker: Manhattan distance to enemy
        Vector2Int _enemyPos = targetTile.GridPosition;
        int _attackRange = playerCharacter.AttackRange;

        for (int i = 0; i < playerCharacter.MovementAvailableTiles.Count; i++)
        {
            WorldSpaceTile _tile = playerCharacter.MovementAvailableTiles[i];

            if (_tile == null)
            {
                continue;
            }

            // Do not step onto the enemy tile
            if (_tile == targetTile)
            {
                continue;
            }

            // Cannot move onto tiles occupied by someone else (allow staying on own tile)
            if (_tile.Occupant != null && _tile.Occupant != playerCharacter)
            {
                continue;
            }

            int _distToEnemy = _tile.GridPosition.GetDistance(_enemyPos);

            if (_distToEnemy > _attackRange)
            {
                continue; // too far to attack even in straight reach terms
            }

            // Validate there is a real path (covers allowed, obstacles block) from this tile to enemy within attack range
            List<PathNode> _attackCheckPath = mapRenderer.Pathfinding.GetPath(_tile.GridPosition, _enemyPos, attackValidationPath, true, _attackRange);

            if (_attackCheckPath == null || _attackCheckPath.Count == 0)
            {
                continue;
            }

            int _steps = _attackCheckPath.Count - 1; // path includes start node

            if (_steps > _attackRange)
            {
                continue;
            }

            // Prefer shortest attack path or tie-break by distance to enemy
            if (_steps < _bestAttackLen || (_steps == _bestAttackLen && _distToEnemy < _bestDist))
            {
                _bestAttackLen = _steps;
                _bestDist = _distToEnemy;
                _bestTile = _tile;
            }
        }

        if (_bestTile != null)
        {
            // Recompute and visualize attack path from best tile to enemy
            attackValidationPath = mapRenderer.Pathfinding.GetPath(_bestTile.GridPosition, _enemyPos, attackValidationPath, true, _attackRange);

            if (attackPathVisualization != null)
            {
                attackPathVisualization.SetPath(attackValidationPath);
            }

            // Move to the chosen tile
            currentPath = mapRenderer.Pathfinding.GetPath(playerCharacter.GridPosition, _bestTile.GridPosition, currentPath, false, playerCharacter.MovementRange);

            if (currentPath == null || currentPath.Count == 0)
            {
                return;
            }

            pendingAttackMoveDestination = currentPath[currentPath.Count - 1]?.Tile ?? _bestTile;
            pathVisualization.SetPath(currentPath);
            playerCharacter.MovementController.MoveAlongPath(currentPath, onAttackMovementEnd);
            return;
        }

        // Fallback: compute path to enemy tile and then drop the last node so we don't enter it
        currentPath = mapRenderer.Pathfinding.GetPath(playerCharacter.GridPosition, targetTile.GridPosition, currentPath, false, playerCharacter.MovementRange);

        if (currentPath == null || currentPath.Count == 0)
        {
            return;
        }

        // Remove the last node (enemy tile) to stop before the target
        currentPath.RemoveAt(currentPath.Count - 1);

        if (currentPath.Count == 0)
        {
            return;
        }

        pendingAttackMoveDestination = currentPath[currentPath.Count - 1].Tile;

        // Visualize attack path from the final movement tile to the enemy
        if (pendingAttackMoveDestination != null && attackPathVisualization != null)
        {
            attackValidationPath = mapRenderer.Pathfinding.GetPath(pendingAttackMoveDestination.GridPosition, _enemyPos, attackValidationPath, true, _attackRange);
            attackPathVisualization.SetPath(attackValidationPath);
        }

        pathVisualization.SetPath(currentPath);
        playerCharacter.MovementController.MoveAlongPath(currentPath, onAttackMovementEnd);
    }

    private void onAttackMovementEnd()
    {
        if (playerCharacter != null && pendingAttackMoveDestination != null)
        {
            playerCharacter.SetOccuipiedTile(pendingAttackMoveDestination);
        }

        pendingAttackMoveDestination = null;

        // Hide attack path after resolving the attack
        if (attackPathVisualization != null)
        {
            attackPathVisualization.SetPath(null);
        }

        //Destroy the enemy on the target tile
        if (targetTile != null && targetTile.Occupant != null)
        {
            Destroy(targetTile.Occupant.gameObject);
        }
    }
}
