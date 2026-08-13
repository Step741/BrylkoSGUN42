using System.Collections.Generic;
using UnityEngine;

public class Katana : WeaponBase
{
    [Header("References")]
    [SerializeField]
    private Transform attackPoint;

    private float nextAttackTime;

    private Collider[] hitBuffer;

    private bool attackPending;

    private readonly HashSet<IDamageable> damagedTargets =
        new HashSet<IDamageable>();

    protected override void Awake()
    {
        base.Awake();

        hitBuffer = new Collider[32];
    }

    public override bool Shoot()
    {
        if (config == null)
            return false;

        // Катана не использует патроны.
        if (attackPending)
            return false;

        if (Time.time < nextAttackTime)
            return false;

        if (attackPoint == null)
        {
            Debug.LogError(
                $"{name}: Attack Point is missing."
            );

            return false;
        }

        attackPending = true;

        nextAttackTime =
            Time.time +
            config.KatanaAttackCooldown;

        return true;
    }

    /// <summary>
    /// Вызывается Animation Event
    /// в момент прохождения лезвия через цель.
    /// </summary>
    public void Attack()
    {
        if (!attackPending)
            return;

        attackPending = false;

        if (config == null)
            return;

        if (attackPoint == null)
            return;

        damagedTargets.Clear();

        Vector3 attackCenter =
            attackPoint.position +
            transform.forward *
            config.KatanaAttackRange;

        int hitCount =
            Physics.OverlapSphereNonAlloc(
                attackCenter,
                config.KatanaAttackRadius,
                hitBuffer,
                config.KatanaAttackMask,
                QueryTriggerInteraction.Ignore
            );

        Vector3 attackDirection =
            transform.forward;

        int damagedCount = 0;

        for (int i = 0; i < hitCount; i++)
        {
            Collider hit =
                hitBuffer[i];

            if (hit == null)
                continue;

            IDamageable damageable =
                hit.GetComponentInParent<
                    IDamageable
                >();

            if (damageable == null)
                continue;

            // Не наносим урон одному объекту
            // несколько раз из-за нескольких Collider.
            if (!damagedTargets.Add(
                    damageable))
            {
                continue;
            }

            Vector3 targetDirection =
                (
                    hit.transform.position -
                    transform.position
                ).normalized;

            float dot =
                Vector3.Dot(
                    attackDirection,
                    targetDirection
                );

            // Цель должна находиться
            // перед персонажем.
            if (dot <= 0f)
                continue;

            damageable.TakeDamage(
                config.Damage
            );

            damagedCount++;

            Debug.Log(
                $"Katana hit: {hit.name}"
            );

            if (damagedCount >=
                config.KatanaMaxTargets)
            {
                break;
            }
        }
    }

    public override void Reload()
    {
        // Катана не перезаряжается.
    }

    private void OnDisable()
    {
        attackPending = false;

        damagedTargets.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        if (config == null)
            return;

        if (attackPoint == null)
            return;

        Gizmos.color =
            new Color(
                1f,
                0.2f,
                0.1f,
                0.35f
            );

        Vector3 center =
            attackPoint.position +
            transform.forward *
            config.KatanaAttackRange;

        Gizmos.DrawWireSphere(
            center,
            config.KatanaAttackRadius
        );
    }
}