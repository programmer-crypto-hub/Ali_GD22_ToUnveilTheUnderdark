using UnityEngine;
using Fusion;
using System;

public class DiceRoller : NetworkBehaviour
{
    public static DiceRoller Instance { get; private set; }

    // 1. Network the result. Render() will catch when it changes!
    [Networked] public int DiceRollResult { get; set; } = -1;

    public event Action<int> OnDiceRollCompleted;

    public override void Spawned()
    {
        if (Instance == null) Instance = this;
    }

    // Clients call this to tell the server "I am rolling!"
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestRollDice()
    {
        if (!HasStateAuthority) return;

        // Server rolls the random number (1 to 20)
        DiceRollResult = UnityEngine.Random.Range(1, 21);
        Debug.Log($"Server rolled: {DiceRollResult}");
    }

    public override void Render()
    {
        // 2. State Sync: Detect when the roll arrives
        if (DiceRollResult > 0)
        {
            OnDiceRollCompleted?.Invoke(DiceRollResult);
        }
    }

    // This logic translates score into grid spaces (No UI involved!)
    public int ConvertDiceToMovement()
    {
        int spaces = Mathf.RoundToInt(DiceRollResult / 3.0f);
        return spaces == 0 ? 1 : spaces; // Ensure at least 1 space
    }
    public int ConvertDiceToCombat()
    {
        int diceToCombatApprox = 100 / 5;
        int damage = (DiceRollResult / diceToCombatApprox);
        return damage;
    }

    public void ResetDice()
    {
        if (HasStateAuthority) DiceRollResult = -1;
    }
}
