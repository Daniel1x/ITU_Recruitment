using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static MapRenderer.VisualizationSettings;

/// <summary> Renders a grid-based map in the world using instanced drawing for performance, and provides pathfinding functionality. </summary>
public class MapRenderer : MonoBehaviour, IPathfindingProvider
{
    /// <summary> Holds a group of tiles that share the same material and can be drawn together using instancing. </summary>
    [System.Serializable]
    public class TileGroup
    {
        private readonly Material material = null;
        private readonly List<Matrix4x4> matrices = new List<Matrix4x4>();

        public TileGroup(Material _mat)
        {
            material = _mat;
        }

        public void Clear() => matrices.Clear();
        public void Add(Matrix4x4 _matrix) => matrices.Add(_matrix);

        public void Draw(Mesh _mesh)
        {
            if (matrices.Count > 0 && material != null)
            {
                Graphics.DrawMeshInstanced(_mesh, 0, material, matrices);
            }
        }
    }

    /// <summary> Holds groups of tiles categorized by their type and state (movement/attack range). </summary>
    [System.Serializable]
    public class TileTypeGroup
    {
        public TileGroup DefaultTiles { get; private set; } = null;
        public TileGroup TilesInMovementRange { get; private set; } = null;
        public TileGroup TilesInAttackRange { get; private set; } = null;
        public TileGroup TilesInMovementAndAttackRange { get; private set; } = null;

        public TileTypeGroup(MaterialGroup _group)
        {
            DefaultTiles = new TileGroup(_group.OnlyColor);
            TilesInMovementRange = new TileGroup(_group.ColorAndMovement);
            TilesInAttackRange = new TileGroup(_group.ColorAndAttack);
            TilesInMovementAndAttackRange = new TileGroup(_group.Full);
        }

        public void Add(WorldSpaceTile _tile)
        {
            if (_tile == null)
            {
                return;
            }

            Matrix4x4 _matrix = Matrix4x4.TRS(_tile.WorldPosition, Quaternion.Euler(90f, 0f, 0f), Vector3.one);

            if (_tile.IsInMovementRange && _tile.IsInAttackRange)
            {
                TilesInMovementAndAttackRange.Add(_matrix);
                return;
            }

            if (_tile.IsInMovementRange)
            {
                TilesInMovementRange.Add(_matrix);
                return;
            }

            if (_tile.IsInAttackRange)
            {
                TilesInAttackRange.Add(_matrix);
                return;
            }

            DefaultTiles.Add(_matrix);
        }

        public void Clear()
        {
            DefaultTiles.Clear();
            TilesInMovementRange.Clear();
            TilesInAttackRange.Clear();
            TilesInMovementAndAttackRange.Clear();
        }

        public void Draw(Mesh _mesh)
        {
            DefaultTiles.Draw(_mesh);
            TilesInMovementRange.Draw(_mesh);
            TilesInAttackRange.Draw(_mesh);
            TilesInMovementAndAttackRange.Draw(_mesh);
        }
    }

    /// <summary> Settings for visualizing the map tiles, including materials and colors for different tile types and states. </summary>
    [System.Serializable]
    public class VisualizationSettings : System.IDisposable
    {
        /// <summary> Holds different material variations for a specific tile type based on its state (movement/attack range). </summary>
        [System.Serializable]
        public class MaterialGroup : System.IDisposable
        {
            public Material OnlyColor { get; private set; } = null;
            public Material ColorAndMovement { get; private set; } = null;
            public Material ColorAndAttack { get; private set; } = null;
            public Material Full { get; private set; } = null;

            public MaterialGroup(Material _baseMaterial, Color _baseColor, Color _movementColor, Color _attackColor, Texture2D _texture)
            {
                if (_baseMaterial == null)
                {
                    return;
                }

                OnlyColor = new Material(_baseMaterial);
                ColorAndMovement = new Material(_baseMaterial);
                ColorAndAttack = new Material(_baseMaterial);
                Full = new Material(_baseMaterial);

                setProperties(OnlyColor, _baseColor, _baseColor, _baseColor, _texture);
                setProperties(ColorAndMovement, _baseColor, _movementColor, _baseColor, _texture);
                setProperties(ColorAndAttack, _baseColor, _baseColor, _attackColor, _texture);
                setProperties(Full, _baseColor, _movementColor, _attackColor, _texture);
            }

            ~MaterialGroup()
            {
                Dispose();
            }

