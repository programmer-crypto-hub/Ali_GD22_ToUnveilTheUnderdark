using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class GameplayHUDController : NetworkBehaviour
{
    [Header("Data Sources")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerProgression playerProgression;
    [SerializeField] private WeaponManager weaponManager;

    [Header("HP")]
    [SerializeField] private Image hpFillImage;
    [Networked, OnChangedRender(nameof(OnStatsChanged))] 
    private int currentHealth { get; set; }
    private Text hpValueText;

    [Header("Gold")]
    [Networked, OnChangedRender(nameof(OnStatsChanged))] 
    private int gold { get; set; }
    private Text goldValueText;

    [Header("XP + Level")]
    [Networked, OnChangedRender(nameof(OnStatsChanged))] 
    private float xp { get; set; }
    private Image xpFillImage;
    [Networked, OnChangedRender(nameof(OnStatsChanged))] 
    private int currentLevel { get; set; }
    private Text levelValueText;

    [Header("Weapon HUD")]
    [SerializeField] private Image weaponIconImage;

    public override void Spawned()
    {
        if (Runner != null && playerStats != null && playerStats.playerData != null)
        {
            currentHealth = playerStats != null ? Mathf.CeilToInt(playerStats.CurrentHealth) : 0;
            gold = playerStats != null && playerStats.playerData != null ? playerStats.playerData.caveCoins : 0;
            xp = playerProgression != null ? playerProgression.CurrentXP : 0f;
            currentLevel = playerProgression != null ? playerProgression.CurrentLevel : 1;
            HandleHealthChanged(playerStats.CurrentHealth, playerStats.playerData.maxHealth);
            HandleGoldChanged(playerStats.playerData.caveCoins, playerStats.playerData.maxCaveCoins);
        }
        ResolveSourcesIfNeeded();
    }

    private void OnEnable()
    {
        Bind();
        RefreshAll();
    }

    private void OnDisable()
    {
        Unbind();
    }

    private void OnStatsChanged()
    {
        RefreshAll();
        playerStats.OnStatsChanged();
    }   
    private void ResolveSourcesIfNeeded()
    {
        if (playerStats == null)
            playerStats = FindFirstObjectByType<PlayerStats>();

        if (playerProgression == null && playerStats != null)
            playerProgression = playerStats.GetComponent<PlayerProgression>();

        if (playerProgression == null)
            playerProgression = FindFirstObjectByType<PlayerProgression>();

        if (weaponManager == null && playerStats != null)
            weaponManager = playerStats.GetComponent<WeaponManager>();

        if (weaponManager == null)
            weaponManager = FindFirstObjectByType<WeaponManager>();
    }

    private void Bind()
    {
        ResolveSourcesIfNeeded();

        if (playerStats != null)
        {
            playerStats.OnHealthChanged += HandleHealthChanged;
            playerStats.OnGoldChanged += HandleGoldChanged;
        }

        if (playerProgression != null)
        {
            playerProgression.OnXPChanged += HandleExperienceChanged;
            playerProgression.OnLevelUp += HandleLevelUp;
        }
        if (weaponManager != null)
            weaponManager.OnWeaponChanged += HandleWeaponChanged;
    }

    private void Unbind()
    {
        if (playerStats != null)
        {
            playerStats.OnHealthChanged -= HandleHealthChanged;
            playerStats.OnGoldChanged -= HandleGoldChanged;
        }

        if (playerProgression != null)
        {
            playerProgression.OnXPChanged -= HandleExperienceChanged;
            playerProgression.OnLevelUp -= HandleLevelUp;
        }

        if (weaponManager != null)
            weaponManager.OnWeaponChanged -= HandleWeaponChanged;
    }

    private void RefreshAll()
    {
        if (playerProgression != null)
        {
            HandleLevelUp(playerProgression.CurrentLevel);
            float requiredXp = playerProgression.baseXPToNextLevel;
            HandleExperienceChanged(playerProgression.CurrentXP, requiredXp);
            Spawned(); // Refresh gold and health as well
        }

        if (weaponManager != null)
            HandleWeaponChanged(weaponManager.CurrentWeapon);
    }

    private void HandleHealthChanged(float current, float max)
    {
        if (hpFillImage != null)
            hpFillImage.fillAmount = max > 0.01f ? Mathf.Clamp01(current / max) : 0f;

        if (hpValueText != null)
            hpValueText.text = Mathf.CeilToInt(current).ToString();
    }

    private void HandleExperienceChanged(float current, float required)
    {
        if (xpFillImage != null)
            xpFillImage.fillAmount = required > 0.01f ? Mathf.Clamp01(current / required) : 0f;
    }

    private void HandleGoldChanged(int gold, int maxGold)
    {
        if (gold > maxGold)
        {
            gold = maxGold;
            return;
        }
        if (goldValueText != null)
            goldValueText.text = gold.ToString();
    }

    private void HandleLevelUp(int level)
    {
        if (levelValueText != null)
            levelValueText.text = level.ToString();
    }

    private void HandleWeaponChanged(WeaponBase weapon)
    {
        if (weaponIconImage == null)
            return;

        Sprite icon = weapon != null && weapon.weaponData != null
            ? weapon.weaponData.icon
            : null;
        weaponIconImage.enabled = icon != null;
        weaponIconImage.sprite = icon;
    }
}