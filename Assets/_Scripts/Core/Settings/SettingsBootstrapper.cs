using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class SettingsBootstrapper : MonoBehaviour
{
    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void Awake()
    {
        ApplyCurrentSettings();
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyCurrentSettings();
    }

    private static void ApplyCurrentSettings()
    {
        GameSettings.Apply(GameSettings.Load());
    }
}