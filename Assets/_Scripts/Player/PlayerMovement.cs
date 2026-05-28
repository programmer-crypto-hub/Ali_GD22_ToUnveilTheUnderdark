using UnityEngine;
using Fusion;
using System;
using ExitGames.Client.Photon;

public class PlayerMovement : NetworkBehaviour
{
    [Header("References")]
    public PlayerStats playerStats;
    public PlayerData playerData;
    public PlayerController playerController;

    public static PlayerMovement Instance;
    [Networked] public float currentDamage { get; set; }
    [Networked] private int currentDiceValue { get; set; }

    // Networked Timer to prevent multiple triggers on the same cell within a short time frame
    [Networked] private TickTimer spaceTriggerCooldown { get; set; }

    private void OnTriggerEnter(Collider collision)
    {
        if (!Runner.IsServer) return; // Просчет шагов делает только сервер (Хост)

        if (collision.CompareTag("SpaceTrigger"))
        {
            // Если таймер кулдауна еще тикает, игнорируем клетку, чтобы не резать шаги дважды
            if (!spaceTriggerCooldown.ExpiredOrNotRunning(Runner)) return;

            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.Playing && currentDiceValue > 0)
            {
                currentDiceValue--;
                Debug.Log($"Stepped on cell {collision.name}. Steps left: {currentDiceValue}");

                // Init a 0.5 second cooldown to prevent multiple triggers on the same cell
                spaceTriggerCooldown = TickTimer.CreateFromSeconds(Runner, 0.5f);

                if (currentDiceValue <= 0)
                {
                    Debug.Log("[BOARD] Ход завершен, шаги закончились!");
                    GameSession.Instance.RPC_RequestEndTurn();
                }
            }
        }
    }

    public void OnDiceRolled()
    {
        if (!HasStateAuthority) return;

        if (DiceUI.Instance == null || GameManager.Instance == null || DiceRoller.Instance == null)
        {
            Debug.LogError("Crucial managers not found on scene!");
            return;
        }

        currentDiceValue = DiceRoller.Instance.DiceRollResult;
        DiceUI.Instance.HandleDiceRolled(currentDiceValue);

        DiceRoller.Instance.OnDiceRollCompleted?.Invoke(currentDiceValue);

        if (GameManager.Instance.CurrentState == GameManager.GameState.Playing)
        {
            DiceRoller.Instance.ConvertDiceToMovement();
            Debug.Log($"Exploration mode activated! Steps available: {currentDiceValue}");
        }
        else if (GameManager.Instance.CurrentState == GameManager.GameState.Combat)
        {
            DiceRoller.Instance.ConvertDiceToCombat();
            Debug.Log($"Combat mode activated!");
        }
    }

    public void OnCollisionEnter(Collision other)
    {
        if (!Runner.IsServer || currentDiceValue <= 0) return;

        if (other.gameObject.CompareTag("Enemy") && GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.Playing)
        {
            Debug.Log($"Hit enemy {other.gameObject.name}!");
        }
    }
}