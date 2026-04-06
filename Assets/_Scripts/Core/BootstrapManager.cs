using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class BootstrapManager : MonoBehaviour
{
    private static bool _initialized = false;

    private GameObject NetworkRunner;

    private void Awake()
    {
        NetworkRunner = new GameObject("Fusion_Runtime_Runner");
        var runner = NetworkRunner.AddComponent<NetworkRunner>();
        var sceneManager = NetworkRunner.AddComponent<NetworkSceneManagerDefault>();
        var eventManager = NetworkRunner.AddComponent<NetworkEvents>();

        if (NetworkRunner != null)
            DontDestroyOnLoad(NetworkRunner.gameObject);
        else
            Debug.LogWarning("No NetworkRunner found in the scene. If you are using Fusion, please ensure a NetworkRunner is present in the Bootstrap scene.");

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
        CreateInputManager();

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
    private void CreateInputManager()
    {
        InputManager existing = FindFirstObjectByType<InputManager>();
        if (existing != null)
        {
            DontDestroyOnLoad(existing.gameObject);
            return;
        }
        GameObject go = new GameObject("InputManager");
        InputManager inputManager = go.AddComponent<InputManager>();
        inputManager.inputActions = Resources.Load<InputActionAsset>("InputSystem_Actions");
        if (inputManager.inputActions == null)
        {
            Debug.LogError("Failed to load InputActionAsset from Resources/InputSystem_Actions. Please ensure the asset exists and is in the correct folder.");
        }
        DontDestroyOnLoad(go);
    }

}
