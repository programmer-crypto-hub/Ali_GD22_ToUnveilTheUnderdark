using System;
using UnityEngine;
using UnityEngine.UI;

/*
 * SettingsPanelController
 * Назначение: контроллер отдельного окна настроек (MainMenu-сценарий: Open/Close/Back).
 * Роль в игре: даёт игроку доступ к sound/music/fullscreen в формате отдельной панели.
 * Связи: объект панели, UI-контролы, GameSettings, опционально явные массивы AudioSource.
 * Как используется: вешается на объект окна настроек, все ссылки задаются в Inspector.
 * Идеи расширения:
 * - Добавить кнопку "Сброс по умолчанию".
 * - Добавить режим "Apply/Cancel" для отложенного применения.
 * - Добавить локализацию подписей внутри окна.
 * Практические советы:
 * - Канонический путь: ссылки назначены вручную; автопоиск — только запасной сценарий.
 * - Если Back не работает, сначала проверьте ссылку backButton и active state settingsPanel.
 */
public class SettingsPanelController : MonoBehaviour
{
    [Header("Окно настроек")]
    [Tooltip("Корневой объект отдельной панели настроек. Если не задан, используется текущий объект как резервный путь.")]
    [SerializeField] private GameObject settingsPanel;

    [Header("UI-controls")]
    [Tooltip("Player Name input field.")]
    [SerializeField] private InputField playerName;

    [Tooltip("Game Code input field.")]
    [SerializeField] private InputField gameCode;

    [SerializeField] private Toggle fullscreenToggle;

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

    /// <summary>
    /// Контракт: при активации окна синхронизирует UI из сохранённых значений,
    /// применяет значения в текущую сцену и только потом подписывает обработчики.
    /// Почему так: предотвращаем рекурсивные callback'и и гарантируем актуальные значения в UI.
    /// Как дебажить: если при открытии окна слайдеры показывают не то, проверьте PlayerPrefs и suppressCallbacks.
    /// </summary>
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
    }

    private void BindUiHandlers()
    {
        if (playerName != null)
            playerName.onValueChanged.AddListener(HandlePlayerNameChanged);

        if (gameCode != null)
            gameCode.onValueChanged.AddListener(HandleGameCodeChanged);

        if (backButton != null)
            backButton.onClick.AddListener(ClosePanel);
    }

    private void UnbindUiHandlers()
    {
        if (playerName != null)
            playerName.onValueChanged.RemoveListener(HandlePlayerNameChanged);

        if (gameCode != null)
            gameCode.onValueChanged.RemoveListener(HandleGameCodeChanged);

        if (backButton != null)
            backButton.onClick.RemoveListener(ClosePanel);
    }

    /// <summary>
    /// Контракт: выставляет значения UI без вызова слушателей и без повторного сохранения в PlayerPrefs.
    /// Почему так: это исключает зацикливание "загрузка -> callback -> сохранение".
    /// Как дебажить: если callback всё равно стреляет, проверьте, что используются SetValueWithoutNotify/SetIsOnWithoutNotify.
    /// </summary>
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

    /// <summary>
    /// Резервный путь: восстанавливает ссылки, если их забыли назначить в Inspector.
    /// В учебном каноне это не основной путь, а страховка от падения сцены.
    /// </summary>
    private void ResolveReferencesIfMissing()
    {
        if (settingsPanel == null)
            return;

        if (playerName != null && gameCode != null)
            return;

        Debug.LogWarning($"{name}: ссылки окна настроек назначены не полностью. Выполняю резервный автопоиск.", this);

        if (playerName == null || gameCode == null)
        {
            InputField[] inputFields = settingsPanel.GetComponentsInChildren<InputField>(true);
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