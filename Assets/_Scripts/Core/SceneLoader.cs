using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }
    [SerializeField] private GameObject loadingScreen;

    private void Awake()
    {
        Debug.Log("Scene Loader script active");
        LoadScene();
    }

    public async void LoadScene()
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

    private IEnumerator LoadingScreenCoroutine(AsyncOperation op)
    {
        yield return new WaitForSeconds(3f); // Optional delay for better UX
    }
}
