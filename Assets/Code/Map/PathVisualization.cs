using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PathVisualization : MonoBehaviour
{
    [SerializeField] private float yOffset = 0.1f;

    private int validWaypointCount = 0;
    private NativeArray<Vector3> waypoints;
    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
        waypoints = new NativeArray<Vector3>(100, Allocator.Persistent);
    }

    private void OnDestroy()
    {
        if (waypoints.IsCreated)
        {
            waypoints.Dispose();
        }
    }

    public void SetPath(List<PathNode> _path)
    {
        if (_path == null || _path.Count <= 1)
        {
            gameObject.SetActive(false);
            return;
        }

        if (!waypoints.IsCreated || waypoints.Length < _path.Count)
        {
            if (waypoints.IsCreated)
            {
                waypoints.Dispose();
            }

            int _newSize = Mathf.Max(waypoints.Length * 2, _path.Count);
            waypoints = new NativeArray<Vector3>(_newSize, Allocator.Persistent);
        }

        Vector3 _offset = new Vector3(0f, yOffset, 0f);
        validWaypointCount = _path.Count;

        for (int i = 0; i < validWaypointCount; i++)
        {
            waypoints[i] = _path[i].Tile.WorldPosition + _offset;
        }

        updateLineRenderer();
        gameObject.SetActive(true);
    }

    private void updateLineRenderer()
    {
        if (lineRenderer == null)
        {
            return;
        }

        lineRenderer.positionCount = validWaypointCount;
        lineRenderer.SetPositions(waypoints.GetSubArray(0, validWaypointCount));
    }
}
