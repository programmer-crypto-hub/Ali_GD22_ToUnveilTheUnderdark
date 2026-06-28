using Fusion;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        MainMenu = 0,
        Playing = 1,
        Combat = 2, 
        Paused = 3,
        Lost = 4,
        Won = 5,
    }

    [Networked, OnChangedRender(nameof(OnStateChanged))]
    public GameState CurrentState { get; private set; }

    public void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public override void Spawned()
    {
        if (HasStateAuthority) CurrentState = GameState.Combat;
    }

    // This is the Brain
    void OnStateChanged()
    {
        switch (CurrentState)
        {
            case GameState.MainMenu:
                HandleMainMenu();
                break;

            case GameState.Playing:
                HandleExploration();
                break;

            case GameState.Combat:
                HandleCombat();
                break;

            case GameState.Lost:
            case GameState.Won:
                HandleGameOver(CurrentState);
                break;
        }
    }

    public void HandleExploration()
    {
        if (HasStateAuthority) CurrentState = GameState.Playing;
        Time.timeScale = 1f;
        PlayerInputHandler.IsMyTurn = true;
        // Close Shop/Inventory if they were open from another state
        ShopUIManager.Instance?.ToggleShop(false);
        Debug.Log("Switched to Exploration Mode.");
    }

    public void HandleCombat()
    {
        if (HasStateAuthority) CurrentState = GameState.Combat;
        // Disable movement but keep UI active for attacks
        PlayerInputHandler.IsMyTurn = false;
        PlayerInputHandler.IsUIActive = true;
        Debug.Log("Combat Initialized. Board movement frozen.");
    }

    public void HandleGameOver(GameState state)
    {
        Time.timeScale = 0f;
        PlayerInputHandler.IsUIActive = true;
        // Trigger Win/Loss UI screens here
        if (state == GameState.Won)
        {
            Debug.Log("Congratulations! You've won the game!");
            // Show win screen
            PlayerInputHandler.IsUIActive = false; // Prevent further actions after winning
        }
        else if (state == GameState.Lost)
        {
            Debug.Log("Game Over! Better luck next time.");
            // Show loss screen
        }
    }

    public void HandleMainMenu()
    {
        Time.timeScale = 1f;
        PlayerInputHandler.IsUIActive = true;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestStateChange(GameState newState)
    {
        if (HasStateAuthority)
        {
            CurrentState = newState;
        }
    }
}
