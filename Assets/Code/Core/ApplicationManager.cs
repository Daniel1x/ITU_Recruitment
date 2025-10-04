using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ApplicationManager : ScreenClickValidator
{
    [SerializeField] private InputProvider inputProvider = null;
    [SerializeField] private Button reloadLevelButton = null;
    [SerializeField] private Button quitButton = null;

    private void Awake()
    {
        if (inputProvider != null)
        {
            inputProvider.OnExit += toggleApplicationManager;
        }
    }

    private void OnDestroy()
    {
        if (inputProvider != null)
        {
            inputProvider.OnExit -= toggleApplicationManager;
        }
    }

    private void Start()
    {
        gameObject.SetActive(false);
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (reloadLevelButton != null)
        {
            reloadLevelButton.onClick.AddListener(onReloadLevelButtonClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(onQuitButtonClicked);
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (reloadLevelButton != null)
        {
            reloadLevelButton.onClick.RemoveListener(onReloadLevelButtonClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(onQuitButtonClicked);
        }
    }

    private void toggleApplicationManager()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }

    private void onReloadLevelButtonClicked() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    private void onQuitButtonClicked() => Application.Quit();

    public override bool IsClickValid(Camera _viewCamera, Vector2 _mousePosition, Vector2 _viewportPosition)
    {
        return false; // Always block clicks when this validator is active
    }
}
