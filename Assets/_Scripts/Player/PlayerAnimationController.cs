using UnityEngine;
using Fusion;

public class PlayerAnimationController : NetworkBehaviour
{
    public enum AttackAnimationType
    {
        Melee = 0,
        Ranged = 1
    }

    [Header("Связи")]
    [Tooltip("Animator на визуальной модели игрока.")]
    [SerializeField] private Animator animator;

    [Tooltip("Статы игрока. Нужны, чтобы корректно реагировать на смерть и не обновлять лишние параметры после неё.")]
    [SerializeField] private PlayerStats playerStats;

    [Header("Параметры Animator")]
    [Tooltip("Имя float-параметра скорости движения в Animator Controller.")]
    [SerializeField] private string moveSpeedParameter = "MoveSpeed";

    [Tooltip("Имя trigger-параметра атаки в Animator Controller.")]
    [SerializeField] private string attackTriggerParameter = "Attack";

    [Tooltip("Имя int-параметра типа атаки в Animator Controller. Через него Animator решает, какой attack-state запускать: melee или ranged.")]
    [SerializeField] private string attackTypeParameter = "AttackType"; 

    [Tooltip("Имя bool-параметра смерти в Animator Controller.")]
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
        if (playerStats != null)
            playerStats.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        if (playerStats != null)
            playerStats.OnDeath -= HandleDeath;
    }

    private void Update()
    {
        if (animator == null)
            return;

        if (playerStats != null && playerStats.CurrentHealth < 0)
        {
            animator.SetFloat(moveSpeedParameter, 0f);
            return;
        }

        if (InputManager.Instance == null)
            return;

        float moveSpeed = InputManager.Instance.MoveInput.magnitude;
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