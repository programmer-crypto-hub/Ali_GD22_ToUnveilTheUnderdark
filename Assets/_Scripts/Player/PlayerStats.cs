using System;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Отвечает за текущие характеристики игрока:
/// здоровье, мана/энергия и связанные с ними события.
/// Хранит ТЕКУЩИЕ значения в рантайме и даёт методы для урона и лечения.
/// </summary>
public class PlayerStats : MonoBehaviour
{
    [Header("Данные игрока")]
    [Tooltip("ScriptableObject с базовыми параметрами игрока (PlayerData).")]
    public PlayerData playerData;

    [Header("Текущее состояние")]
    [SerializeField]
    [Tooltip("Текущее здоровье игрока.")]
    private float currentHealth;

    private string currentRole;
    private int currentRoleId;

    [Header("Currencies")]
    [SerializeField]
    [Tooltip("Текущее количество монет (Cave Coins).")]
    private float caveCoins;

    [Header("Movement")]
    [SerializeField]
    [Tooltip("Rolled Die Amount")]
    private float currentDiceValue;

    [Header("Combat")]
    [SerializeField]
    [Tooltip("Current Combat Value, calculated from the current dice value and applied multipliers.")]
    private float diceValue;
    /// <summary>
    /// Текущее здоровье игрока (только для чтения).
    /// Для изменения используйте методы TakeDamage() или Heal().
    /// </summary>
    public float CurrentHealth => currentHealth;

    /// <summary>
    /// Currency Values
    /// For Modifying use AddCaveCoins() or SpendCaveCoins() methods.
    /// </summary>
    public float CaveCoins => caveCoins;

    public int CurrentRoleId => currentRoleId;
    public string CurrentRole => currentRole;

    // События для связи с другими системами (UI, эффекты и т.п.)
    /// <summary>
    /// Вызывается при изменении здоровья.
    /// Параметры: текущее здоровье, максимальное здоровье.
    /// </summary>
    public event Action<float, float> OnHealthChanged;
    public event Action<float, float> OnCaveCoinsChanged;
    public event Action<float, float> OnDiceRolled;
    public event Action<string, int> OnRoleApplied;

    /// <summary>
    /// Вызывается один раз в момент "смерти" игрока (здоровье упало до 0).
    /// </summary>
    public event Action OnDeath;

    /// <summary>
    /// Точка входа компонента.
    /// При старте берёт стартовые значения из PlayerData.
    /// </summary>
    private void Awake()
    {
        InitializeFromData();
        PlayerRolesController.Instance.OnRoleGiven += () =>
        {
            PlayerRolesController.Instance.ApplyRole();
            currentRole = PlayerRolesController.Instance.roleName.ToString();
            currentRoleId = PlayerRolesController.Instance.RoleId;
            //currentRole = ((PlayerRolesController.Instance.RoleId >= 0) ? ((PlayerRoles.RoleType)currentRoleId).ToString() : "No Role");
            OnRoleApplied?.Invoke(currentRole, currentRoleId);
        };
    }

    /// <summary>
    /// Инициализирует текущие значения из PlayerData.
    /// Можно вызвать повторно, например, при респауне.
    /// </summary>
    public void InitializeFromData()
    {
        if (playerData == null)
        {
            Debug.LogError("PlayerStats: PlayerData не назначен!", this);
            return;
        }

        // Берём стартовые значения и ограничиваем их в разумных пределах.
        currentHealth = Mathf.Clamp(playerData.maxHealth, 1f, float.MaxValue);
        caveCoins = Mathf.Clamp(playerData.caveCoins, 0f, float.MaxValue);
        currentDiceValue = 0f;

        // Уведомляем подписчиков о начальных значениях.
        OnHealthChanged?.Invoke(currentHealth, playerData.maxHealth);
        OnCaveCoinsChanged?.Invoke(CaveCoins, playerData.maxCaveCoins);
        OnDiceRolled?.Invoke(currentDiceValue, playerData.maxDiceValue);

    }

