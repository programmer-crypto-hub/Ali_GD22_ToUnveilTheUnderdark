using UnityEngine;

public static class GameInitializer
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void InitializeServices()
    {
        // Check if they exist, if not, create them and set DontDestroyOnLoad
        EnsureServiceExists<EventBus>("EventBus");
        EnsureServiceExists<InputManager>("InputManager");
    }

    private static void EnsureServiceExists<T>(string name) where T : Component
    {
        if (Object.FindFirstObjectByType<T>() == null)
        {
            GameObject go = new GameObject(name);
            go.AddComponent<T>();
            Object.DontDestroyOnLoad(go);
        }
    }
}
