using System.Collections.Generic;
using UnityEngine;

public class Katana : WeaponBase
{
    [Header("References")]
    [SerializeField]
    private Transform attackPoint;


    [Header("Weapon Sounds")]
    [SerializeField]
    private AudioClip swingSound;

    [SerializeField]
    private AudioClip hitSound;


    [Header("Sound Settings")]
    [SerializeField]
    [Range(0f, 1f)]
    private float soundVolume = 1f;

    [SerializeField]
    private float soundPitch = 1f;


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


    // =========================================================
    // SHOOT
    // =========================================================

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


    // =========================================================
    // ATTACK
    // =========================================================

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


        // =====================================================
        // SWING SOUND
        // =====================================================

        // Звук реального взмаха.
        PlaySound(
            swingSound
        );


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


        // =====================================================
        // HIT SOUND
        // =====================================================

        // Если катана задела хотя бы один объект,
        // проигрываем звук удара только один раз
        // за текущий взмах.
        bool hasHit = false;


        for (int i = 0; i < hitCount; i++)
        {
            if (hitBuffer[i] != null)
            {
                hasHit = true;
                break;
            }
        }


        if (hasHit)
        {
            PlaySound(
                hitSound
            );
        }


        // =====================================================
        // DAMAGE
        // =====================================================

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


            if (
                damagedCount >=
                config.KatanaMaxTargets
            )
            {
                break;
            }
        }
    }


    // =========================================================
    // RELOAD
    // =========================================================

    public override void Reload()
    {
        // Катана не перезаряжается.
    }


    // =========================================================
    // SOUND
    // =========================================================

    private void PlaySound(
        AudioClip clip)
    {
        if (clip == null)
            return;

        if (SoundService.Instance == null)
            return;


        SoundService.Instance.Play2D(
            clip,
            SoundType.SFX,
            soundVolume,
            soundPitch
        );
    }


    // =========================================================
    // DISABLE
    // =========================================================

    private void OnDisable()
    {
        attackPending = false;

        damagedTargets.Clear();
    }


    // =========================================================
    // GIZMOS
    // =========================================================

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