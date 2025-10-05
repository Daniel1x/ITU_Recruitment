using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class FreeCamera : MonoBehaviour
{
    private const float MAX_PITCH_ANGLE = 89f;
    private static readonly Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

    public event UnityAction<bool, Vector3> OnMouseClickAtGroundPosition = null;

    [Header("Camera Settings")]
    [SerializeField] private float minY = 0f;
    [SerializeField] private float maxY = 100f;
    [SerializeField] private float movementSpeed = 10f;
    [SerializeField] private float sprintMultiplier = 2f;
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private InputProvider inputProvider = null;

    private bool isCameraLockedAtPosition = true;
    private Camera thisCamera = null;

    private void Awake()
    {
        thisCamera = GetComponent<Camera>();
    }

    private void OnEnable()
    {
        updateCameraLockState();

        if (inputProvider != null)
        {
            inputProvider.OnToggleCameraLock += toggleCameraLock;
            inputProvider.OnMouseClick += handleMouseClick;
        }
    }

    private void OnDisable()
    {
        if (inputProvider != null)
        {
            inputProvider.OnToggleCameraLock -= toggleCameraLock;
            inputProvider.OnMouseClick -= handleMouseClick;
        }
    }

    private void Update()
    {
        if (isCameraLockedAtPosition || inputProvider == null)
        {
            return;
        }

        handleMovement();
        handleRotation();
    }

    private void handleMovement()
    {
        Vector2 _moveInput = inputProvider.Move;

        if (_moveInput == Vector2.zero)
        {
            return;
        }

        float _movementScale = movementSpeed * Time.deltaTime;

        if (inputProvider.Sprint)
        {
            _movementScale *= sprintMultiplier; // Increase speed when sprinting
        }

        _moveInput *= _movementScale;

        // Move in the direction the camera is facing
        transform.Translate(new Vector3(_moveInput.x, 0f, _moveInput.y), Space.Self);

        // Clamp the Y position
        Vector3 _position = transform.position;

        if (_position.y < minY || _position.y > maxY)
        {
            _position.y = Mathf.Clamp(_position.y, minY, maxY);
            transform.position = _position;
        }
    }

    private void handleRotation()
    {
        Vector2 _lookInput = inputProvider.Look;

        if (_lookInput == Vector2.zero)
        {
            return;
        }

        transform.Rotate(Vector3.up, _lookInput.x * rotationSpeed * Time.deltaTime, Space.World);

        Vector3 _angles = transform.eulerAngles;
        float _pitchInput = -_lookInput.y * rotationSpeed * Time.deltaTime; // Invert Y for typical camera control
        float _currentPitch = _angles.x;

        // Adjust for Unity's 0-360 degree representation
        if (_currentPitch > 180f)
        {
            _currentPitch -= 360f;
        }

        _angles.x = Mathf.Clamp(_currentPitch + _pitchInput, -MAX_PITCH_ANGLE, MAX_PITCH_ANGLE);

        transform.eulerAngles = _angles;
    }

    private void toggleCameraLock()
    {
        isCameraLockedAtPosition = !isCameraLockedAtPosition;
        updateCameraLockState();
    }

    // Updates the cursor lock state and visibility based on whether the camera is locked.
    private void updateCameraLockState()
    {
        Cursor.lockState = isCameraLockedAtPosition
            ? CursorLockMode.None
            : CursorLockMode.Locked;

        Cursor.visible = isCameraLockedAtPosition;
    }

    /// <summary> Handles a mouse click event, determining whether the click is valid and invoking the appropriate action if so. </summary>
    private void handleMouseClick(bool _isLeftClick, InputAction.CallbackContext _context)
    {
        if (thisCamera == null
            || !isCameraLockedAtPosition
            || _context.control.device is not Mouse _mouse)
        {
            return;
        }

        Vector2 _mousePosition = _mouse.position.ReadValue();
        Vector2 _viewportPosition = thisCamera.ScreenToViewportPoint(_mousePosition);

        if (ScreenClickValidator.IsClickBlocked(thisCamera, _mousePosition, _viewportPosition))
        {
            return; // Click is blocked by a validator
        }

        Ray _ray = thisCamera.ScreenPointToRay(_mousePosition);

        if (groundPlane.Raycast(_ray, out float _enter))
        {
            OnMouseClickAtGroundPosition?.Invoke(_isLeftClick, _ray.GetPoint(_enter));
        }
    }
}
