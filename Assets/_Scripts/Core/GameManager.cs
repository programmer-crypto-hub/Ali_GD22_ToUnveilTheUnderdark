using System.Diagnostics.Contracts;
using UnityEngine;

public enum GameState
{
    MainMenu = 0,
    Playing = 1,
    Paused = 2,
}
public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }
    public GameState CurrentState { get; private set; } = GameState.MainMenu;

    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public void StartGame()
    {
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;
        SceneLoader.Instance.Load(SceneNames.GameScene);
        Debug.Log("Game Started");
    }

    public void GoToMenu()
    {
        CurrentState = GameState.MainMenu;
        //Time.timeScale = 1f;
        SceneLoader.Instance.Load(SceneNames.MainMenu);
        Debug.Log("Got to Main Menu");
    }

    public void Pause()
    {
        CurrentState = GameState.Paused;
        Time.timeScale = 0f;
        Debug.Log("Got to Main Menu");
        EventBus.Instance.RaiseGamePaused();
    }

    public void Resume()
    {
        if (CurrentState != GameState.Paused)
        {
            Debug.LogWarning("Cannot resume the game because it is not paused.");
            return;
        }
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;
        Debug.Log("Game Resumed");
        EventBus.Instance.RaiseGameResumed();
    }
}
