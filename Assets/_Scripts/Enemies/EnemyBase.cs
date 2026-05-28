using System;
using UnityEngine;
using Fusion;

public class EnemyBase : NetworkBehaviour, IDamageable
{
    protected enum EnemyState { Chase, Attack, Dead, Idle }

    [Header("Enemy Data")]
    [SerializeField] private EnemyData enemyData;
    [SerializeField] public Animator enemyAnim;

    // 1. BANDWIDTH OPTIMIZATION: Networked state variables
    [Networked] public float CurrentHP { get; set; }
    [Networked] protected EnemyState CurrentState { get; set; }
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
            CurrentHP = enemyData != null ? enemyData.maxHealth : 10f;
            CurrentState = EnemyState.Idle;
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
            CurrentState = EnemyState.Idle; // Default fallback
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
                enemyAnim.SetTrigger("walk_trig");
                break;
            case EnemyState.Attack:
                enemyAnim.SetTrigger("attack_trig");
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
        Invoke(nameof(DespawnBoss), 2f);
    }

    private void DespawnBoss()
    {
        if (Object != null && Runner != null)
        {
            Runner.Despawn(Object);
        }
    }

    public void FindClosestPlayer()
    {
        float closestDistance = float.MaxValue;
        PlayerController closestPlayer = null;

        // Безопасный сетевой поиск ближайшего живого игрока по компоненту
        foreach (var player in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
        {
            float dist = Vector3.Distance(transform.position, player.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestPlayer = player;
            }
        }

        if (closestPlayer != null)
        {
            _target = closestPlayer.transform;
        }
    }
}