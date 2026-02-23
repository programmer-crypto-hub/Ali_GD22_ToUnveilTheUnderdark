using System;
using UnityEngine;

/// <summary>
/// Отвечает за текущие характеристики врага:
/// здоровье, мана/энергия и связанные с ними события.
/// Хранит ТЕКУЩИЕ значения в рантайме и даёт методы для урона и лечения.
/// </summary>
public class EnemyStats : MonoBehaviour
{
    [Header("Данные врага")]
    [Tooltip("ScriptableObject с базовыми параметрами врага (enemyData).")]
    public EnemyData enemyData;

    [Header("Текущее состояние")]
    [SerializeField]
    [Tooltip("Текущее здоровье врага.")]
    private float currentEnemyHealth;

    [Header("Урон врагов")]
    [SerializeField]
    [Tooltip("Текущий урон врага.")]
    private float enemyDamage;

    /// <summary>
    /// Текущее здоровье врага (только для чтения).
    /// Для изменения используйте методы TakeDamage() или Heal().
    /// </summary>
    public float CurrentEnemyHealth => currentEnemyHealth;

    /// <summary>
    /// Damage Values
    /// For Modifying use IncreaseDamage() method.
    /// </summary>
    public float EnemyDamage => enemyDamage;

    // События для связи с другими системами (UI, эффекты и т.п.)
    /// <summary>
    /// Вызывается при изменении здоровья.
    /// Параметры: текущее здоровье, максимальное здоровье.
    /// </summary>
    public event Action<float, float> OnEnemyHealthChanged;
    //public event Action<float, float> OnEnemyDamageChanged;

    /// <summary>
    /// Вызывается один раз в момент "смерти" врага (здоровье упало до 0).
    /// </summary>
    public event Action OnEnemyDeath;

    /// <summary>
    /// Точка входа компонента.
    /// При старте берёт стартовые значения из enemyData.
    /// </summary>
    private void Awake()
    {
        InitializeFromEnemyData();
    }

    /// <summary>
    /// Инициализирует текущие значения из enemyData на основе тега врага.
    /// Можно вызвать повторно, например, при респауне.
    /// </summary>
    public void InitializeFromEnemyData()
    {
        if (enemyData == null)
        {
            Debug.LogError("EnemyStats: enemyData не назначен!", this);
            return;
        }

        string enemyTag = gameObject.tag;

        if (enemyTag == enemyData.enemyType)
        {
            currentEnemyHealth = Mathf.Clamp(currentEnemyHealth, 1f, enemyData.skeletonMaxHealth);

        }
        else if (enemyTag == enemyData.enemyType2)
        {
            currentEnemyHealth = Mathf.Clamp(currentEnemyHealth, 1f, enemyData.goblinMaxHealth);
        }
        else if (enemyTag == enemyData.enemyType3)
        {
            currentEnemyHealth = Mathf.Clamp(currentEnemyHealth, 1f, enemyData.vampireMaxHealth);
        }
        else
        {
            Debug.LogWarning("EnemyStats.InitializeFromEnemyData: Неизвестный тип врага, используем дефолтные значения.", this);
            return;
        }

        // Берём стартовые значения и ограничиваем их в разумных пределах.
        //enemyDamage = Mathf.Clamp(enemyDamage, 0f, float.MaxValue);

        // Уведомляем подписчиков о начальных значениях.
        OnEnemyHealthChanged?.Invoke(currentEnemyHealth, currentEnemyHealth);
        //OnEnemyDamageChanged?.Invoke(enemyDamage, enemyDamage);
    }

    /// <summary>
    /// Применяет бонусы за повышение уровня:
    /// меняет здоровье и урон с вызовом событий.
    /// </summary>
    public void ApplyEnemyBuffs(float healthBonus, float damageBonus)
    {
        if (enemyData == null)
        {
            Debug.LogWarning("EnemyStats.ApplyEnemyBuffs: enemyData не назначен.", this);
            return;
        }

        // Увеличиваем текущие значения
        currentEnemyHealth += healthBonus;
        enemyDamage += damageBonus;

        // Ограничиваем значения
        // TODO: Fix the lines below
        currentEnemyHealth = Mathf.Clamp(currentEnemyHealth, 1f, float.MaxValue);
        enemyDamage = Mathf.Clamp(enemyDamage, 0f, enemyDamage);

        // События вызываем здесь, внутри EnemyStats
        OnEnemyHealthChanged?.Invoke(currentEnemyHealth, currentEnemyHealth);
        //OnEnemyDamageChanged?.Invoke(enemyDamage, enemyDamage);
    }

    /// <summary>
    /// Наносит урон врагу.
    /// Не даёт опустить здоровье ниже 0 и при необходимости вызывает OnDeath.
    /// </summary>
    public void TakeDamage(float amount)
    {
        if (enemyData == null)
        {
            Debug.LogWarning("EnemyStats.TakeDamage: enemyData не назначен.", this);
            return;
        }

        // Не реагируем на некорректный урон или если враг уже мёртв.
        if (amount <= 0f || currentEnemyHealth <= 0f)
            return;

        currentEnemyHealth -= amount;
        currentEnemyHealth = Mathf.Clamp(currentEnemyHealth, 0f, float.MaxValue);

        OnEnemyHealthChanged?.Invoke(currentEnemyHealth, currentEnemyHealth);

        if (currentEnemyHealth <= 0f)
        {
            // Враг "умирает" — здесь можно запустить анимацию смерти, эффекты и т.п.
            OnEnemyDeath?.Invoke();
        }
    }

    /// <summary>
    /// Лечит врага на указанное значение.
    /// Не поднимает здоровье выше максимального и не лечит мёртвого врага.
    /// </summary>
    public void Heal(float amount)
    {
        if (enemyData == null)
        {
            Debug.LogWarning("EnemyStats.Heal: enemyData не назначен.", this);
            return;
        }

        // Нет смысла лечить на неположительное значение или лечить мёртвого.
        if (amount <= 0f || currentEnemyHealth <= 0f)
            return;

        currentEnemyHealth += amount;
        currentEnemyHealth = Mathf.Clamp(currentEnemyHealth, 0f, float.MaxValue);

        OnEnemyHealthChanged?.Invoke(currentEnemyHealth, currentEnemyHealth);
    }
}
