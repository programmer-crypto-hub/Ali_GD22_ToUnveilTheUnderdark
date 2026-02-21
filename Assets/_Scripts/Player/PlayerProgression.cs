using System;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Отвечает за прогрессию игрока:
/// уровень, опыт и повышение уровня.
/// </summary>
public class PlayerProgression : MonoBehaviour
{
    [Header("Связи")]
    [Tooltip("Ссылка на PlayerStats для возможного усиления характеристик при уровне.")]
    private PlayerStats playerStats;

    [Header("Уровень")]
    [SerializeField]
    [Tooltip("Текущий уровень игрока.")]
    private int currentLevel = 1;

    [Header("Опыт")]
    [SerializeField]
    [Tooltip("Текущее количество опыта.")]
    private float currentXP = 0f;

    [Header("Прибавка к статам пр иповышении уровня")]
    [SerializeField]
    [Tooltip("Увеличение максимального здоровья при каждом уровне.")]
    private float healthIncreasePerLevel = 10f;

    [SerializeField]
    [Tooltip("Увеличение количества монет (Cave Coins) при каждом уровне.")]
    private float caveCoinIncreasePerLevel = 5f;

    [Header("Максимальный уровень ХП")]
    [SerializeField]
    [Tooltip("Максимальный уровень здоровья, который может быть достигнут.")]
    private int maxLevel = 200;

    /// <summary>
    /// Текущий уровень игрока (только для чтения).
    /// Для изменения уровня используйте метод AddXP().
    /// </summary>
    public int CurrentLevel => currentLevel;

    /// <summary>
    /// Текущее количество опыта игрока (только для чтения).
    /// Для добавления опыта используйте метод AddXP().
    /// </summary>
    public float CurrentXP => currentXP;

    [Tooltip("Базовое количество опыта для перехода с 1 на 2 уровень.")]
    public float baseXPToNextLevel = 100f;

    [Tooltip("Множитель роста требуемого опыта на каждый следующий уровень.")]
    public float xpGrowthFactor = 1.5f;

    // Событие, вызываемое при повышении уровня
    public event Action<int> OnLevelUp;

    // Событие для обновления UI опыта: (текущий опыт, опыт до следующего уровня)
    public event Action<float, float> OnXPChanged;

    private void Awake()
    {
        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>();

        // Инициализируем подписчиков начальными значениями
        float required = GetRequiredXPForNextLevel();
        OnXPChanged?.Invoke(currentXP, required);
    }

    /// <summary>
    /// Сколько опыта нужно для перехода на следующий уровень.
    /// </summary>
    private float GetRequiredXPForNextLevel()
    {
        // Например: baseExp * factor^(level-1)
        float required = baseXPToNextLevel;

        // Для 1 уровня (currentLevel = 1) степень будет 0 ? множитель = 1
        int power = Mathf.Max(0, currentLevel - 1);
        required *= Mathf.Round(Mathf.Pow(xpGrowthFactor, power)) / 10f * 10f;

        return required;
    }

    /// <summary>
    /// Добавление опыта. Можно вызывать из других систем
    /// (убийство врага, выполнение квеста и т.д.).
    /// </summary>
    public void AddXP(float amount)
    {
        if (amount <= 0f || currentLevel >= maxLevel)
            return;

        currentXP += amount;

        // Проверяем, хватает ли опыта для повышения уровня (возможно, несколько раз подряд)
        bool leveledUpAtLeastOnce = false;

        while (true)
        {
            float required = GetRequiredXPForNextLevel();

            if (currentXP < required)
                break;

            currentXP -= required;
            LevelUpInternal();
            leveledUpAtLeastOnce = true;
        }

        float nextRequired = GetRequiredXPForNextLevel();
        OnXPChanged?.Invoke(currentXP, nextRequired);

        if (leveledUpAtLeastOnce)
        {
            Debug.Log($"Новый уровень: {currentLevel}, опыт: {currentXP}/{nextRequired}");
        }
    }

    /// <summary>
    /// Внутренняя логика повышения уровня.
    /// </summary>
    private void LevelUpInternal()
    {
        currentLevel++;
        caveCoinIncreasePerLevel += 5f; // Увеличиваем прибавку монет на каждом уровне (пример динамической прогрессии)

        // Уведомляем подписчиков
        OnLevelUp?.Invoke(currentLevel);

        // Пример: усиливаем характеристики игрока при каждом уровне
        if (playerStats != null)
        {
            // Все изменения здоровья/маны и вызовы событий
            // делаем через PlayerStats, чтобы события вызывались
            // только изнутри класса-источника.
            playerStats.ApplyLevelUpBonuses(healthIncreasePerLevel, caveCoinIncreasePerLevel);
        }

        if (currentLevel > maxLevel)
        {
            Debug.LogWarning("Достигнут максимальный уровень! Уровень не будет повышаться дальше.");
            currentLevel = maxLevel;
        }
    }
}