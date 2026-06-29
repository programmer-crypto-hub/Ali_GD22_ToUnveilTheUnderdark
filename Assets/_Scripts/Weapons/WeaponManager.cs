using Fusion;
using System;
using UnityEngine;

public class WeaponManager : NetworkBehaviour
{
    [Header("Combat Settings")]
    [SerializeField] private LayerMask enemyLayer;

    /*
     * Usage: Only execute methods, or use variables
     * For other scripts, while not firing an Exception error
     */
    public bool isNetworkReady = false;

    private PlayerStats playerStats;
    [Networked] public int UnlockedWeaponMask { get; set; }
    [Networked] public int CurrentWeaponIndex { get; set; }

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
        // Notify other script about it's Fusion readiness
        isNetworkReady = true;
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

    public void SwitchToPrevWeapon()
    {
        if (!Object.HasInputAuthority) return;
        for (int i = 1; i <= weaponPrefabs.Length; i++)
        {
            int prev = (CurrentWeaponIndex - i + weaponPrefabs.Length) % weaponPrefabs.Length;
            if (IsWeaponUnlocked(prev))
            {
                CurrentWeaponIndex = prev;
                break;
            }
        }
    }
    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            if (data.attackPressed)
            {
                PerformCurrentWeaponAttack();
            }
        }
    }
    public void PerformCurrentWeaponAttack()
    {
        if (CurrentWeapon == null) return;

        // Logic check: What kind of attack are we doing?
        //if (CurrentWeapon is RangedWeapon ranged)
        //{
        //    ExecuteRangedAttack(ranged);
        //}
        {
            ExecuteMeleeAttack(CurrentWeapon);
        }
    }
    private void ExecuteMeleeAttack(WeaponBase weapon)
    {
        // 1. Detect enemies in a circle in front of the player
        Vector2 attackPos = (Vector2)transform.position + (Vector2)transform.up * 0.5f;
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(attackPos, weapon.Range, enemyLayer);

        foreach (var obj in hitObjects)
        {
            // Interface check instead of a specific script!
            if (obj.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(weapon.Damage);
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