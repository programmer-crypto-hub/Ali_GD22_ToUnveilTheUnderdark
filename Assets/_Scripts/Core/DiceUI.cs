using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DiceUI : MonoBehaviour
{
    public static DiceUI Instance;
    [Header("References")]
    public Image diceImage;
    public GameObject dicePanel;
    public Sprite[] diceSprites;
    public Animator playerAnim;

    private void OnEnable()
    {
        DiceRoller.Instance.OnDiceRollCompleted += HandleDiceRolled;
    }

    private void OnDisable()
    {
        DiceRoller.Instance.OnDiceRollCompleted -= HandleDiceRolled;
    }

    public void HandleDiceRolled(int result)
    {
        // 1. Set the correct sprite
        if (result >= 1 && result <= diceSprites.Length)
        {
            diceImage.sprite = diceSprites[result - 1];
        }

        dicePanel.SetActive(true);
        diceImage.enabled = true;

        // 2. Play animations
        playerAnim.SetTrigger("roll_Trig");
        StartCoroutine(HideUIDelay(3f));
    }

    private IEnumerator HideUIDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        diceImage.enabled = false;
        dicePanel.SetActive(false);

        // Tell your separate Movement script to go!
        int spacesToMove = DiceRoller.Instance.ConvertDiceToMovement();
        // playerMovement.MoveSpaces(spacesToMove);
    }
}
