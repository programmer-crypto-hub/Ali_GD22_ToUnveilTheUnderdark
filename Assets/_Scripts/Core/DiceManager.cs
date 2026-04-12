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
    public Image diceImage; // <-- Добавьте это поле и назначьте через инспектор
    public GameObject dicePanel; // Панель для отображения результата броска кубика

    [Header("Dice Settings")]
    public int diceRollResult;
    public int spaceLength;
    public Image[] diceSprites;

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

    public void DisplayDice(int diceValue)
    {
        if (diceSprites == null || diceImage == null)
        {
            Debug.LogWarning("Dice sprites or dice image is not assigned!");
            return;
        }
        if (diceSprites != null && diceSprites.Length > 0)
        {
            dicePanel.SetActive(true); // Показываем панель с результатом броска кубика
            diceImage = diceSprites[diceValue - 1];
            diceImage.enabled = true;
            new WaitForSeconds(2f); // Задержка для отображения результата броска кубика
            diceImage.enabled = false; // Скрываем изображение после задержки
        }
    }
}
