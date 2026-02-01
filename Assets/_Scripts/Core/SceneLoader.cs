using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
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

    private IEnumerator LoadSceneAsyncCoroutine(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        while (!operation.isDone)
        {
            //TODO: Add loading screen progress bar update
            Debug.Log($"Loading progress: {operation.progress}");
            yield return null;
        }

        Debug.Log($"Loaded Scene Asynchronously: {sceneName}");
    }
}
