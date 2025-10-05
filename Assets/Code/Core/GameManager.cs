using TMPro;
using UnityEngine;

/// <summary> Manages the overall game state and transitions between different modes of operation. </summary>
public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        MapEditing = 0,
        UnitPlacement = 1,
        PathfindingTesting = 2,
    }

    private const string MAP_EDITING_INFO = "Click on map to change tiles";
    private const string UNIT_PLACEMENT_INFO = "Left-click to place player\nRight-click to place enemy";
    private const string PATHFINDING_TESTING_INFO = "Click on map to move player";
    private static readonly int STATE_COUNT = System.Enum.GetValues(typeof(GameState)).Length;

    [Header("References")]
    [SerializeField] private InputProvider inputProvider = null;
    [SerializeField] private MapSettingsMenu mapSettingsMenu = null;
    [SerializeField] private PlayerSettingsMenu playerSettingsMenu = null;
    [SerializeField] private PathfindingTestingMenu pathfindingTestingMenu = null;

    [Header("State Display")]
    [SerializeField] private TMP_Text mode = null;
    [SerializeField] private TMP_Text additionalInfo = null;

    public GameState CurrentGameState { get; private set; } = GameState.MapEditing;

    private void OnEnable()
    {
        if (inputProvider != null)
        {
            inputProvider.OnChagePage += onChangePage;
        }
    }

    private void OnDisable()
    {
        if (inputProvider != null)
        {
            inputProvider.OnChagePage -= onChangePage;
        }
    }

    private void Start()
    {
        if (mapSettingsMenu != null)
        {
            mapSettingsMenu.gameObject.SetActive(false);
        }

        if (playerSettingsMenu != null)
        {
            playerSettingsMenu.gameObject.SetActive(false);
        }

        if (pathfindingTestingMenu != null)
        {
            pathfindingTestingMenu.gameObject.SetActive(false);
        }

        setNewState(CurrentGameState, true);
    }

    private void onChangePage(bool _next)
    {
        GameState _newState = _next
            ? (GameState)(((int)CurrentGameState + 1) % STATE_COUNT)
            : (GameState)(((int)CurrentGameState - 1 + STATE_COUNT) % STATE_COUNT);

        setNewState(_newState);
    }

    /// <summary> Sets the new game state, handling transitions and UI updates. </summary>
    private void setNewState(GameState _newState, bool _force = false)
    {
        if (_newState == CurrentGameState && _force == false)
        {
            return; // No change
        }

        switch (CurrentGameState)
        {
            case GameState.MapEditing:

                if (mapSettingsMenu != null)
                {
                    mapSettingsMenu.gameObject.SetActive(false);
                }

                break;

            case GameState.UnitPlacement:

                if (playerSettingsMenu != null)
                {
                    playerSettingsMenu.gameObject.SetActive(false);
                }

                break;

            case GameState.PathfindingTesting:

                if (pathfindingTestingMenu != null)
                {
                    if (pathfindingTestingMenu.IsMovementInProgress)
                    {
                        return; // Don't allow changing state while movement is in progress
                    }

                    pathfindingTestingMenu.gameObject.SetActive(false);
                }

                break;
        }

        CurrentGameState = _newState;
        updateStateDisplay();

        switch (CurrentGameState)
        {
            case GameState.MapEditing:

                if (mapSettingsMenu != null)
                {
                    mapSettingsMenu.gameObject.SetActive(true);
                }

                break;
            case GameState.UnitPlacement:

                if (playerSettingsMenu != null)
                {
                    playerSettingsMenu.gameObject.SetActive(true);
                }

                break;

            case GameState.PathfindingTesting:

                if (pathfindingTestingMenu != null)
                {
                    pathfindingTestingMenu.gameObject.SetActive(true);
                }

                break;
        }
    }

    private void updateStateDisplay()
    {
        if (mode != null)
        {
            mode.text = "Mode:" + CurrentGameState;
        }

        if (additionalInfo != null)
        {
            additionalInfo.text = CurrentGameState switch
            {
                GameState.MapEditing => MAP_EDITING_INFO,
                GameState.UnitPlacement => UNIT_PLACEMENT_INFO,
                GameState.PathfindingTesting => PATHFINDING_TESTING_INFO,
                _ => string.Empty
            };
        }
    }
}
