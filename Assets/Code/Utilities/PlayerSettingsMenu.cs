using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSettingsMenu : RectTransformClickValidator
{
    [Header("References")]
    [SerializeField] private MapRenderer mapRenderer = null;
    [SerializeField] private FreeCamera freeCamera = null;

    [Header("Character Prefabs")]
    [SerializeField] private GridObject playerCharacterPrefab = null;
    [SerializeField] private GridObject enemyCharacterPrefab = null;

    [Header("Player Settings Menu")]
    [SerializeField] private Slider movementSlider = null;
    [SerializeField] private TMP_Text movementValue = null;
    [SerializeField] private Slider attackSlider = null;
    [SerializeField] private TMP_Text attackValue = null;
    [Space]
    [SerializeField, Min(1)] private int minRange = 1;
    [SerializeField, Min(1)] private int maxRange = 100;
    [Space]
    [SerializeField, Min(1)] private int movementRange = 5;
    [SerializeField, Min(1)] private int attackRange = 3;

    public int MovementRange => movementRange;
    public int AttackRange => attackRange;

    private Transform charactersParent = null;
    private GridObject playerInstance = null;
    private readonly List<GridObject> spawnedEnemyInstances = new List<GridObject>();

    public GridObject PlayerInstance => playerInstance;
    public List<GridObject> SpawnedEnemyInstances => spawnedEnemyInstances;

    private void Awake()
    {
        if (mapRenderer != null)
        {
            mapRenderer.OnPathfindingUpdated += reassignTiles;
        }
    }

    private void OnDestroy()
    {
        if (mapRenderer != null)
        {
            mapRenderer.OnPathfindingUpdated -= reassignTiles;
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (movementSlider != null)
        {
            movementSlider.onValueChanged.AddListener(onMovementChanged);
            movementSlider.value = Mathf.InverseLerp(minRange, maxRange, movementRange);
        }

        if (attackSlider != null)
        {
            attackSlider.onValueChanged.AddListener(onAttackChanged);
            attackSlider.value = Mathf.InverseLerp(minRange, maxRange, attackRange);
        }

        if (freeCamera != null)
        {
            freeCamera.OnMouseClickAtGroundPosition += spawnCharacterAtTile;
        }

    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (movementSlider != null)
        {
            movementSlider.onValueChanged.RemoveListener(onMovementChanged);
        }

        if (attackSlider != null)
        {
            attackSlider.onValueChanged.RemoveListener(onAttackChanged);
        }

        if (freeCamera != null)
        {
            freeCamera.OnMouseClickAtGroundPosition -= spawnCharacterAtTile;
        }
    }

    private void onMovementChanged(float _value)
    {
        movementRange = getRange(movementSlider != null ? movementSlider.value : 0f);

        if (movementValue != null)
        {
            movementValue.text = movementRange.ToString();
        }

        if (playerInstance is PlayerCharacter _player)
        {
            _player.SetRanges(attackRange, movementRange);
        }
    }

    private void onAttackChanged(float _value)
    {
        attackRange = getRange(attackSlider != null ? attackSlider.value : 0f);

        if (attackValue != null)
        {
            attackValue.text = attackRange.ToString();
        }

        if (playerInstance is PlayerCharacter _player)
        {
            _player.SetRanges(attackRange, movementRange);
        }
    }

    private int getRange(float _progress)
    {
        return Mathf.RoundToInt(Mathf.Lerp(minRange, maxRange, _progress));
    }

    private void spawnCharacterAtTile(bool _isPlayerCharacter, Vector3 _position)
    {
        if (mapRenderer == null || mapRenderer.Map == null)
        {
            return; // No valid map available
        }

        Vector2Int _gridPosition = _position.GetGridPosition();

        if (_gridPosition.IsInGridBounds(mapRenderer.Map.Size) == false)
        {
            return; // Position is out of map bounds
        }

        WorldSpaceTile _worldSpaceTile = mapRenderer.GetTileAtPosition(_gridPosition);

        if (_worldSpaceTile == null
            || _worldSpaceTile.Occupant != null
            || _worldSpaceTile.Type is not TileType.Traversable)
        {
            return; // No valid tile at this position
        }

        if (charactersParent == null)
        {
            charactersParent = new GameObject("Characters").transform;
            charactersParent.position = Vector3.zero;
            charactersParent.rotation = Quaternion.identity;
            charactersParent.localScale = Vector3.one;
        }

        if (_isPlayerCharacter)
        {
            if (playerCharacterPrefab == null)
            {
                return; // No player character prefab assigned
            }

            if (playerInstance == null) // First time spawning
            {
                playerInstance = Instantiate(playerCharacterPrefab, _worldSpaceTile.WorldPosition, Quaternion.identity, charactersParent);

                if (playerInstance is PlayerCharacter _player)
                {
                    _player.CurrentMap = mapRenderer.Map;
                    _player.PathfindingProvider = mapRenderer;
                    _player.SetRanges(attackRange, movementRange, false); // Set ranges without recalculating tiles yet
                }
            }
            else // Move existing player
            {
                playerInstance.transform.position = _worldSpaceTile.WorldPosition;
            }

            playerInstance.SetOccuipiedTile(_worldSpaceTile);
        }
        else
        {
            if (enemyCharacterPrefab == null)
            {
                return; // No enemy character prefab assigned
            }

            GridObject _newEnemy = Instantiate(enemyCharacterPrefab, _worldSpaceTile.WorldPosition, Quaternion.identity, charactersParent);
            _newEnemy.SetOccuipiedTile(_worldSpaceTile);

            spawnedEnemyInstances.Add(_newEnemy);
        }
    }

    private void reassignTiles(Pathfinding _pathfinding, WorldSpaceTile[,] _grid)
    {
        if (_grid == null
            || mapRenderer == null
            || mapRenderer.Map == null
            || mapRenderer.Map.IsValid == false
            || mapRenderer.Map.Width != _grid.GetLength(0)
            || mapRenderer.Map.Height != _grid.GetLength(1))
        {
            return;
        }

        Vector2Int _mapSize = mapRenderer.Map.Size;
        _reassign(playerInstance, _mapSize, _grid);

        for (int i = 0; i < spawnedEnemyInstances.Count; i++)
        {
            _reassign(spawnedEnemyInstances[i], _mapSize, _grid);
        }

        void _reassign(GridObject _obj, Vector2Int _mapSize, WorldSpaceTile[,] pathfindingGrid)
        {
            if (_obj == null)
            {
                return;
            }

            Vector2Int _gridPos = _obj.transform.position.GetGridPosition();

            if (!_gridPos.IsInGridBounds(_mapSize))
            {
                Destroy(_obj.gameObject);
                return;
            }

            _obj.SetOccuipiedTile(pathfindingGrid[_gridPos.x, _gridPos.y]);
        }
    }
}
