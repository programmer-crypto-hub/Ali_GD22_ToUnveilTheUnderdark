using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button buttonNewGame;
    [SerializeField] private Button buttonQuitGame;
    [SerializeField] private Button buttonSettings;
    [SerializeField] private SettingsPanelController settingsPanelController;

    private void Awake()
    {
        if (buttonQuitGame != null)
            buttonQuitGame.onClick.AddListener(() => Application.Quit());
        if (buttonSettings != null)
            buttonSettings.onClick.AddListener(HandleSettingsClicked);
    }
    private void OnDisable()
    {
        if (buttonQuitGame != null)
            buttonQuitGame.onClick.RemoveAllListeners();
        if (buttonSettings != null)
            buttonSettings.onClick.RemoveAllListeners();
    }
    private void ValidateReferences()
    {
        if (buttonNewGame == null)
            Debug.LogError($"{name}: buttonNewGame не назначен.", this);

        if (buttonQuitGame == null)
            Debug.LogError($"{name}: buttonQuitGame не назначен.", this);

        if (buttonSettings == null)
            Debug.LogWarning($"{name}: buttonSettings не назначен.", this);

        if (settingsPanelController == null)
            Debug.LogError($"{name}: settingsPanelController не назначен в Inspector.", this);
    }
    private void HandleSettingsClicked()
    {
        if (settingsPanelController == null)
        {
            Debug.LogError($"{name}: settingsPanelController отсутствует. Ќазначьте ссылку в Inspector.", this);
            return;
        }

        settingsPanelController.OpenPanel();
    }
} 
