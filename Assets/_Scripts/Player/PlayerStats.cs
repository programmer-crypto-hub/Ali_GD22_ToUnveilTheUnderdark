using Fusion;
using System;
using UnityEngine;
public class PlayerStats : NetworkBehaviour
{
    public PlayerData playerData;
    public PlayerStatRow playerStatRow;
    public PlayerProgression playerProgression;

    public Animator playerAnim;

    private string currentRole;
    private int currentRoleId;

    [Networked, OnChangedRender(nameof(OnStatsChanged))]
    public int CurrentHealth { get; set; }
    public bool IsDead => CurrentHealth <= 0;

    [Networked, OnChangedRender(nameof(OnStatsChanged))]
    public int MaxHealth { get; set; }

    [Networked, OnChangedRender(nameof(OnStatsChanged))]
    public int Gold { get; set; }

    [Networked, OnChangedRender(nameof(OnStatsChanged))]
    public int CurrentDiceValue { get; set; }

    [Networked]
    public int currentPlayerLevel { get; set; } = 0;
    public int maxLevel = 20;

    [Networked, Capacity(20)] // Max 20 items
    public NetworkArray<int> InventoryItemIDs => default;

    [Networked]
    [OnChangedRender(nameof(OnStatsChanged))]
    public float XP { get; set; }
    [Networked] public string PlayerName { get; set; }


    public void OnStatsChanged()
{
    // These ensure your existing UI listeners still work!
    OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    OnGoldChanged?.Invoke(Gold, 9999); // Max gold
    OnDiceRolled?.Invoke(CurrentDiceValue, 20f); // D20
    
    UpdateUI();
}

    public void SetDefaultValues()
    {
        if (Object.HasStateAuthority) // Only the owner/host sets the starting data
        {
            XP = playerProgression.CurrentXP;
            CurrentHealth = (int)playerData.maxHealth;
            Gold = (int)playerData.caveCoins;
            PlayerName = $"Player {Object.InputAuthority.PlayerId}";
        }
    }
    // This runs every time CaveCoins or Health changes on the network
    // Tell the Stats UI to refresh the display for this player
    //public override void Render() => UIStatsController.Instance.UpdateDisplay(this);
    private void UpdateUI() => playerStatRow.SetStats(PlayerName, CurrentHealth, Gold, XP);

    public int CurrentRoleId => currentRoleId;
    public string CurrentRole => currentRole;

    public event Action<float, float> OnHealthChanged;
    public event Action<int, int> OnGoldChanged;
    public event Action<float, float> OnDiceRolled;
    public event Action<string, int> OnRoleApplied;

    public event Action OnDeath;

    public override void Spawned()
    {
        if (HasStateAuthority) // Only the Server/Host sets initial values
        {
            MaxHealth = playerData.maxHealth;
            CurrentHealth = MaxHealth;
            Gold = (int)playerData.caveCoins;
        }

        // Subscribe to Role changes (Keep your existing logic here)
        if (PlayerRolesController.Instance != null)
        {
            PlayerRolesController.Instance.OnRoleGiven += () =>
            {
                currentRole = PlayerRolesController.Instance.roleName.ToString();
                currentRoleId = PlayerRolesController.Instance.RoleId;
                OnRoleApplied?.Invoke(currentRole, currentRoleId);
            };
        }
    }

    public void ApplyLevelUpBonuses(int healthBonus, float caveCoinsBonus)
    {
        if (!HasStateAuthority) return; // Only the server calculates the level up

        // We modify the NETWORKED variables, NOT the ScriptableObject
        MaxHealth += healthBonus;
        CurrentHealth = MaxHealth; // Heal to full on level up

        // If you have a MaxGold variable, increase that too
        // Gold += caveCoinsBonus; 
    }

    public void TakeDamage(int amount)
    {
        if (!HasStateAuthority) return; // SERVER ONLY
        if (amount <= 0f || CurrentHealth <= 0f) return;

        CurrentHealth = Mathf.Clamp(CurrentHealth - amount, 0, MaxHealth);

        if (CurrentHealth <= 0f)
        {
            OnDeath?.Invoke();
            // Since Animator isn't networked by default, use an RPC for the animation
            RPC_PlayDeathAnimation();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayDeathAnimation()
    {
        playerAnim.SetInteger("health", -1);
    }

    public void Heal(int amount)
    {
        if (!HasStateAuthority) return; // SERVER ONLY
        if (playerData == null)
        {
            Debug.LogWarning("PlayerStats.Heal: PlayerData не назначен.", this);
            return;
        }

        // Ќет смысла лечить на неположительное значение или лечить мЄртвого.
        if (amount <= 0 || CurrentHealth <= 0)
            return;

        CurrentHealth += amount;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, playerData.maxHealth);
    }

    public void AddGold(int amount)
    {
        if (!HasStateAuthority) return; // SERVER ONLY
        if (amount <= 0) return;

        Gold += amount;
        // Note: We don't modify playerData.maxCaveCoins here! 
        // We use a local variable or a networked MaxGold.
    }

    public void RollDice()
    {
        if (!HasStateAuthority) return; // SERVER ONLY

        // The Server generates the random number so no one can cheat
        CurrentDiceValue = UnityEngine.Random.Range(1, playerData.maxDiceValue + 1);
    }

    public void ApplyDiceMultiplier(float multiplier)
    {
        if (!HasStateAuthority) return; // SERVER ONLY
        if (playerData == null)
        {
            Debug.LogWarning("PlayerStats.ApplyDiceMultiplier: PlayerData не назначен.", this);
            return;
        }
        if (multiplier <= 0 || multiplier >= 10)
            return;
        CurrentDiceValue = Mathf.RoundToInt(CurrentDiceValue * multiplier);
        CurrentDiceValue = Mathf.Clamp(CurrentDiceValue, 1, playerData.maxDiceValue);
        OnDiceRolled?.Invoke(CurrentDiceValue, playerData.maxDiceValue);
    }
}
