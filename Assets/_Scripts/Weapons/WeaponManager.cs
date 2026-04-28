using Fusion;
using UnityEngine;

public class WeaponManager : NetworkBehaviour
{
    [Networked] public int UnlockedWeaponMask { get; set; }
    [Networked] public int CurrentWeaponIndex { get; set; }

    // This is your inspector array (not networked, just the data source)
    [SerializeField] private WeaponBase[] weaponPrefabs;

    private ChangeDetector _changes;

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
}
