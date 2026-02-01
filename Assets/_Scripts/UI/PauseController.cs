using UnityEngine;
using UnityEngine.UI;

public class PauseController : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;

    private void OnEnable()
    {
        if (EventBus.Instance != null)
        {
            EventBus.Instance.OnGamePaused += ShowPausePanel;
            EventBus.Instance.OnGameResumed += HidePausePanel;
        }
    }
    private void OnDisable()
    {
        if (EventBus.Instance != null)
        {
            EventBus.Instance.OnGamePaused -= ShowPausePanel;
            EventBus.Instance.OnGameResumed -= HidePausePanel;
        }
    }

    private void Start()
    {
        resumeButton.onClick.AddListener(OnResumeClicked);
        mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    private void TogglePause()
    {
        if (GameManager.Instance == null) 
        {
            Debug.LogWarning("GameManager instance is null. Cannot toggle pause state.");
            return;
        }
        if (GameManager.Instance.CurrentState == GameState.Playing)
        {
            GameManager.Instance.Pause();
        }
        else if (GameManager.Instance.CurrentState == GameState.Paused)
        {
            GameManager.Instance.Resume();
        }
    }

    void ShowPausePanel()
    { 
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
    }
    void HidePausePanel()
    {
        if (pausePanel)
        {
            pausePanel.SetActive(false);
        }
    }

    void OnResumeClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.Resume();
            pausePanel.SetActive(false);
        }
    }
    void OnMainMenuClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GoToMenu();
        }
    }
}