    /// <summary>
    /// Применяет бонусы за повышение уровня:
    /// меняет maxHealth / maxMana и обновляет текущие значения
    /// с подниманием событий OnHealthChanged / OnManaChanged.
    /// Вызывать этот метод предпочтительнее, чем напрямую
    /// менять currentHealth и ScriptableObject снаружи.
    /// </summary>
    public void ApplyLevelUpBonuses(float healthBonus, float caveCoinsBonus)
    {
        if (playerData == null)
        {
            Debug.LogWarning("PlayerStats.ApplyLevelUpBonuses: PlayerData не назначен.", this);
            return;
        }

        // Увеличиваем максимальные значения
        playerData.maxHealth += healthBonus;
        playerData.maxCaveCoins += caveCoinsBonus;

        // Синхронизируем текущее с новыми максимумами
        currentHealth = playerData.maxHealth;

        // События вызываем здесь, внутри PlayerStats
        OnHealthChanged?.Invoke(currentHealth, playerData.maxHealth);
    }

    /// <summary>
    /// Наносит урон игроку.
    /// Не даёт опустить здоровье ниже 0 и при необходимости вызывает OnDeath.
    /// </summary>
    public void TakeDamage(float amount)
    {
        if (playerData == null)
        {
            Debug.LogWarning("PlayerStats.TakeDamage: PlayerData не назначен.", this);
            return;
        }

        // Не реагируем на некорректный урон или если игрок уже мёртв.
        if (amount <= 0f || currentHealth <= 0f)
            return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, playerData.maxHealth);

        OnHealthChanged?.Invoke(currentHealth, playerData.maxHealth);

        if (currentHealth <= 0f)
        {
            // Игрок "умирает" — здесь можно запустить анимацию смерти, перезапуск уровня и т.п.
            OnDeath?.Invoke();
        }
    }

    /// <summary>
    /// Лечит игрока на указанное значение.
    /// Не поднимает здоровье выше максимального и не лечит мёртвого игрока.
    /// </summary>
    public void Heal(float amount)
    {
        if (playerData == null)
        {
            Debug.LogWarning("PlayerStats.Heal: PlayerData не назначен.", this);
            return;
        }

        // Нет смысла лечить на неположительное значение или лечить мёртвого.
        if (amount <= 0f || currentHealth <= 0f)
            return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, playerData.maxHealth);

        OnHealthChanged?.Invoke(currentHealth, playerData.maxHealth);
    }

    /// <summary>
    /// Adds cave coins to the player's current amount.
    /// Doesnt add coins if the player already has the maximum amount.
    /// </summary>
    
    public void AddCaveCoins(float amount)
    {
        if (playerData == null)
        {
            Debug.LogWarning("PlayerStats.AddCaveCoins: PlayerData не назначен.", this);
            return;
        }
        if (amount <= 0f || caveCoins >= playerData.maxCaveCoins)
            return;
        caveCoins += amount;
        caveCoins = Mathf.Clamp(caveCoins, 0f, playerData.maxCaveCoins);
        OnCaveCoinsChanged?.Invoke(caveCoins, playerData.maxCaveCoins);
    }

    /// <summary>
    /// Control Dice Rolling for Player Stats 
    /// </summary>
    
    public void RollDice()
    {
        if (playerData == null)
        {
            Debug.LogWarning("PlayerStats.RollDice: PlayerData не назначен.", this);
            return;
        }
        currentDiceValue = UnityEngine.Random.Range(1f, playerData.maxDiceValue + 1f);
        OnDiceRolled?.Invoke(currentDiceValue, playerData.maxDiceValue);
    }

    /// <summary>
    /// Apply Multipliers to the current dice value, for example, from buffs or debuffs.
    /// Will not change the current dice value if the multiplier is more than or equal to 10.
    /// </summary>
    
    public void ApplyDiceMultiplier(float multiplier)
    {
        if (playerData == null)
        {
            Debug.LogWarning("PlayerStats.ApplyDiceMultiplier: PlayerData не назначен.", this);
            return;
        }
        if (multiplier <= 0f || multiplier >= 10f)
            return;
        currentDiceValue *= multiplier;
        currentDiceValue = Mathf.Clamp(currentDiceValue, 1f, playerData.maxDiceValue);
        OnDiceRolled?.Invoke(currentDiceValue, playerData.maxDiceValue);
    }
}
