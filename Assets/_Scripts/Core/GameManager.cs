using UnityEngine;
using Fusion;

public enum GameState
{
    MainMenu = 0,
    Playing = 1,
    Combat = 2,
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
        SceneLoader.Instance.Load(SceneNames.GameScene);
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

//    public void Resume()
//    {
//        if (CurrentState != GameState.Paused)
//        {
//            Debug.LogWarning("Cannot resume the game because it is not paused.");
//            return;
//        }
//        CurrentState = GameState.Playing;
//        Time.timeScale = 1f;
//        Debug.Log("Game Resumed");
//        EventBus.Instance.RaiseGameResumed();
//    }
}

