using UnityEngine;
using Fusion;

public class BossController : NetworkBehaviour
{
    // 1. STATE ENUM: Perfect for tracking what the boss is doing
    public enum BossState { Idle, Chasing, Attacking, PhaseTransition }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float attackRange = 2f;

    [Header("References")]
    [SerializeField] private Animator animator;

    // 2. NETWORKED VARIABLES: Syncs the current behavior to all clients
    [Networked] public BossState CurrentState { get; set; }
    [Networked] public NetworkObject TargetPlayer { get; set; }

    private ChangeDetector _changes;

    public override void Spawned()
    {
        _changes = GetChangeDetector(ChangeDetector.Source.SimulationState);

        if (Object.HasStateAuthority)
        {
            CurrentState = BossState.Idle;
        }
    }

    public override void FixedUpdateNetwork()
    {
        // Only the Server/Host calculates AI behavior
        if (!Object.HasStateAuthority) return;

        FindNearestPlayer();

        if (TargetPlayer != null)
        {
            float distance = Vector2.Distance(transform.position, TargetPlayer.transform.position);

            if (distance > attackRange)
            {
                CurrentState = BossState.Chasing;

                // Move towards player
                Vector2 direction = (TargetPlayer.transform.position - transform.position).normalized;
                transform.position += (Vector3)direction * moveSpeed * Runner.DeltaTime;
            }
            else
            {
                CurrentState = BossState.Attacking;
                TriggerAttack();
            }
        }
        else
        {
            CurrentState = BossState.Idle;
        }
    }

    public override void Render()
    {
        // 3. LISTEN TO STATE CHANGES: Update animations/visuals on ALL clients
        foreach (var change in _changes.DetectChanges(this))
        {
            if (change == nameof(CurrentState))
            {
                UpdateVisualsByState(CurrentState);
            }
        }
    }

    private void UpdateVisualsByState(BossState state)
    {
        switch (state)
        {
            case BossState.Idle:
                animator.SetBool("IsMoving", false);
                break;
            case BossState.Chasing:
                animator.SetBool("IsMoving", true);
                break;
            case BossState.Attacking:
                animator.SetBool("IsMoving", false);
                break;
        }
    }

    private void FindNearestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float closestDistance = float.MaxValue;
        NetworkObject closestPlayer = null;

        foreach (var player in players)
        {
            float dist = Vector2.Distance(transform.position, player.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestPlayer = player.GetComponent<NetworkObject>();
            }
        }
        TargetPlayer = closestPlayer;
    }

    private void TriggerAttack()
    {
        animator.SetTrigger("Attack");

        // 4. RPC CALL: Good for sudden, one-shot events everyone must witness
        RPC_PlayBossRoar();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_PlayBossRoar()
    {
        // Play roar SFX on all machines!
        // CameraShake.Instance.Shake();
        Debug.Log("The Boss Roars!");
    }
}
