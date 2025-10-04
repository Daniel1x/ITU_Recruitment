using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapSettingsMenu : RectTransformClickValidator
{
    [Header("Map Settings Menu")]
    [SerializeField] private FreeCamera freeCamera = null;
    [SerializeField] private MapRenderer mapRenderer = null;
    [SerializeField] private Slider widthSlider = null;
    [SerializeField] private TMP_Text widthValue = null;
    [SerializeField] private Slider heightSlider = null;
    [SerializeField] private TMP_Text heightValue = null;
    [Space]
    [SerializeField, Min(1)] private int minSize = 1;
    [SerializeField, Min(1)] private int maxSize = 100;

    protected override void OnEnable()
    {
        base.OnEnable();

        if (widthSlider != null)
        {
            var _initialWidth = mapRenderer != null && mapRenderer.Map != null
                ? mapRenderer.Map.Width
                : minSize;

            float _sliderValue = Mathf.InverseLerp(minSize, maxSize, _initialWidth);

            widthSlider.SetValueWithoutNotify(_sliderValue);
            widthSlider.onValueChanged.AddListener(onWidthChanged);
        }

        if (heightSlider != null)
        {
            var _initialHeight = mapRenderer != null && mapRenderer.Map != null
                ? mapRenderer.Map.Height
                : minSize;

            float _sliderValue = Mathf.InverseLerp(minSize, maxSize, _initialHeight);

            heightSlider.SetValueWithoutNotify(_sliderValue);
            heightSlider.onValueChanged.AddListener(onHeightChanged);
        }

        if (freeCamera != null)
        {
            freeCamera.OnMouseClickAtGroundPosition += onMouseClickAtGroundPosition;
        }

        updateMapStats();
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (widthSlider != null)
        {
            widthSlider.onValueChanged.RemoveListener(onWidthChanged);
        }

        if (heightSlider != null)
        {
            heightSlider.onValueChanged.RemoveListener(onHeightChanged);
        }

        if (freeCamera != null)
        {
            freeCamera.OnMouseClickAtGroundPosition -= onMouseClickAtGroundPosition;
        }
    }

    private void onWidthChanged(float _value) => updateMapStats();
    private void onHeightChanged(float _value) => updateMapStats();

    private void updateMapStats()
    {
        int _width = getMapSize(widthSlider != null ? widthSlider.value : 0f);
        int _height = getMapSize(heightSlider != null ? heightSlider.value : 0f);

        if (widthValue != null)
        {
            widthValue.text = _width.ToString();
        }

        if (heightValue != null)
        {
            heightValue.text = _height.ToString();
        }

        if (mapRenderer == null)
        {
            return;
        }

        if (mapRenderer.Map == null)
        {
            mapRenderer.Map = new MapData(_width, _height);
        }
        else
        {
            mapRenderer.Map.SetNewSize(new Vector2Int(_width, _height), true);
        }
    }

    private int getMapSize(float _progress)
    {
        return Mathf.RoundToInt(Mathf.Lerp(minSize, maxSize, _progress));
    }

    private void onMouseClickAtGroundPosition(bool _leftClick, Vector3 _groundPosition)
    {
        if (mapRenderer == null || mapRenderer.Map == null || !mapRenderer.Map.IsValid)
        {
            return;
        }

        Vector2Int _gridPosition = _groundPosition.GetGridPosition();

        if (!_gridPosition.IsInGridBounds(mapRenderer.Map.Size))
        {
            return;
        }

        TileType _currentTile = mapRenderer.Map[_gridPosition];
        TileType _newTile = _leftClick ? _currentTile.Next() : _currentTile.Previous();

        if (_currentTile != _newTile)
        {
            mapRenderer.Map[_gridPosition] = _newTile;
            mapRenderer.CheckCharacterAtPosition(_gridPosition, _newTile);
        }
    }
}
