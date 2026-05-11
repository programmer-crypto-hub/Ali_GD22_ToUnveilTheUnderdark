using UnityEngine;

public class EnemyDeathRewarder : MonoBehaviour
{
    [Header("Ссылки")]
    [Tooltip("Компонент прогрессии игрока, куда будем добавлять опыт.")]
    public PlayerProgression playerProgression;

    public void RegisterEnemy(EnemyStats stats)
    {
        if (stats == null)
            return;

        // Подписываемся на событие смерти конкретного врага.
        stats.OnDied += HandleEnemyDied;
    }

    private void HandleEnemyDied(EnemyStats stats)
    {
        if (stats == null)
            return;

        stats.OnDied -= HandleEnemyDied;

        if (playerProgression == null)
        {
            Debug.LogWarning("EnemyDeathRewarder: PlayerProgression не назначен.", this);
            return;
        }

        float reward = stats.ExperienceReward;
        if (reward > 0f)
        {
            playerProgression.AddXP(reward);
        }
    }
}

