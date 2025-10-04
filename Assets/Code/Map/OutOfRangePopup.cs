using UnityEngine;

public class OutOfRangePopup : MonoBehaviour
{
    [SerializeField, Min(0f)] private float displayDuration = 1.5f;
    [SerializeField] private Vector3 positionOffset = new Vector3(0f, 2f, 0f);

    private float displayTimer = 0f;
    private FreeCamera viewCamera = null;

    private void Update()
    {
        displayTimer -= Time.deltaTime;

        if (displayTimer <= 0f)
        {
            gameObject.SetActive(false);
            return;
        }

        adjustRotationToViewCamera();
    }

    public void Show(Vector3 _position, FreeCamera _viewCamera)
    {
        viewCamera = _viewCamera;
        displayTimer = displayDuration;
        transform.position = _position + positionOffset;
        adjustRotationToViewCamera();

        gameObject.SetActive(true);
    }

    private void adjustRotationToViewCamera()
    {
        if (viewCamera != null)
        {
            transform.rotation = viewCamera.transform.rotation;
        }
    }
}
