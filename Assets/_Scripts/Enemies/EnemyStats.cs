using System;
using UnityEngine;
using Fusion;

public class EnemyStats : NetworkBehaviour, IDamageable
{
    [Header("Data Source")]
    [Tooltip("The Scriptable Object holding this enemy's default rules.")]
    [SerializeField] private EnemyData enemyData;

    // 1. NETWORKED STATE
    // This is the only thing that changes, so it's the only thing that's [Networked].
    [Networked] public float CurrentHP { get; set; }

    // 2. READ-ONLY SHORTCUTS (From Scriptable Object)
    public float ExperienceReward => enemyData != null ? enemyData.experienceReward : 20f;
    public float MaxHP => enemyData != null ? enemyData.maxHealth : 100f;
    public float Damage => enemyData != null ? enemyData.damage : 10f;
    public float MoveSpeed => enemyData != null ? enemyData.moveSpeed : 3f;
    public float AttackRange => enemyData != null ? enemyData.attackRange : 1.5f;

    // 3. IDAMAGEABLE IMPLEMENTATION
    public bool IsDead => CurrentHP <= 0;

    public event Action<EnemyStats> OnDied;

    public override void Spawned()
    {
        // Only the Server/Host initializes the HP to prevent client desyncs
        if (Object.HasStateAuthority && CurrentHP == 0)
        {
            CurrentHP = MaxHP;
        }
    }

    // This method is called by the RoomEncounterHandler
    public void Setup(EnemyData data)
    {
        if (!Object.HasStateAuthority) return;

        enemyData = data;
        CurrentHP = MaxHP;
    }

    // 4. NETWORKED COMBAT LOGIC
    public void TakeDamage(float damage)
    {
        // Only the server actually deducts health to prevent cheating
        if (!Object.HasStateAuthority) return;

        if (IsDead) return;

        CurrentHP -= damage;
        Debug.Log($"{name} took {damage} damage. HP remaining: {CurrentHP}");

        if (IsDead)
        {
            Die();
        }
    }

    public void Die()
    {
        if (!Object.HasStateAuthority) return;

        Debug.Log($"{name} has died.");

        // Notify any local systems (like a UI stats panel)
        OnDied?.Invoke(this);

        // Despawn deletes the object across the network for everyone
        Runner.Despawn(Object);
    }
}
