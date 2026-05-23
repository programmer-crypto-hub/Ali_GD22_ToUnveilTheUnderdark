using UnityEngine;
using Fusion;
using System.Collections;
using System;

public class PlayerMovement : NetworkBehaviour
{
    [Header("References")]
    public PlayerStats playerStats;
    public PlayerData playerData;
    public PlayerController playerController;

    public static PlayerMovement Instance { get; private set; }
    [Networked] public float currentDamage { get; set; }
    [Networked] private int currentDiceValue { get; set; }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("SpaceTrigger"))
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.Playing && currentDiceValue > 0)
            {
                // Decrease dice value after being triggered
                currentDiceValue--;
                collision.enabled = false;
                StartCoroutine(SpaceEnterDelay(collision));
            }
        }
    }

    // Ensure the player doesn't hit the same trigger twice
    // To not cut in half his movement
    public IEnumerator SpaceEnterDelay(Collider2D collision)
    {
        yield return new WaitForSeconds(0.5f);
        if (collision != null) collision.enabled = true;
    }

    public void OnDiceRolled()
    {
        // Only allow the owner of this piece to process the roll logic
        if (!HasStateAuthority) return;
        if (DiceUI.Instance == null && GameManager.Instance == null && DiceRoller.Instance == null) return;
        // Obtain rolled value
        currentDiceValue = DiceRoller.Instance.DiceRollResult;
        DiceUI.Instance.HandleDiceRolled(currentDiceValue);
        // Invoke a C# event (fires an error)
        DiceRoller.Instance.OnDiceRollCompleted?.Invoke(currentDiceValue); 

        if (GameManager.Instance.CurrentState == GameManager.GameState.Playing)
        {
            // Use dice to convert to movement steps 
            // If is playing
            DiceRoller.Instance.ConvertDiceToMovement();
            Debug.Log($"Player can move: remaining steps = {currentDiceValue}");
        }
        else if (GameManager.Instance.CurrentState == GameManager.GameState.Combat)
        {
            // Use dice to convert to combat damage
            // If is in an encounter
            DiceRoller.Instance.ConvertDiceToCombat();
            Debug.Log($"Player can attack.");
        }
    }

    // Questionable to use both OnTriggerEnter2D and OnCollisionEnter2D, but for now we can use collision to detect enemies and trigger to detect space tiles
    public void OnCollisionEnter2D(Collision2D other)
    {
        // Duplicate logic for no valid reason
        if (currentDiceValue <= 0) return;

        if (other.gameObject.CompareTag("Enemy") && GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.Playing)
        {
            currentDiceValue--;
            Debug.Log($"{other.gameObject.name} hit! Steps remaining: {currentDiceValue}");
        }
    }
}
