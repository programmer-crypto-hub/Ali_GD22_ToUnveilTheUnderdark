using UnityEngine;
using Fusion;
using System.Collections;

public class PlayerMovement : NetworkBehaviour
{
    [Header("References")]
    public PlayerStats playerStats;
    public PlayerData playerData;
    public PlayerController playerController;

    // REMOVED: public static PlayerMovement Instance (Singletons break multiple client prefabs!)

    [Networked] public float currentDamage { get; set; }
    [Networked] private int currentDiceValue { get; set; }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("SpaceTrigger"))
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.Playing && currentDiceValue > 0)
            {
                currentDiceValue--;
                collision.enabled = false;
                StartCoroutine(SpaceEnterDelay(collision));
            }
        }
    }

    public IEnumerator SpaceEnterDelay(Collider2D collision)
    {
        yield return new WaitForSeconds(0.5f);
        if (collision != null) collision.enabled = true;
    }

    public void OnDiceRolled()
    {
        // Only allow the owner of this piece to process the roll logic
        if (!HasStateAuthority) return;

        currentDiceValue = DiceRoller.Instance.DiceRollResult;

        if (DiceUI.Instance != null)
            DiceUI.Instance.HandleDiceRolled(currentDiceValue);

        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.Playing)
        {
            DiceRoller.Instance.ConvertDiceToMovement();

            // FIX: Instead of trying to force movement calculations instantly via code,
            // your PlayerController will naturally start moving inside its own FixedUpdateNetwork() loop
            // now that currentDiceValue is updated.

            Debug.Log($"Player can move: remaining steps = {currentDiceValue}");
        }
    }

    public void OnCollisionEnter2D(Collision2D other)
    {
        if (currentDiceValue <= 0) return;

        if (other.gameObject.CompareTag("Enemy") && GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.Playing)
        {
            currentDiceValue--;
            Debug.Log($"{other.gameObject.name} hit! Steps remaining: {currentDiceValue}");
        }
    }
}
