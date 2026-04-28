using System.Collections.Generic;
using UnityEngine;

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
        if (!CanAttack())
            return;

        StartAttackCooldown();

        if (weaponData == null)
        {
            Debug.LogWarning($"{name}: WeaponData не назначен, ближняя атака невозможна.", this);
            return;
        }

        // Важно для урока 7.2 (урон через IDamageable):
        // правило “игрок бьёт только врагов” обеспечивается настройкой hitLayers в инспекторе (обычно только слой Enemy).
        // Если не указан радиус, используем Range из WeaponData
        float radius = hitRadius > 0f ? hitRadius : Range;

        // Если attackOrigin не задан, используем позицию owner или самого оружия
        Vector3 origin = attackOrigin != null
            ? attackOrigin.position
            : (owner != null ? owner.position : transform.position);

        // Простой поиск попаданий.
        // OverlapSphere может вернуть несколько коллайдеров одного и того же врага,
        // поэтому HashSet защищает от нанесения урона несколько раз за одну атаку.
        Collider[] hits = Physics.OverlapSphere(origin, radius, hitLayers);

        if (hits.Length == 0)
        {
            Debug.Log($"{name}: ближняя атака — никого не задели.");
        }
        else
        {
            Debug.Log($"{name}: ближняя атака, задели {hits.Length} объект(ов).");
            HashSet<IDamageable> damagedTargets = new HashSet<IDamageable>();

            foreach (Collider collider in hits)
            {
                Debug.Log($"Попали по объекту: {collider.name}");

                IDamageable damageable = collider.GetComponent<IDamageable>();
                if (damageable == null)
                    damageable = collider.GetComponentInParent<IDamageable>();

                if (damageable != null && damagedTargets.Add(damageable))
                    damageable.TakeDamage(Damage);
            }
        }

        // Здесь же в будущем можно запускать анимацию атаки и звук удара.
    }

    private void OnDrawGizmosSelected()
    {
        // Рисуем сферу удара в редакторе, чтобы видеть радиус
        Gizmos.color = Color.red;

        float radius = hitRadius > 0f ? hitRadius : (weaponData != null ? weaponData.range : 1.5f);
        Vector3 origin = attackOrigin != null
            ? attackOrigin.position
            : (owner != null ? owner.position : transform.position);

        Gizmos.DrawWireSphere(origin, radius);
    }
}
