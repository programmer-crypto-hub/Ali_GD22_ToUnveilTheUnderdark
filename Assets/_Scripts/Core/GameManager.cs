using Fusion;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    public GameState CurrentState { get; private set; }

    [Header("Level Sequence")]
    [Tooltip("Путь в Resources до LevelSequenceData без расширения .asset.")]
    [SerializeField] private string levelSequenceResourcePath = "Levels/LevelSequence_Default";
    [SerializeField] private LevelSequenceData levelSequenceOverride;

    /// Индекс текущего уровня из LevelSequenceData.
    /// -1 означает "не определён или fallback режим".
    /// </summary>
    public int CurrentLevelIndex => currentLevelIndex;

    private LevelSequenceData levelSequenceData;
    private int currentLevelIndex = -1;

    public override void Spawned()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
        LoadLevelSequenceData();
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
        currentLevelIndex = 0;

        if (!TryGetLevelSceneName(currentLevelIndex, out string firstLevelScene))
        {
            firstLevelScene = SceneNames.GameScene;
            currentLevelIndex = -1;
        }

        LoadGameplayScene(firstLevelScene);
    }

    public void GoToMenu()
    {
        CurrentState = GameState.MainMenu;
        Time.timeScale = 1f;
        SceneLoader.Instance.LoadScene();
        if (InputManager.Instance != null)
        {
            InputManager.Instance.EnableUIInput();
        }
        Debug.Log("Got to Main Menu");
    }

    public void RestartGameScene()
    {
        string sceneToReload = ResolveCurrentGameplayScene();
        LoadGameplayScene(sceneToReload);
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

    public bool TryLoadNextLevel()
    {
        UpdateCurrentLevelIndexFromActiveScene();

        int nextLevelIndex = currentLevelIndex + 1;
        if (!TryGetLevelSceneName(nextLevelIndex, out string nextScene))
            return false;

        currentLevelIndex = nextLevelIndex;
        LoadGameplayScene(nextScene);
        return true;
    }
    /// <summary>
    /// Загружает sequence asset из Resources.
    /// Если не найден, система всё равно работает в fallback режиме через GameScene.
    /// </summary>
    private void LoadLevelSequenceData()
    {
        if (levelSequenceOverride != null)
        {
            levelSequenceData = levelSequenceOverride;
            return;
        }

#if UNITY_EDITOR
        // В Editor приоритет у учебного ассета в _ScriptableObjects,
        // чтобы брались именно те данные, которые вы редактируете вручную.
        if (TryLoadLevelSequenceFromEditorAssetPath())
            return;
#endif

        if (string.IsNullOrWhiteSpace(levelSequenceResourcePath))
        {
#if UNITY_EDITOR
            TryLoadLevelSequenceFromEditorAssetPath();
#endif
            return;
        }

        levelSequenceData = Resources.Load<LevelSequenceData>(levelSequenceResourcePath);

#if UNITY_EDITOR
        if (levelSequenceData == null)
            TryLoadLevelSequenceFromEditorAssetPath();
#endif

        if (levelSequenceData == null)
        {
            Debug.LogWarning(
                $"GameManager: LevelSequenceData not found at Resources/{levelSequenceResourcePath}. " +
                "Fallback to SceneNames.GameScene will be used.");
        }
    }

#if UNITY_EDITOR
    private bool TryLoadLevelSequenceFromEditorAssetPath()
    {
        const string editorAssetPath = "Assets/_ScriptableObjects/Scenes/LevelSequenceData.asset";
        levelSequenceData = UnityEditor.AssetDatabase.LoadAssetAtPath<LevelSequenceData>(editorAssetPath);
        return levelSequenceData != null;
    }
#endif

    /// <summary>
    /// Единая точка перехода в игровую сцену:
    /// переводит state в Playing, возвращает timeScale и включает player input.
    /// </summary>
    private void LoadGameplayScene(string sceneName)
    {
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;
        SceneLoader.Instance.LoadScene();

        if (InputManager.Instance != null)
            InputManager.Instance.EnablePlayerInput();
    }

    /// <summary>
    /// Безопасный доступ к имени сцены уровня из sequence.
    /// </summary>
    private bool TryGetLevelSceneName(int levelIndex, out string sceneName)
    {
        sceneName = null;

        if (levelSequenceData == null)
            return false;

        return levelSequenceData.TryGetLevelSceneName(levelIndex, out sceneName);
    }

    /// <summary>
    /// Синхронизирует currentLevelIndex с реально активной сценой.
    /// Нужен перед вычислением "следующего" уровня.
    /// </summary>
    private void UpdateCurrentLevelIndexFromActiveScene()
    {
        if (levelSequenceData == null)
            return;

        string activeSceneName = SceneManager.GetActiveScene().name;
        int sceneIndex = levelSequenceData.FindLevelIndex(activeSceneName);
        if (sceneIndex >= 0)
            currentLevelIndex = sceneIndex;
    }

    /// <summary>
    /// Определяет, какую сцену перезапускать на Restart:
    /// 1) текущую сцену из sequence
    /// 2) если sequence не знает сцену - активную сцену
    /// 3) если активна MainMenu - fallback на GameScene
    /// </summary>
    private string ResolveCurrentGameplayScene()
    {
        UpdateCurrentLevelIndexFromActiveScene();

        if (TryGetLevelSceneName(currentLevelIndex, out string sceneName))
            return sceneName;

        return SceneManager.GetActiveScene().name == SceneNames.MainMenu
            ? SceneNames.GameScene
            : SceneManager.GetActiveScene().name;
    }
}