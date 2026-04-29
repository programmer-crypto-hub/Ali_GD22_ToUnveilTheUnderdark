using UnityEngine;
using Fusion;

public class PlayerCombatController : NetworkBehaviour
{
    [Header("Connections")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private PlayerAnimationController playerAnimationController;

    [Header("Effects")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Transform attackEffectPoint;
    [SerializeField] private GameObject attackStartEffectPrefab;
    [SerializeField] private GameObject attackActionEffectPrefab;

    // Use a networked "count" or "state" to trigger animations on proxies
    [Networked] private int AttackTriggerCount { get; set; }
    private int _localAttackCount;

    private bool _isAttackInProgress;

    public override void Render()
    {
        // Detect if the AttackTriggerCount increased on the network
        if (AttackTriggerCount > _localAttackCount)
        {
            _localAttackCount = AttackTriggerCount;
            TriggerVisualAttack();
        }
    }

    public bool TryStartAttack()
    {
        if (!Object.HasInputAuthority || !CanStartAttack())
            return false;

        // 1. Tell the server to increment the attack count
        // This will automatically sync to all clients
        AttackTriggerCount++;

        _isAttackInProgress = true;
        return true;
    }

    private void TriggerVisualAttack()
    {
        var type = ResolveAttackAnimationType();
        playerAnimationController.PlayAttack(type);

        // Play local-only "wind up" effects
        PlayAttackStartEffects();
    }

    // Called by Unity Animation Event
    public void HandleAttackActionAnimationEvent()
    {
        // ONLY the person who "owns" this player should calculate damage
        if (!Object.HasStateAuthority) return;

        if (weaponManager.weaponPrefabs != null)
        {
            weaponManager.PerformCurrentWeaponAttack();
            // Tell everyone to play the "Hit/Muzzle" effect
            RPC_PlayActionEffects();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayActionEffects()
    {
        // This runs on everyone's machine
        if (attackActionEffectPrefab != null && attackEffectPoint != null)
            Instantiate(attackActionEffectPrefab, attackEffectPoint.position, attackEffectPoint.rotation);

        // audioSource.PlayOneShot(attackClip);
    }

    public void HandleAttackFinishedAnimationEvent()
    {
        _isAttackInProgress = false;
    }

    private bool CanStartAttack()
    {
        var weapon = weaponManager.CurrentWeapon;
        return playerStats != null && !playerStats.IsDead && !_isAttackInProgress 
        && weaponManager.weaponPrefabs != null && weapon != null && weapon.CanAttack(); 
    }

    private PlayerAnimationController.AttackAnimationType ResolveAttackAnimationType()
    {
        var weapon = weaponManager.CurrentWeapon;
        if (weapon is RangedWeapon) // Assuming RangedWeapon exists
            return PlayerAnimationController.AttackAnimationType.Ranged;
        return PlayerAnimationController.AttackAnimationType.Melee;
    }

    private void PlayAttackStartEffects()
    {
        if (attackStartEffectPrefab != null && attackEffectPoint != null)
            Instantiate(attackStartEffectPrefab, attackEffectPoint.position, attackEffectPoint.rotation);
    }
}
