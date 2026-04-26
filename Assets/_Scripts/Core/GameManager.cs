using UnityEngine;
using Fusion;

public enum GameState
{
    MainMenu = 0,
    Playing = 1,
    Combat = 2,
    Lost = 3,
    Won = 4,
}
public class GameManager : NetworkBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }
    public GameState CurrentState { get; private set; } = GameState.MainMenu;

    public override void Spawned()
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
        if (InputManager.Instance != null)
        {
            InputManager.Instance.EnablePlayerInput();
        }
        Debug.Log("Game Started");
        if (PlayerRolesController.Instance != null)
        {
            PlayerRolesController.Instance.ApplyRole();
        }
    }

    public void GoToMenu()
    {
        CurrentState = GameState.MainMenu;
        Time.timeScale = 1f;
        SceneLoader.Instance.Load(SceneNames.MainMenu);
        SceneLoader.Instance.LoadWithLoading(SceneNames.GameScene);
        if (InputManager.Instance != null)
        {
            InputManager.Instance.EnableUIInput();
        }
        Debug.Log("Got to Main Menu");
    }

    /// <summary>
    /// Переводит игру в состояние поражения и включает UI-ввод.
    /// </summary>
    public void EnterLoseState()
    {
        if (CurrentState != GameState.Playing)
            return;

        CurrentState = GameState.Lost;
        Time.timeScale = 0f;
        if (InputManager.Instance != null)
            InputManager.Instance.EnableUIInput();
        Debug.Log("Game lost");
    }

    /// <summary>
    /// Переводит игру в состояние победы и включает UI-ввод.
    /// </summary>
    public void EnterWinState()
    {
        if (CurrentState != GameState.Playing)
            return;

        CurrentState = GameState.Won;
        Time.timeScale = 0f;
        if (InputManager.Instance != null)
            InputManager.Instance.EnableUIInput();
        Debug.Log("Game won");
    }
}