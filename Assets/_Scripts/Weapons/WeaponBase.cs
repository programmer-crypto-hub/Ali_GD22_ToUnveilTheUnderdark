using UnityEngine;
using Fusion;

public abstract class WeaponBase : NetworkBehaviour
{
    [Header("Weapon Data")]
    [Tooltip("SO with default configs")]
    public WeaponData weaponData;

    [Header("Weapon Owner (Optional)")]
    [Tooltip("Who holds this weapon (e.g., Player). Used for animations/direction of attacks.")]
    public Transform owner;

    [Networked] protected TickTimer AttackCooldown { get; set; }

    protected float nextAttackTime = 0f;

    public float Damage => weaponData != null ? weaponData.damage : 0f;
    public float Range => weaponData != null ? weaponData.range : 0f;
    public float AttackSpeed => weaponData != null ? weaponData.attackSpeed : 1f;

    public virtual bool CanAttack()
    {
        if (weaponData == null)
        {
            Debug.LogWarning($"{name}: WeaponData isn't assigned in the Inspector.", this);
            return false;
        }
        if (weaponData == null) return false;

        // Check if the timer has expired (works across all clients)
        return AttackCooldown.ExpiredOrNotRunning(Runner);
    }

    protected void StartAttackCooldown()
    {
        float cooldown = AttackSpeed > 0f ? (1f / AttackSpeed) : 0.5f;
        AttackCooldown = TickTimer.CreateFromSeconds(Runner, cooldown);
    }

    public abstract void Attack();
}