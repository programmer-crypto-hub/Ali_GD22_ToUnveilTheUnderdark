using UnityEngine;

public class PlayerStatsTest : MonoBehaviour
{
    [Tooltip("Ссылка на компонент PlayerStats. Если не назначена, будет найдена автоматически на этом объекте.")]
    public PlayerStats playerStats;

    private void Awake()
    {
        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>();
    }

    private void OnEnable()
    {
        if (playerStats == null) return;

        playerStats.OnHealthChanged += HandleHealthChanged;
        playerStats.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        if (playerStats == null) return;

        playerStats.OnHealthChanged -= HandleHealthChanged;
        playerStats.OnDeath -= HandleDeath;
    }

    private void Update()
    {
        if (playerStats == null) return;

        // Нанести урон по нажатию клавиши H.
        if (Input.GetKeyDown(KeyCode.H))
        {
            playerStats.TakeDamage(10);
        }

        // Вылечить по нажатию клавиши J.
        if (Input.GetKeyDown(KeyCode.J))
        {
            playerStats.Heal(10);
        }
    }

    private void HandleHealthChanged(float current, float max)
    {
        Debug.Log($"Здоровье изменилось: {current} / {max}");
    }

    private void HandleDeath()
    {
        Debug.Log("Игрок умер!");
    }
}