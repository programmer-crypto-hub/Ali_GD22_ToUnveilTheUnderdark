using Fusion;
using UnityEngine;
using UnityEngine.InputSystem.XR.Haptics;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        MainMenu = 0,
        Playing = 1,
        Combat = 2, // Useful for when a player lands on an encounter
        Paused = 3,
        Lost = 4,
        Won = 5,
    }

    [Networked, OnChangedRender(nameof(OnStateChanged))] 
    public GameState CurrentState { get; private set; }

    public override void Spawned()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (HasStateAuthority) CurrentState = GameState.MainMenu;
    }

    // This is the "Brain" of your state machine
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
        Time.timeScale = 1f;
        InputManager.Instance?.EnablePlayerInput();
        // Close Shop/Inventory if they were open from another state
        ShopUIManager.Instance?.ToggleShop(false);
        Debug.Log("Switched to Exploration Mode.");
    }

    public void HandleCombat()
    {
        // For your D&D slice: Disable movement but keep UI active for attacks
        if (InputManager.Instance.playerActionMap.enabled == false)
        {
            InputManager.Instance.EnablePlayerInput();
            InputManager.Instance.EnableUIInput();
        }
        Debug.Log("Combat Initialized. Board movement frozen.");
    }

    public void HandleGameOver(GameState state)
    {
        Time.timeScale = 0f;
        InputManager.Instance?.EnableUIInput();
        // Trigger your Win/Loss UI screens here
        if (InputManager.Instance != null && state == GameState.Won)
        {
            Debug.Log("Congratulations! You've won the game!");
            // Show win screen
            InputManager.Instance.DisablePlayerInput(); // Prevent further actions after winning
        }
        else if (InputManager.Instance != null && state == GameState.Lost)
        {
            Debug.Log("Game Over! Better luck next time.");
            // Show loss screen
        }
    }

    private void HandleMainMenu()
    {
        Time.timeScale = 1f;
        InputManager.Instance?.EnableUIInput();
    }

    // --- State Change Requests (Server Only) ---

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestStateChange(GameState newState)
    {
        if (HasStateAuthority)
        {
            CurrentState = newState;
        }
    }
}
