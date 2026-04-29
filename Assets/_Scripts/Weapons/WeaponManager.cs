using Fusion;
using UnityEngine;

public class WeaponManager : NetworkBehaviour
{
    [Header("Combat Settings")]
    [SerializeField] private LayerMask enemyLayer;

    private PlayerStats playerStats;
    [Networked] public int UnlockedWeaponMask { get; set; }
    [Networked] public int CurrentWeaponIndex { get; set; }

    // This is your inspector array (not networked, just the data source)
    [SerializeField] public WeaponBase[] weaponPrefabs;

    private ChangeDetector _changes;

    public WeaponBase CurrentWeapon
    {
        get
        {
            if (weaponPrefabs == null || CurrentWeaponIndex < 0 || CurrentWeaponIndex >= weaponPrefabs.Length)
                return null;
            return weaponPrefabs[CurrentWeaponIndex];
        }
        set { }
    }

    public override void Spawned()
    {
        if (Object.HasStateAuthority && UnlockedWeaponMask == 0)
        {
            // Unlock the first weapon by default (bit 0)
            UnlockWeapon(0);
        }
        _changes = GetChangeDetector(ChangeDetector.Source.SimulationState);
    }

    public override void Render()
    {
        foreach (var change in _changes.DetectChanges(this))
        {
            if (change == nameof(CurrentWeaponIndex))
            {
                UpdateWeaponVisuals(CurrentWeaponIndex);
            }
        }
    }

    private void UpdateWeaponVisuals(int index)
    {
        for (int i = 0; i < weaponPrefabs.Length; i++)
        {
            weaponPrefabs[i].gameObject.SetActive(i == index);
        }
    }

    // Call this when a player buys a weapon in the Shop
    public void UnlockWeapon(int index)
    {
        // Sets the bit at 'index' to 1
        UnlockedWeaponMask |= (1 << index);
    }

    public bool IsWeaponUnlocked(int index)
    {
        // Checks if the bit at 'index' is 1
        return (UnlockedWeaponMask & (1 << index)) != 0;
    }

    public void SwitchToNextWeapon()
    {
        if (!Object.HasInputAuthority) return;

        // Simple loop to find the next unlocked bit
        for (int i = 1; i <= weaponPrefabs.Length; i++)
        {
            int next = (CurrentWeaponIndex + i) % weaponPrefabs.Length;
            if (IsWeaponUnlocked(next))
            {
                CurrentWeaponIndex = next;
                break;
            }
        }
    }
    public void PerformCurrentWeaponAttack()
    { 
        // CRITICAL: Only the Server/Host calculates damage to prevent cheating
        if (!Object.HasStateAuthority) return;

        if (CurrentWeapon == null) return;

        var weapon = CurrentWeapon;

        // Logic check: What kind of attack are we doing?
        if (weapon is RangedWeapon ranged)
        {
            ExecuteRangedAttack(ranged);
        }
        else
        {
            ExecuteMeleeAttack(weapon);
        }
    }
    private void ExecuteMeleeAttack(WeaponBase weapon)
    {
        // 1. Detect enemies in a circle in front of the player
        Vector2 attackPos = (Vector2)transform.position + (Vector2)transform.up * 0.5f;
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPos, weapon.Range, enemyLayer);

        foreach (var enemy in hitEnemies)
        {
            // 2. Apply damage via the enemy's networked stats script
            if (enemy.TryGetComponent<EnemyStats>(out var stats))
            {
                stats.TakeDamage(weapon.Damage + playerStats.CurrentHealth);
                Debug.Log($"Hit {enemy.name} for {weapon.Damage} damage!");
            }
        }
    }

    private void ExecuteRangedAttack(WeaponBase weapon)
    {
        // 1. Fire a networked raycast
        var hit = Runner.LagCompensation.Raycast(
            transform.position,
            transform.up,
            weapon.Range,
            Object.InputAuthority,
            out var hitInfo,
            enemyLayer
        );

        if (hit)
        {
            if (hitInfo.GameObject.TryGetComponent<EnemyStats>(out var stats))
            {
                stats.TakeDamage(weapon.Damage);
            }
        }
    }
}