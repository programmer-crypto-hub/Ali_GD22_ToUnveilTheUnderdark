using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;

public class SettingsPanelController : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject mainMenuPanel;

    [SerializeField] private TMP_InputField playerName;
    [SerializeField] private TMP_InputField gameCode;

    [Networked] private Toggle fullscreenToggle { get; set; }

    [SerializeField] private Button backButton;
    [SerializeField] private AudioSource[] soundSources;
    [SerializeField] private AudioSource[] musicSources;

    private bool suppressCallbacks;

    public bool IsOpen => settingsPanel != null && settingsPanel.activeSelf;

    private void Awake()
    {
        if (settingsPanel == null)
        {
            settingsPanel = gameObject;
            Debug.LogWarning($"{name}: settingsPanel isn't assigned.", this);
        }

        ResolveReferencesIfMissing();
    }

    private void OnEnable()
    {
        SyncUiFromSavedSettings();
        GameSettings.Apply(GameSettings.Load(), soundSources, musicSources);
        BindUiHandlers();
    }
    private void OnDisable()
    {
        UnbindUiHandlers();
    }

    public void OpenPanel()
    {
        if (settingsPanel == null)
            return;

        SyncUiFromSavedSettings();
        settingsPanel.SetActive(true);
    }

    public void ClosePanel()
    {
        if (settingsPanel == null || GameManager.Events == null)
            return;

        settingsPanel.SetActive(false);
        GameManager.Instance.RaiseEvent(GameManager.Events.OnSettingsClosed);
        mainMenuPanel.SetActive(false);
    }

    private void BindUiHandlers()
    {
        if (playerName != null)
            playerName.onValueChanged.AddListener(HandlePlayerNameChanged);

        if (gameCode != null)
            gameCode.onValueChanged.AddListener(HandleGameCodeChanged);

        if (backButton != null)
            backButton.onClick.AddListener(ClosePanel);
        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(HandleFullscreenToggleChanged);
    }

    private void UnbindUiHandlers()
    {
        if (playerName != null)
            playerName.onValueChanged.RemoveListener(HandlePlayerNameChanged);
        if (gameCode != null)
            gameCode.onValueChanged.RemoveListener(HandleGameCodeChanged);

        if (backButton != null)
            backButton.onClick.RemoveListener(ClosePanel);
        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.RemoveListener(HandleFullscreenToggleChanged);
    }

    private void SyncUiFromSavedSettings()
    {
        GameSettings.Data data = GameSettings.Load();
        suppressCallbacks = true;

        if (playerName != null)
            playerName.text = data.PlayerName;

        if (gameCode != null)
            gameCode.text = data.GameCode.ToString();

        if (fullscreenToggle != null)
            fullscreenToggle.SetIsOnWithoutNotify(data.Fullscreen);

        suppressCallbacks = false;
    }

    private void HandlePlayerNameChanged(string value)
    {
        if (suppressCallbacks)
            return;

        GameSettings.SetPlayerName(value);
    }

    private void HandleGameCodeChanged(string value)
    {
        if (suppressCallbacks)
            return;

        GameSettings.SetGameCode(int.Parse(value));
    }

    private void HandleFullscreenToggleChanged(bool isFullscreen)
    {
        if (suppressCallbacks)
            return;

        GameSettings.SetFullscreen(isFullscreen);
    }

    private void ResolveReferencesIfMissing()
    {
        if (settingsPanel == null)
            return;

        if (playerName != null && gameCode != null)
            return;

        Debug.LogWarning($"{name}: ссылки окна настроек назначены не полностью. ¬ыполн€ю резервный автопоиск.", this);

        if (playerName == null || gameCode == null)
        {
            TMP_InputField[] inputFields = settingsPanel.GetComponentsInChildren<TMP_InputField>(true);
            if (playerName == null && inputFields.Length > 0)
                playerName = inputFields[0];
            if (gameCode == null && inputFields.Length > 1)
                gameCode = inputFields[1];
        }

        if (fullscreenToggle == null)
            fullscreenToggle = settingsPanel.GetComponentInChildren<Toggle>(true);

        if (backButton == null)
        {
            Button[] buttons = settingsPanel.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] == null)
                    continue;

                string buttonName = buttons[i].name.ToLowerInvariant();
                if (buttonName.Contains("back") || buttonName.Contains("назад"))
                {
                    backButton = buttons[i];
                    break;
                }
            }
        }

        if (playerName == null || gameCode == null)
            Debug.LogError($"{name}: не удалось автоматически найти все об€зательные контролы (playerName/gameCode). Ќазначьте ссылки в Inspector.", this);

        if (backButton == null)
            Debug.LogWarning($"{name}: backButton не найден. ѕанель будет открыватьс€, но закрытие кнопкой \"Ќазад\" не сработает.", this);
    }
}