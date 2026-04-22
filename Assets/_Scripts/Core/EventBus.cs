using Fusion;
using System;

public class EventBus : NetworkBehaviour
{
    public static EventBus Instance { get; private set; }

    // Add this new event
    public event Action OnMapGenerated;

    public event Action OnGamePaused;
    public event Action OnGameResumed;

    public override void Spawned()
    {
        // Fusion handles Singleton logic differently; 
        // strictly ensure only one exists globally.
        if (Instance == null) Instance = this;
    }

    // Call this to tell the UI/Game the map is ready
    public void RaiseMapGenerated()
    {
        OnMapGenerated?.Invoke();
    }

    public void RaiseGamePaused() => OnGamePaused?.Invoke();
    public void RaiseGameResumed() => OnGameResumed?.Invoke();
}
