using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Player Stats to Obtain Data")]
    [Tooltip("Reference to PlayerStats component to access player data and events.")]
    public PlayerStats playerStats;

    public static PlayerMovement Instance { get; private set; }

    [Header("Player Stats")]
    [Tooltip("Current Stats of the Player")]
    private float currentDamage = 0;
    private int currentDiceValue { get; set; }

    public void Awake()
    {
        Instance = this;
    }

    public void OnDiceRolled()
    {
        // Получаем результат броска кубика из DiceManager
        int diceValue = DiceManager.Instance.GetDiceValue();

        if (GameManager.Instance.CurrentState == GameState.Playing)
        {
            // Конвертируем результат броска кубика в количество шагов для перемещения игрока
            DiceManager.Instance.ConvertDiceToMovement();
            // Здесь можно добавить логику для перемещения игрока на основе результата броска кубика
            Debug.Log($"Игрок может переместиться на {diceValue} шагов.");
        }

        if (GameManager.Instance.CurrentState == GameState.Combat)
        {
            float damage = 0;
            DiceManager.Instance.ConvertDiceToCombat(damage);
            currentDamage *= damage;
            // Логика для боя, если игрок находится в боевом состоянии
            Debug.Log($"Игрок атакует с силой {currentDamage} и броском кубика {diceValue}.");
        }
    }

    public void MoveFromDiceValue(int diceValue)
    {
        // Логика для перемещения игрока на основе результата броска кубика
        Debug.Log($"Игрок перемещается на {diceValue} шагов.");
    }
}