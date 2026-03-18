using UnityEngine;
using UnityEngine.UI;

public class DiceManager : MonoBehaviour
{
    public static DiceManager Instance { get; private set; }

    [Header("Animation Settings")]
    public Animator playerAnim;

    [Header("UI Settings")]
    public Button rollDiceButton;

    [Header("Dice Settings")]
    public GameObject dice;
    int diceRollResult;

    public void Awake()
    {
        rollDiceButton.onClick.AddListener(RollDice);
        PlayerMovement.Instance.OnDiceRolled();
    }

    public void RollDice()
    {
        playerAnim.SetTrigger("roll_Trig");
        new WaitForSeconds(5f); // Задержка для анимации броска кубика
        playerAnim.SetTrigger("run_Trig");
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
        int movedSpaces = (diceRollResult /= 3);
        Debug.Log($"Игрок может переместиться на {diceRollResult} шагов.");
    }

    public void ConvertDiceToCombat(int damage)
    {
        damage *= (diceRollResult / 20); 
        // Применение процента к урону и округление до целого числа
    }
}
