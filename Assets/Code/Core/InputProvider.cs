using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InputProvider : MonoBehaviour
{
    private CustomInputActions inputActions = null;

    /// <summary> True if left click, false if right click. Additionally provides the InputAction.CallbackContext. </summary>
    public event UnityAction<bool, InputAction.CallbackContext> OnMouseClick = null;

    /// <summary> True if next page, false if previous page. </summary>
    public event UnityAction<bool> OnChagePage = null;
    public event UnityAction OnToggleCameraLock = null;
    public event UnityAction OnExit = null;

    public Vector2 Move => inputActions.Player.Move.ReadValue<Vector2>();
    public Vector2 Look => inputActions.Player.Look.ReadValue<Vector2>();
    public bool Sprint => inputActions.Player.Sprint.IsPressed();

    private void Awake()
    {
        inputActions = new();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();

        inputActions.Player.LeftClick.performed += onLeftClick;
        inputActions.Player.RightClick.performed += onRightClick;
        inputActions.Player.ToggleCamera.performed += onToggleCameraLock;
        inputActions.Player.Next.performed += onNextPage;
        inputActions.Player.Previous.performed += onPreviousPage;
        inputActions.Player.Exit.performed += onExit;
    }

    private void OnDisable()
    {
        inputActions.Player.LeftClick.performed -= onLeftClick;
        inputActions.Player.RightClick.performed -= onRightClick;
        inputActions.Player.ToggleCamera.performed -= onToggleCameraLock;
        inputActions.Player.Next.performed -= onNextPage;
        inputActions.Player.Previous.performed -= onPreviousPage;
        inputActions.Player.Exit.performed -= onExit;

        inputActions.Player.Disable();
    }

    private void OnDestroy()
    {
        inputActions.Dispose();
    }

    private void onLeftClick(InputAction.CallbackContext _context) => OnMouseClick?.Invoke(true, _context);
    private void onRightClick(InputAction.CallbackContext _context) => OnMouseClick?.Invoke(false, _context);
    private void onToggleCameraLock(InputAction.CallbackContext _context) => OnToggleCameraLock?.Invoke();
    private void onNextPage(InputAction.CallbackContext _context) => OnChagePage?.Invoke(true);
    private void onPreviousPage(InputAction.CallbackContext _context) => OnChagePage?.Invoke(false);
    private void onExit(InputAction.CallbackContext _context) => OnExit?.Invoke();
}
