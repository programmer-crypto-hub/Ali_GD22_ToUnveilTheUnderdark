using Fusion;
using UnityEngine;
using System;
using Unity.VisualScripting;

public class EventBus : MonoBehaviour
{
    public static EventBus Instance { get; private set; }

    // Add this new event
    public event Action OnMapGenerated;

    public event Action OnGamePaused;
    public event Action OnGameResumed;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // If an instance already exists, kill this one
            Destroy(gameObject);
            return; // Don't let any more code run in this Awake!
        }
    }

    // Call this to tell the UI/Game the map is ready
    public void RaiseMapGenerated()
    {
        OnMapGenerated?.Invoke();
    }

    public void RaiseGamePaused() => OnGamePaused?.Invoke();
    public void RaiseGameResumed() => OnGameResumed?.Invoke();
}
