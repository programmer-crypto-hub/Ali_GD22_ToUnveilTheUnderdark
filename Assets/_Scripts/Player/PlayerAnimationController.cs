using UnityEngine;
using Fusion;

public class PlayerAnimationController : NetworkBehaviour
{
    public enum AttackAnimationType
    {
        Melee = 0,
        Ranged = 1
    }

    [SerializeField] private Animator animator;

    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private string moveSpeedParameter = "MoveSpeed";
    [SerializeField] private string attackTriggerParameter = "Attack";
    [SerializeField] private string attackTypeParameter = "AttackType"; 
    [SerializeField] private string isDeadParameter = "IsDead";

    public override void Spawned()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (playerStats == null)
            playerStats = GetComponentInParent<PlayerStats>();
    }

    private void OnEnable()
    {
        if (GameManager.Events != null)
            GameManager.Events.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        if (GameManager.Events != null)
            GameManager.Events.OnDeath -= HandleDeath;
    }

    public override void FixedUpdateNetwork()
    {
        float moveSpeed = playerStats != null ? playerStats.playerData.moveSpeed : 0f;
        animator.SetFloat(moveSpeedParameter, moveSpeed);
    }

    public void PlayAttack(AttackAnimationType attackAnimationType)
    {
        if (animator == null)
            return;

        animator.SetInteger(attackTypeParameter, (int)attackAnimationType);
        animator.ResetTrigger(attackTriggerParameter);
        animator.SetTrigger(attackTriggerParameter);
    }

    public void PlayDeath()
    {
        if (animator == null)
            return;

        animator.SetBool(isDeadParameter, true);
    }

    private void HandleDeath()
    {
        PlayDeath();
    }
}