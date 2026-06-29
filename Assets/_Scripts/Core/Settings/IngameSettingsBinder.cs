using UnityEngine;
using UnityEngine.UI;
using Fusion;

[DisallowMultipleComponent]
public class IngameSettingsBinder : NetworkBehaviour
{
    public static IngameSettingsBinder Instance { get; private set; }
    [Header("Settings")]
    [SerializeField] private Button openSettingsButton;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Slider soundSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Toggle fullscreenToggle;

    [Header("Ui States")]
    [Networked] private float networkedSound { get; set; }
    [Networked] private float networkedMusic { get; set; }
    [Networked] private bool networkedFullscreen { get; set; }

    [SerializeField] private AudioSource[] soundSources;
    [SerializeField] private AudioSource[] musicSources;

    private bool suppressCallbacks;
    private bool duplicateWarningLogged;

    public override void Spawned()
    {
        if (soundSlider == null || musicSlider == null || fullscreenToggle == null)
        {
            Debug.LogWarning($"{name}: Some settings are not assigned. Using fallback references.", this);
            soundSlider = FindFirstObjectByType<Slider>();
            musicSlider = FindFirstObjectByType<Slider>();
            fullscreenToggle = FindFirstObjectByType<Toggle>();
            ResolveReferencesIfMissing();
        }
    }

    private void OnEnable()
    {
        WarnIfDuplicateBinders();
        SyncUiFromSavedSettings();
        GameSettings.Apply(GameSettings.Load(), soundSources, musicSources);
        BindUiHandlers();
    }

    private void OnDisable()
    {
        UnbindUiHandlers();
    }

    private void BindUiHandlers()
    {
        if (openSettingsButton != null)
            openSettingsButton.onClick.AddListener(OpenSettingsMenu);
        if (soundSlider != null)
            soundSlider.onValueChanged.AddListener(HandleSoundChanged);

        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(HandleMusicChanged);

        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(HandleFullscreenChanged);
    }

    private void UnbindUiHandlers()
    {
        if (soundSlider != null)
            soundSlider.onValueChanged.RemoveListener(HandleSoundChanged);

        if (musicSlider != null)
            musicSlider.onValueChanged.RemoveListener(HandleMusicChanged);

        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.RemoveListener(HandleFullscreenChanged);
    }

    public void OpenSettingsMenu()
    {
        settingsPanel.SetActive(true);
    }

    private void SyncUiFromSavedSettings()
    {
        GameSettings.Data data = GameSettings.Load();
        suppressCallbacks = true;

        if (soundSlider != null)
            soundSlider.SetValueWithoutNotify(data.Sound);

        if (musicSlider != null)
            musicSlider.SetValueWithoutNotify(data.Music);

        if (fullscreenToggle != null)
            fullscreenToggle.SetIsOnWithoutNotify(data.Fullscreen);

        suppressCallbacks = false;
    }

    private void HandleSoundChanged(float value)
    {
        if (suppressCallbacks)
            return;

        GameSettings.SetSound(value, soundSources);
    }

    private void HandleMusicChanged(float value)
    {
        if (suppressCallbacks)
            return;

        GameSettings.SetMusic(value, musicSources);
    }

    private void HandleFullscreenChanged(bool value)
    {
        if (suppressCallbacks)
            return;

        GameSettings.SetFullscreen(value);
    }

    private void ResolveReferencesIfMissing()
    {
        if (soundSlider != null && musicSlider != null && fullscreenToggle != null)
            return;

        Debug.LogWarning($"{name}: Some settings are not assigned. Using fallback references.", this);

        if (soundSlider == null || musicSlider == null)
        {
            Slider[] sliders = GetComponentsInChildren<Slider>(true);
            if (soundSlider == null && sliders.Length > 0)
                soundSlider = sliders[0];
            if (musicSlider == null && sliders.Length > 1)
                musicSlider = sliders[1];
        }

        if (fullscreenToggle == null)
            fullscreenToggle = GetComponentInChildren<Toggle>(true);

        if (soundSlider == null || musicSlider == null || fullscreenToggle == null)
            Debug.LogError($"{name}: IngameSettingsBinder couldn't resolve all references. Please assign sound/music/fullscreen in the Inspector.", this);
    }

    private void WarnIfDuplicateBinders()
    {
        if (duplicateWarningLogged)
            return;

        IngameSettingsBinder[] binders = FindObjectsByType<IngameSettingsBinder>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (binders.Length > 1)
        {
            duplicateWarningLogged = true;
            Debug.LogWarning($"{name}: Some settings are not assigned. Using fallback references.{binders.Length}: Some settings are not assigned. Using fallback references.", this);
        }
    }
}