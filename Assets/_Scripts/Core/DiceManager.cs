using UnityEngine;
using System.Collections;
using Fusion;
using UnityEngine.UI;

public class DiceManager : NetworkBehaviour
{
    public static DiceManager Instance { get; private set; }

    public PlayerMovement playerMovement;
    public Animator playerAnim;

    [Header("UI Settings")]
    public Button rollDiceButton;
    public Image diceImage; // <-- Добавьте это поле и назначьте через инспектор
    public GameObject dicePanel; // Панель для отображения результата броска кубика

    [Header("Dice Settings")]
    [Networked, OnChangedRender(nameof(OnDiceChanged))] public int diceRollResult { get; set; }
    public int spaceLength;
    public Sprite[] diceSprites;

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_RequestRollDice()
    {
        int result = GetDiceValue();
        diceRollResult = result;
        Debug.Log($"Результат броска: {diceRollResult}");
        //playerAnim.SetTrigger("roll_Trig");
        new WaitForSeconds(5f); // Задержка для анимации броска кубика
    }

    void OnDiceChanged()
    {
        if (diceRollResult < 0) return;
        if (diceRollResult >= 1 && diceRollResult <= diceSprites.Length)
        {
            diceImage.sprite = diceSprites[diceRollResult - 1];
        }
        dicePanel.SetActive(true);
        diceImage.enabled = true;
        // 1. Play the "Rolling" animation for everyone
        playerAnim.SetTrigger("roll_Trig");

        // 2. Instead of a 'WaitForSeconds', use an Animation Event or 
        // a simple timer to trigger the NEXT animation
        StartCoroutine(PlayResultAnimationAfterDelay(2f));
    }
    private IEnumerator PlayResultAnimationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Check the game state locally to play the right follow-up
        if (GameManager.Instance.CurrentState == GameState.Playing)
            playerAnim.SetTrigger("walk_trig");
        else if (GameManager.Instance.CurrentState == GameState.Combat)
            playerAnim.SetTrigger("attack_trig");
    }
    public override void Spawned()
    {
        rollDiceButton.onClick.AddListener(RPC_RequestRollDice);
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

    private int GetDiceValue()
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
            diceImage.sprite = diceSprites[diceValue - 1];
            diceImage.enabled = true;
            new WaitForSeconds(2f); // Задержка для отображения результата броска кубика
            diceImage.enabled = false; // Скрываем изображение после задержки
        }
    }
}
