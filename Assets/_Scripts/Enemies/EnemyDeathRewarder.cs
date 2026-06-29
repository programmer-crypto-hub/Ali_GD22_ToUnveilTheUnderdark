using UnityEngine;

public class EnemyDeathRewarder : MonoBehaviour
{
    [Header("Ссылки")]
    [Tooltip("Компонент прогрессии игрока, куда будем добавлять опыт.")]
    public PlayerProgression playerProgression;

    public void RegisterEnemy()
    {
        if (GameManager.Events == null) return;
        GameManager.Events.OnEnemyDied += HandleEnemyDied;
    }

    private void HandleEnemyDied()
    {
        if (GameManager.Events == null)
            return;

        GameManager.Events.OnEnemyDied -= HandleEnemyDied;

        if (playerProgression == null)
        {
            Debug.LogWarning("EnemyDeathRewarder: PlayerProgression не назначен.", this);
            return;
        }

        var stats = GetComponent<EnemyStats>();
        float reward = stats.ExperienceReward;
        if (reward > 0f)
        {
            playerProgression.AddXP(reward);
        }
    }
}

