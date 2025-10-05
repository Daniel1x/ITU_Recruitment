using UnityEditor;
using UnityEngine;

public class MapDataEditor : EditorWindow
{
    [SerializeField] private MapRenderer mapRenderer = null;
    [SerializeField] private float tileScale = 16f;

    private Texture2D buttonTexture = null;
    private GUIStyle buttonStyle = null;

    [MenuItem("Tools/Map Data Editor")]
    public static void ShowWindow() => GetWindow<MapDataEditor>("Map Data Editor");

    private void OnGUI()
    {
        tileScale = EditorGUILayout.Slider("Tile Scale", tileScale, 4f, 32f);
        mapRenderer = EditorGUILayout.ObjectField("Map Data Container", mapRenderer, typeof(MapRenderer), true) as MapRenderer;

        if (mapRenderer == null)
        {
            if (FindFirstObjectByType<MapRenderer>() is MapRenderer _foundRenderer)
            {
                mapRenderer = _foundRenderer;
            }

            EditorGUILayout.HelpBox("Please assign a MapRenderer object to edit its MapData.", MessageType.Info);
            return;
        }

        if (mapRenderer.Map == null || !mapRenderer.Map.IsValid)
        {
            mapRenderer.Map = new MapData(5, 5);
            EditorUtility.SetDirty(mapRenderer);
        }

        using (var _check = new EditorGUI.ChangeCheckScope())
        {
            Vector2Int _newSize = EditorGUILayout.Vector2IntField("Map Size", mapRenderer.Map.Size);

            if (_check.changed && _newSize != mapRenderer.Map.Size)
            {
                Undo.RecordObject(mapRenderer, "Change Map Size");
                mapRenderer.Map.SetNewSize(_newSize, true);
                EditorUtility.SetDirty(mapRenderer);
            }
        }

        for (int y = mapRenderer.Map.Height - 1; y >= 0; y--)
        {
            EditorGUILayout.BeginHorizontal();

            for (int x = 0; x < mapRenderer.Map.Width; x++)
            {
                Vector2Int _gridPosition = new Vector2Int(x, y);
                TileType _currentTile = mapRenderer.Map[_gridPosition];
                Color _color = mapRenderer.Visualization.GetColor(_currentTile);
                Texture2D _texture = getAdjustedTexture(_color);

                if (GUILayout.Button(new GUIContent(((int)_currentTile).ToString(), _texture), getButtonStyle(_texture), GUILayout.Width(tileScale), GUILayout.Height(tileScale)))
                {
                    Undo.RecordObject(mapRenderer, "Change Tile Type");
                    mapRenderer.Map[_gridPosition] = _currentTile.Next();
                    EditorUtility.SetDirty(mapRenderer);
                }
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    private GUIStyle getButtonStyle(Texture2D _texture)
    {
        if (buttonStyle == null)
        {
            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.alignment = TextAnchor.MiddleCenter;            
            buttonStyle.normal.textColor = Color.black;
        }

        buttonStyle.normal.background = _texture;

        return buttonStyle;
    }

    private Texture2D getAdjustedTexture(Color _color)
    {
        if (buttonTexture == null)
        {
            buttonTexture = new Texture2D(1, 1);
        }

        buttonTexture.SetPixel(0, 0, _color);
        buttonTexture.Apply();

        return buttonTexture;
    }
}
