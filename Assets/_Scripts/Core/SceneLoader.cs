using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private GameObject loadingScreen; // Your placeholder UI panel
    public static SceneLoader Instance { get; private set; }

    private void Awake()
    {
        Debug.Log("Scene Loader script active");
        Instance = this;
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
