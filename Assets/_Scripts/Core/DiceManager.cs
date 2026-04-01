using UnityEngine;
using UnityEngine.UI;

public class DiceManager : MonoBehaviour
{
    public static DiceManager Instance { get; private set; }

    public PlayerMovement playerMovement;

    [Header("Animation Settings")]
    [SerializeField]
    public Animator playerAnim;

    [Header("UI Settings")]
    public Button rollDiceButton;

    [Header("Dice Settings")]
    //public GameObject dice;
    public int diceRollResult;
    public int spaceLength;

    public void Awake()
    {
        rollDiceButton.onClick.AddListener(RollDice);
        //playerMovement.OnDiceRolled();

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Prevents duplicate managers
        }
    }

    public void RollDice()
    {
        GetDiceValue();
        Debug.Log($"Результат броска: {diceRollResult}");
        //playerAnim.SetTrigger("roll_Trig");
        new WaitForSeconds(5f); // Задержка для анимации броска кубика
        if (GameManager.Instance.CurrentState == GameState.Playing)
            playerAnim.SetTrigger("walk_trig");
        if (GameManager.Instance.CurrentState == GameState.Combat)
            playerAnim.SetTrigger("attack_trig");
    }

    public int GetDiceValue()
    {
        diceRollResult = Random.Range(1, 21); // Генерируем число от 1 до 20
        Debug.Log($"Игрок бросил кубик и получил: {diceRollResult}");
        return diceRollResult;
    }

    public void ApplyDiceMult(int multiplier)
    {
        diceRollResult *= multiplier;
        Debug.Log($"Результат броска после применения множителя {multiplier}: {diceRollResult}");
    }

    public void ResetDice() => diceRollResult = 0;

    public void ConvertDiceToMovement()
    {
        int diceToMoveApprox = 3;
        int movedSpaces = (diceRollResult /= diceToMoveApprox);
        Debug.Log($"Игрок может переместиться на {diceRollResult} шагов.");
    }

    public void ConvertDiceToCombat(float damage)
    {
        int diceToCombatApprox = 100 / 5;
        damage = (diceRollResult / diceToCombatApprox);
        // Применение процента к урону и округление до целого числа
    }
}
