using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Player Stats to Obtain Data")]
    [SerializeField]
    [Tooltip("Reference to PlayerStats component to access player data and events.")]
    public PlayerStats playerStats;
    [SerializeField]
    public PlayerData playerData;
    [SerializeField]
    public PlayerController playerController;

    public static PlayerMovement Instance { get; private set; }

    [Header("Player Stats")]
    [Tooltip("Current Stats of the Player")]
    private float currentDamage = 0;
    private int currentDiceValue { get; set; }

    public void Awake()
    {
        Instance = this;
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Prevents duplicate managers
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("SpaceTrigger"))
        {
            if (GameManager.Instance.CurrentState == GameState.Playing && currentDiceValue > 0)
            {
                currentDiceValue--;
                collision.enabled = false;
                new WaitForSeconds(0.5f); // Задержка для предотвращения мгновенного повторного срабатывания
            }
        }
    }

    public void OnDiceRolled()
    {
        // Получаем результат броска кубика из DiceManager
        currentDiceValue = DiceManager.Instance.GetDiceValue();
        DiceManager.Instance.DisplayDice(currentDiceValue);

        if (GameManager.Instance.CurrentState == GameState.Playing)
        {
            // Конвертируем результат броска кубика в количество шагов для перемещения игрока
            DiceManager.Instance.ConvertDiceToMovement();
            playerController.HandleMovement();
            // Здесь можно добавить логику для перемещения игрока на основе результата броска кубика
            Debug.Log($"Игрок может переместиться на {currentDiceValue} шагов.");
        }

        if (GameManager.Instance.CurrentState == GameState.Combat)
        {
            float damage = 0;
            DiceManager.Instance.ConvertDiceToCombat(damage);
            currentDamage *= damage;
            // Логика для боя, если игрок находится в боевом состоянии
            Debug.Log($"Игрок атакует с силой {currentDamage} и броском кубика {currentDiceValue}.");
        }
    }

    public void OnCollisionEnter2D(Collision2D other)
    {
        if (currentDiceValue <= 0)
        {
            Debug.Log("Player has used all movement steps for this turn.");
            return;
        }
        if (other.gameObject.CompareTag("Enemy") && GameManager.Instance.CurrentState == GameState.Playing && currentDiceValue > 0)
        {
            currentDiceValue--; // Decrease remaining steps left to move
            new WaitForEndOfFrame(); // Delay to prevent immediate retriggering
            Debug.Log(other + " triggered! Remaining steps: " + currentDiceValue);
        }
    }
}