// TODO: Find usage for this script (for example, combat), or delete.
using UnityEngine;
using Fusion;

public class PlayerAttackAnimationEvents : NetworkBehaviour
{
    [Header("Связи")]
    [Tooltip("Боевой контроллер игрока, который обрабатывает фазы атаки.")]
    [SerializeField] private PlayerCombatController playerCombatController;

    public override void Spawned()
    {
        if (playerCombatController == null)
            playerCombatController = GetComponentInParent<PlayerCombatController>();
    }

    public void OnAttackActionAnimationEvent()
    {
        playerCombatController?.HandleAttackActionAnimationEvent();
    }

    public void OnAttackFinishedAnimationEvent()
    {
        playerCombatController?.HandleAttackFinishedAnimationEvent();
    }
}