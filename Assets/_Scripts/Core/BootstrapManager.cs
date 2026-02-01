using UnityEditor.SearchService;
using UnityEngine;

public class BootstrapManager : MonoBehaviour
{
    private static bool _initialized = false;

    private void Awake()
    {
        if (_initialized)
        { 
            Destroy(gameObject);
            return;
        }
        _initialized = true;
        DontDestroyOnLoad(gameObject);

        CreateGameManager();
        CreateSceneLoader();
        CreateEventBus();

        SceneLoader.Instance.Load(SceneNames.MainMenu);
    }

    private void CreateGameManager()
    {
        GameManager existing = FindFirstObjectByType<GameManager>();
        if (existing != null)
        {
            DontDestroyOnLoad(existing.gameObject);
            return;
        }

        GameObject go = new GameObject("GameManager");
        go.AddComponent<GameManager>();
        DontDestroyOnLoad(go);
    }
    private void CreateSceneLoader()
    {
        SceneLoader existing = FindFirstObjectByType<SceneLoader>();
        if (existing != null)
        {
            DontDestroyOnLoad(existing.gameObject);
            return;
        }

        GameObject go = new GameObject("SceneLoader");
        go.AddComponent<SceneLoader>();
        DontDestroyOnLoad(go);
    }
    private void CreateEventBus()
    {
        EventBus existing = FindFirstObjectByType<EventBus>();
        if (existing != null)
        {
            DontDestroyOnLoad(existing.gameObject);
            return;
        }

        GameObject go = new GameObject("EventBus");
        go.AddComponent<EventBus>();
        DontDestroyOnLoad(go);
    }

}