            private void setProperties(Material _mat, Color _baseColor, Color _movementColor, Color _attackColor, Texture2D _texture)
            {
                if (_mat == null)
                {
                    return;
                }

                _mat.SetColor(TILE_TYPE_COLOR_ID, _baseColor);
                _mat.SetColor(TILE_MOVEMENT_COLOR_ID, _movementColor);
                _mat.SetColor(TILE_ATTACK_COLOR_ID, _attackColor);
                _mat.SetTexture(TILE_TEXTURE_ID, _texture);
            }

            public void Dispose()
            {
                if (OnlyColor != null)
                {
                    Object.Destroy(OnlyColor);
                    OnlyColor = null;
                }

                if (ColorAndMovement != null)
                {
                    Object.Destroy(ColorAndMovement);
                    ColorAndMovement = null;
                }

                if (ColorAndAttack != null)
                {
                    Object.Destroy(ColorAndAttack);
                    ColorAndAttack = null;
                }

                if (Full != null)
                {
                    Object.Destroy(Full);
                    Full = null;
                }

                System.GC.SuppressFinalize(this);
            }
        }

        private const string TILE_TYPE_COLOR_PROPERTY = "_TypeColor";
        private const string TILE_MOVEMENT_COLOR_PROPERTY = "_MovementColor";
        private const string TILE_ATTACK_COLOR_PROPERTY = "_RangeColor";
        private const string TILE_TEXTURE_PROPERTY = "_MainTex";

        private static readonly int TILE_TYPE_COLOR_ID = Shader.PropertyToID(TILE_TYPE_COLOR_PROPERTY);
        private static readonly int TILE_MOVEMENT_COLOR_ID = Shader.PropertyToID(TILE_MOVEMENT_COLOR_PROPERTY);
        private static readonly int TILE_ATTACK_COLOR_ID = Shader.PropertyToID(TILE_ATTACK_COLOR_PROPERTY);
        private static readonly int TILE_TEXTURE_ID = Shader.PropertyToID(TILE_TEXTURE_PROPERTY);

        [SerializeField] private Texture2D traversableTexture = null;
        [SerializeField] private Texture2D obstacleTexture = null;
        [SerializeField] private Texture2D coverTexture = null;
        [SerializeField] private Color traversableColor = Color.white;
        [SerializeField] private Color obstacleColor = Color.black;
        [SerializeField] private Color coverColor = Color.gray;
        [SerializeField] private Color movementRangeColor = Color.green;
        [SerializeField] private Color attackRangeColor = Color.red;

        [SerializeField] private Material tileMaterial = null;

        private MaterialGroup traversable = null;
        private MaterialGroup obstacle = null;
        private MaterialGroup cover = null;

        ~VisualizationSettings()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (traversable != null)
            {
                traversable.Dispose();
                traversable = null;
            }

            if (obstacle != null)
            {
                obstacle.Dispose();
                obstacle = null;
            }

            if (cover != null)
            {
                cover.Dispose();
                cover = null;
            }

            System.GC.SuppressFinalize(this);
        }

        public void Initialize()
        {
            traversable = new MaterialGroup(tileMaterial, traversableColor, movementRangeColor, attackRangeColor, traversableTexture);
            obstacle = new MaterialGroup(tileMaterial, obstacleColor, movementRangeColor, attackRangeColor, obstacleTexture);
            cover = new MaterialGroup(tileMaterial, coverColor, movementRangeColor, attackRangeColor, coverTexture);
        }

        public MaterialGroup GetGroup(TileType _type)
        {
            return _type switch
            {
                TileType.Traversable => traversable,
                TileType.Obstacle => obstacle,
                TileType.Cover => cover,
                _ => traversable,
            };
        }

