using System.Collections;
using UnityEngine;
using System;
using Fusion;
using UnityEngine.SceneManagement;

public class SceneLoader : NetworkBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [SerializeField, Min(0f)] private float minimumLoadingDuration = 3.2f;

    private string _pendingSceneName;
    private bool _waitForLoadingScene;
    private Func<IEnumerator> _pendingPreloadRoutine;

    public override void Spawned()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void Load(string sceneName)
    {
        Debug.Log($"Loaded Scene: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }

    public void LoadAsync(string sceneName)
    {
        StartCoroutine(LoadSceneAsyncCoroutine(sceneName));
    }

    public void LoadWithLoading(string targetSceneName, Func<IEnumerator> preloadRoutine = null)
    {
        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            Debug.LogError("SceneLoader: target scene name is empty.");
            return;
        }

        if (targetSceneName == SceneNames.Loading)
        {
            Load(targetSceneName);
            return;
        }

        _pendingSceneName = targetSceneName;
        _pendingPreloadRoutine = preloadRoutine;
        _waitForLoadingScene = true;
        Load(SceneNames.Loading);
    }

    private IEnumerator LoadSceneAsyncCoroutine(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;
        float startTime = Time.unscaledTime;
        while (!operation.isDone)
        {
            bool minDurationReached = Time.unscaledTime - startTime >= minimumLoadingDuration;
            bool loadingReady = operation.progress >= 0.9f;

            if (loadingReady && minDurationReached)
            {
                operation.allowSceneActivation = true;
            }
            yield return null;
        }

        Debug.Log($"Loaded Scene Asynchronously: {sceneName}");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!_waitForLoadingScene)
            return;

        if (scene.name != SceneNames.Loading)
            return;

        _waitForLoadingScene = false;

        if (string.IsNullOrWhiteSpace(_pendingSceneName))
        {
            Debug.LogError("SceneLoader: pending scene is empty after Loading scene opened.");
            return;
        }

        StartCoroutine(LoadPendingSceneFlow());
    }

    private IEnumerator LoadPendingSceneFlow()
    {
        // Даем Loading сцене гарантированно отрисоваться хотя бы один кадр.
        yield return null;

        if (_pendingPreloadRoutine != null)
        {
            yield return StartCoroutine(_pendingPreloadRoutine.Invoke());
        }

        string sceneToLoad = _pendingSceneName;
        _pendingSceneName = null;
        _pendingPreloadRoutine = null;

        yield return StartCoroutine(LoadSceneAsyncCoroutine(sceneToLoad));
    }
}