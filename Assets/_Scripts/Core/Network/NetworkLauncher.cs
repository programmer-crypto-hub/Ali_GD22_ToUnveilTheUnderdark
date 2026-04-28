using UnityEngine;

public class NetworkLauncher : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("Bootstrap complete. Services initialized. Moving to Menu...");
        // Use standard Unity loading here because Fusion hasn't started yet
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }
}