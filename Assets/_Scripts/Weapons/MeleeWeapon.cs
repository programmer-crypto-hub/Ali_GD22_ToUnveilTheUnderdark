using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class MeleeWeapon : WeaponBase
{
    [Header("Melee attack parameters")]
    [Tooltip("The point from which the attack is calculated (usually the sword/hand).")]
    [SerializeField]
    private Transform attackOrigin;

    [Tooltip("The radius of the attack. If 0, the Range from WeaponData can be used.")]
    [SerializeField]
    private float hitRadius = 1.5f;

    [Tooltip("Layers that can be damaged by this attack (enemies, breakable objects).")]
    [SerializeField]
    private LayerMask hitLayers;

    public override void Attack()
    {
        if (!CanAttack() || owner != null && !owner.GetComponent<NetworkObject>().HasInputAuthority) 
            return;

        StartAttackCooldown();

        if (weaponData == null)
        {
            Debug.LogWarning($"{name}: WeaponData isn't assigned.", this);
            return;
        }

        float radius = hitRadius > 0f ? hitRadius : Range;
        Vector3 origin = attackOrigin != null ? attackOrigin.position : (owner != null ? owner.position : transform.position);

        var animator = owner != null ? owner.GetComponentInChildren<Animator>() : null;
        if (animator != null)
        {
            animator.SetTrigger("attack_trig");
        }

        Collider[] hits = new Collider[10]; // Create a fixed size array to store hits.

        PhysicsScene networkPhysics = default;
        if (owner != null)
        {
            var networkObject = owner.GetComponent<NetworkObject>();
            if (networkObject != null && networkObject.Runner != null)
            {
                networkPhysics = networkObject.Runner.GetPhysicsScene();
            }
        }

        int hitCount = 0;
        if (networkPhysics != null)
        {
            hitCount = networkPhysics.OverlapSphere(origin, radius, hits, hitLayers, QueryTriggerInteraction.UseGlobal);
        }
        else
        {
            hitCount = Physics.OverlapSphereNonAlloc(origin, radius, hits, hitLayers);
        }

        if (hitCount == 0)
        {
            Debug.Log($"{name}: melee attack Ч nobody was hit.");
        }
        else
        {
            Debug.Log($"{name}: close-range attack, hit {hitCount} object(s).");
            HashSet<IDamageable> damagedTargets = new HashSet<IDamageable>();

            for (int i = 0; i < hitCount; i++)
            {
                Collider collider = hits[i];
                if (collider == null) continue;

                Debug.Log($"Hit network object: {collider.name}");

                IDamageable damageable = collider.GetComponent<IDamageable>();
                if (damageable == null) damageable = collider.GetComponentInParent<IDamageable>();

                if (damageable != null && damagedTargets.Add(damageable))
                {
                    // EnemyBase on the server will instantly reduce the boss's HP
                    damageable.TakeDamage(Damage);
                }
            }
        }
    }


    private void OnDrawGizmosSelected()
    {
        // –исуем сферу удара в редакторе, чтобы видеть радиус
        Gizmos.color = Color.red;

        float radius = hitRadius > 0f ? hitRadius : (weaponData != null ? weaponData.range : 1.5f);
        Vector3 origin = attackOrigin != null
            ? attackOrigin.position
            : (owner != null ? owner.position : transform.position);

        Gizmos.DrawWireSphere(origin, radius);
    }
}