        public Color GetColor(TileType _type)
        {
            return _type switch
            {
                TileType.Traversable => traversableColor,
                TileType.Obstacle => obstacleColor,
                TileType.Cover => coverColor,
                _ => traversableColor,
            };
        }
    }

    public event UnityAction<Pathfinding, WorldSpaceTile[,]> OnPathfindingUpdated = null;

    [SerializeField] private MapData mapData = new(5, 5);
    [SerializeField] private VisualizationSettings settings = new();
    [SerializeField] private Mesh meshToDraw = null;
    [SerializeField] private WorldSpaceTile tilePrefab = null;

    private TileTypeGroup traversable = null;
    private TileTypeGroup obstacle = null;
    private TileTypeGroup cover = null;
    private List<WorldSpaceTile> spawnedTiles = new List<WorldSpaceTile>();
    private WorldSpaceTile[,] pathfindingGrid = null;

    public MapData Map { get => mapData; set => mapData = value; }
    public Pathfinding Pathfinding { get; private set; } = null;
    public WorldSpaceTile[,] PathfindingGrid => pathfindingGrid;
    public VisualizationSettings Visualization => settings;

    private void Awake()
    {
        settings.Initialize();

        traversable = new TileTypeGroup(settings.GetGroup(TileType.Traversable));
        obstacle = new TileTypeGroup(settings.GetGroup(TileType.Obstacle));
        cover = new TileTypeGroup(settings.GetGroup(TileType.Cover));
    }

    private void OnDestroy()
    {
        if (settings != null)
        {
            settings.Dispose();
        }

        if (Pathfinding != null)
        {
            Pathfinding.Dispose();
            Pathfinding = null;
        }
    }

    private void OnEnable()
    {
        if (Map != null)
        {
            Map.OnMapDataChanged += onMapDataChanged;
        }
    }

    private void OnDisable()
    {
        if (Map != null)
        {
            Map.OnMapDataChanged -= onMapDataChanged;
        }
    }

    private void Start()
    {
        if (mapData != null && mapData.IsValid)
        {
            onMapDataChanged();
        }
    }

    private void Update()
    {
        if (mapData == null || mapData.IsValid == false)
        {
            return;
        }

        if (WorldSpaceTile.AnyVisualizationChanged)
        {
            traversable.Clear();
            obstacle.Clear();
            cover.Clear();

            WorldSpaceTile.PerformActionOnActiveTiles(addTileToProperGroup, true);
        }

        traversable.Draw(meshToDraw);
        obstacle.Draw(meshToDraw);
        cover.Draw(meshToDraw);
    }

    public void CheckCharacterAtPosition(Vector2Int _gridPosition, TileType _newType)
    {
        WorldSpaceTile _tile = GetTileAtPosition(_gridPosition);

        if (_tile.Occupant != null && _newType is not TileType.Traversable)
        {
            Destroy(_tile.Occupant.gameObject);
        }
    }

    public WorldSpaceTile GetTileAtPosition(Vector2Int _gridPosition)
    {
        if (Map == null
            || Map.IsValid == false
            || _gridPosition.IsInGridBounds(Map.Size) == false
            || pathfindingGrid == null
            || pathfindingGrid.Length != Map.TileCount
            || pathfindingGrid.GetLength(0) != Map.Width
            || pathfindingGrid.GetLength(1) != Map.Height)
        {
            return null;
        }

        return pathfindingGrid[_gridPosition.x, _gridPosition.y];
    }

    private void addTileToProperGroup(WorldSpaceTile _tile)
    {
        if (_tile == null)
        {
            return;
        }

        switch (_tile.Type)
        {
            case TileType.Traversable:
                traversable.Add(_tile);
                break;

            case TileType.Obstacle:
                obstacle.Add(_tile);
                break;

            case TileType.Cover:
                cover.Add(_tile);
                break;
        }
    }

    private void onMapDataChanged()
    {
        if (Map == null || Map.IsValid == false)
        {
            return;
        }

        int _requiredTiles = Map.TileCount;

        // Spawn new or disable excess tiles
        for (int i = 0; i < _requiredTiles || i < spawnedTiles.Count; i++)
        {
            if (i >= spawnedTiles.Count) // Need to spawn a new tile
            {
                if (tilePrefab == null)
                {
                    Debug.LogWarning($"[{nameof(MapRenderer)}] Cannot spawn new tile, because the tile prefab is not assigned.");
                    break;
                }

                WorldSpaceTile _newTile = Instantiate(tilePrefab, transform);
                spawnedTiles.Add(_newTile);
            }
            else if (i >= _requiredTiles) // Disable excess tile
            {
                spawnedTiles[i].gameObject.SetActive(false);
            }
        }

        pathfindingGrid = new WorldSpaceTile[Map.Width, Map.Height];

        // Setup all active tiles
        for (int y = 0; y < Map.Height; y++)
        {
            for (int x = 0; x < Map.Width; x++)
            {
                Vector2Int _gridPosition = new Vector2Int(x, y);
                int _index = Map.GetIndex(_gridPosition);
                WorldSpaceTile _tile = spawnedTiles[_index];

                if (_tile == null)
                {
                    continue;
                }

                _tile.transform.position = _gridPosition.GetWorldPosition();
                _tile.SetTileData(_gridPosition, Map[_gridPosition]);
                _tile.gameObject.SetActive(true);

                pathfindingGrid[x, y] = _tile;
            }
        }

        if (Pathfinding != null)
        {
            Pathfinding.Dispose();
        }

        Pathfinding = new Pathfinding(pathfindingGrid);
        OnPathfindingUpdated?.Invoke(Pathfinding, pathfindingGrid);
    }
}
