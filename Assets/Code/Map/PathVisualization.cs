using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PathVisualization : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new Vector3(0f, 0.1f, 0f);

    private LineRenderer lineRenderer = null;
    private int validWaypointCount = 0; // Number of valid waypoints in the array
    private NativeArray<Vector3> waypoints; // Preallocated array for waypoints

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;

        // Initial allocation of waypoints array, will resize if needed
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

        // Resize waypoints array if necessary
        if (!waypoints.IsCreated || waypoints.Length < _path.Count)
        {
            if (waypoints.IsCreated)
            {
                waypoints.Dispose();
            }

            int _newSize = Mathf.Max(waypoints.Length * 2, _path.Count);
            waypoints = new NativeArray<Vector3>(_newSize, Allocator.Persistent);
        }

        validWaypointCount = _path.Count;

        for (int i = 0; i < validWaypointCount; i++)
        {
            waypoints[i] = _path[i].Tile.WorldPosition + offset;
        }

        if (lineRenderer != null)
        {
            // Update LineRenderer with valid waypoints, using only the portion of the array that contains valid data
            lineRenderer.positionCount = validWaypointCount;
            lineRenderer.SetPositions(waypoints.GetSubArray(0, validWaypointCount));
        }

        gameObject.SetActive(true);
    }
}
