using System;
using UnityEngine;
using Fusion;

public class EnemyBase : NetworkBehaviour, IDamageable
{
    private enum EnemyState { Chase, Attack, Dead }

    [Header("Enemy Data")]
    [SerializeField] private EnemyData enemyData;
    [SerializeField] public Animator enemyAnim;

    // 1. BANDWIDTH OPTIMIZATION: Networked state variables
    [Networked] public float CurrentHP { get; set; }
    [Networked] private EnemyState CurrentState { get; set; }
    [Networked] private TickTimer AttackCooldown { get; set; }

    private Transform _target;
    private ChangeDetector _changes;

    // Read-only property for IDamageable
    public bool IsDead => CurrentState == EnemyState.Dead;
    public event Action OnDied;

    public override void Spawned()
    {
        _changes = GetChangeDetector(ChangeDetector.Source.SimulationState);

        if (HasStateAuthority)
        {
            CurrentHP = enemyData != null ? enemyData.maxHealth : 100f;
            CurrentState = EnemyState.Chase;
        }
    }

    public override void FixedUpdateNetwork()
    {
        // 2. SERVER ONLY: Only the host runs AI calculations
        if (!HasStateAuthority || IsDead) return;

        if (_target == null)
        {
            FindClosestPlayer();
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, _target.position);

        // State Machine
        if (distanceToTarget <= enemyData.attackRange)
        {
            CurrentState = EnemyState.Attack;
            TryAttack();
        }
        else if (distanceToTarget <= enemyData.detectionRange)
        {
            CurrentState = EnemyState.Chase;
            MoveTowardsTarget();
        }
        else
        {
            CurrentState = EnemyState.Chase; // Default fallback
        }
    }

    public override void Render()
    {
        // 3. ANIMATION SYNC: Triggers animations on all clients via state changes
        foreach (var change in _changes.DetectChanges(this))
        {
            if (change == nameof(CurrentState))
            {
                UpdateAnimations();
            }
        }
    }

    private void MoveTowardsTarget()
    {
        Vector3 direction = (_target.position - transform.position).normalized;
        direction.y = 0f; // Keep on flat plane if needed

        transform.position += direction * enemyData.moveSpeed * Runner.DeltaTime;

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }

    private void TryAttack()
    {
        if (!AttackCooldown.ExpiredOrNotRunning(Runner)) return;

        // Apply damage to player
        if (_target.TryGetComponent<IDamageable>(out var player))
        {
            player.TakeDamage(enemyData.damage);
        }

        // Reset cooldown using Fusion TickTimer
        AttackCooldown = TickTimer.CreateFromSeconds(Runner, 1.0f); // Default 1s cooldown
    }

    private void UpdateAnimations()
    {
        switch (CurrentState)
        {
            case EnemyState.Chase:
                enemyAnim.SetTrigger("enemy_move_trig");
                break;
            case EnemyState.Attack:
                enemyAnim.SetTrigger("enemy_attack_trig"); // Assuming you have an attack trigger
                break;
        }
    }

    public void TakeDamage(float damage)
    {
        if (!HasStateAuthority || IsDead) return;

        CurrentHP -= damage;

        if (CurrentHP <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        CurrentState = EnemyState.Dead;
        OnDied?.Invoke();

        // 4. NETWORK DELETE: Safely remove from network for all players
        Runner.Despawn(Object);
    }

    private void FindClosestPlayer()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            _target = player.transform;
        }
    }
}
