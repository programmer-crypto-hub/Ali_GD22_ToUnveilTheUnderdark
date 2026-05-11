using UnityEngine;

/// <summary>
/// Usage: Apply DontDestroyOnLoad (DDoL) to all core scripts
/// to prevent destroying key data on scene load.
/// </summary>
public class RuntimeStateController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private IngameSettingsBinder settingsBinder;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private BasicPlayerSpawner playerSpawner;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private GameplayHUDController gameplayHUDController;
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private RoomEncounterHandler roomEncounterHandler;
    [SerializeField] private CameraTarget cameraTarget;
    [SerializeField] private EventBus eventBus;
    [SerializeField] private GameSession gameSession;
    [SerializeField] private ShopUIManager shopUIManager;
    [SerializeField] private PlayerProgression playerProgression;
    [SerializeField] private PlayerData playerData;

    private void Awake()
    {
        DontDestroyOnLoad(this);
        ApplyDDoLToScripts();
    }
    private void ApplyDDoLToScripts()
    {
        if (settingsBinder != null) DontDestroyOnLoad(settingsBinder);
        if (inputManager != null) DontDestroyOnLoad(inputManager);
        if (playerSpawner != null) DontDestroyOnLoad(playerSpawner);
        if (playerController != null) DontDestroyOnLoad(playerController);
        if (playerStats != null) DontDestroyOnLoad(playerStats);
        if (gameplayHUDController != null) DontDestroyOnLoad(gameplayHUDController);
        if (weaponManager != null) DontDestroyOnLoad(weaponManager);
        if (gameManager != null) DontDestroyOnLoad(gameManager);
        if (roomEncounterHandler != null) DontDestroyOnLoad(roomEncounterHandler);
        if (cameraTarget != null) DontDestroyOnLoad(cameraTarget);
        if (eventBus != null) DontDestroyOnLoad(eventBus);
        if (gameSession != null) DontDestroyOnLoad(gameSession);
        if (shopUIManager != null) DontDestroyOnLoad(shopUIManager);
        if (playerProgression != null) DontDestroyOnLoad(playerProgression);
        if (playerData != null) DontDestroyOnLoad(playerData);
    }
}
