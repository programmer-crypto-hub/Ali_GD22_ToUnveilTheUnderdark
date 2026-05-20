using Fusion;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Unity.Collections.Unicode;

public class SceneLoader : NetworkBehaviour
{
    public static SceneLoader Instance { get; private set; }
    [SerializeField] private GameObject loadingScreen;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("Scene Loader script active");
    }

    public async void LoadScene()
    {
        if (Runner == null || !Runner.IsRunning)
        {
            Debug.LogError("Runner isn't running yet! Can't load scene - SceneLoader");
        }
        else
        {
            Debug.Log("LoadScene method active");
            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                // Start the load
                Debug.Log("Scene started loading");
                var op = SceneManager.LoadSceneAsync(nextSceneIndex);
                StartCoroutine(LoadingScreenCoroutine(op));

                // Wait for it to finish
                while (!op.isDone)
                {
                    Debug.Log("Loading hasn't finished yet");
                    await Task.Yield();
                }
            }
            else
            {
                Debug.LogError("No more scenes in Build Settings!");
                if (loadingScreen != null) loadingScreen.SetActive(false);
            }
        }
    }

    private IEnumerator LoadingScreenCoroutine(AsyncOperation op)
    {
        yield return new WaitForSeconds(3f); // Optional delay for better UX
    }
}
