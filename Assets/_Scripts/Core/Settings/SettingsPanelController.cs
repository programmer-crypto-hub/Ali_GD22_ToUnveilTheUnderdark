using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;

public class SettingsPanelController : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject mainMenuPanel;

    [Header("UI-controls")]
    [Tooltip("Player Name input field.")]
    [SerializeField] private TMP_InputField playerName;

    [Tooltip("Game Code input field.")]
    [SerializeField] private TMP_InputField gameCode;

    [Networked] private Toggle fullscreenToggle { get; set; }

    [Tooltip("Кнопка \"Назад\" для закрытия панели.")]
    [SerializeField] private Button backButton;

    [Header("Аудио-источники (опционально)")]
    [Tooltip("Явные источники для канала sound. Рекомендуется назначать вручную; иначе используется резервный путь по loop=false.")]
    [SerializeField] private AudioSource[] soundSources;

    [Tooltip("Явные источники для канала music. Рекомендуется назначать вручную; иначе используется резервный путь по loop=true.")]
    [SerializeField] private AudioSource[] musicSources;

    private bool suppressCallbacks;

    public event Action OnSettingsClosed;

    public bool IsOpen => settingsPanel != null && settingsPanel.activeSelf;

    private void Awake()
    {
        if (settingsPanel == null)
        {
            settingsPanel = gameObject;
            Debug.LogWarning($"{name}: settingsPanel не назначен. Использую текущий объект как резервный путь.", this);
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
        if (settingsPanel == null)
            return;

        settingsPanel.SetActive(false);
        OnSettingsClosed?.Invoke();
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

        Debug.LogWarning($"{name}: ссылки окна настроек назначены не полностью. Выполняю резервный автопоиск.", this);

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
            Debug.LogError($"{name}: не удалось автоматически найти все обязательные контролы (playerName/gameCode). Назначьте ссылки в Inspector.", this);

        if (backButton == null)
            Debug.LogWarning($"{name}: backButton не найден. Панель будет открываться, но закрытие кнопкой \"Назад\" не сработает.", this);
    }
}