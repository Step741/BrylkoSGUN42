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

    [Header("Katana Impact")]

    [SerializeField]
    private ParticleSystem katanaImpactVfx;

    [SerializeField]
    private GameObject katanaDecalPrefab;

    private float nextAttackTime;

    private Collider[] hitBuffer;

    private bool attackPending;

    private readonly HashSet<IDamageable> damagedTargets =
        new HashSet<IDamageable>();

    protected override void Awake()
    {
        base.Awake();

        hitBuffer =
            new Collider[32];
    }

    public override bool Shoot()
    {
        if (config == null)
            return false;


        //Катана не использует патроны
        if (attackPending)
            return false;


        if (
            Time.time <
            nextAttackTime
        )
        {
            return false;
        }


        if (attackPoint == null)
        {
            return false;
        }


        attackPending =
            true;


        nextAttackTime =
            Time.time +
            config.KatanaAttackCooldown;


        return true;
    }

public void Attack()
    {
        if (!attackPending)
            return;


        attackPending =
            false;


        if (config == null)
            return;


        if (attackPoint == null)
            return;

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

        Vector3 attackDirection =
            transform.forward;


        int damagedCount =
            0;

        bool hasValidHit =
            false;


        for (
            int i = 0;
            i < hitCount;
            i++
        )
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

            if (
                !damagedTargets.Add(
                    damageable
                )
            )
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

            if (dot <= 0f)
                continue;

            damageable.TakeDamage(
                config.Damage
            );

            ProcessSurfaceImpact(
                hit
            );


            hasValidHit =
                true;

            damagedCount++;

            if (
                damagedCount >=
                config.KatanaMaxTargets
            )
            {
                break;
            }
        }

        if (hasValidHit)
        {
            PlaySound(
                hitSound
            );
        }
    }

    private void ProcessSurfaceImpact(
        Collider targetCollider)
    {
        if (targetCollider == null)
            return;


        Vector3 attackOrigin =
            attackPoint.position;


        Vector3 closestPoint =
            targetCollider.ClosestPoint(
                attackOrigin
            );


        Vector3 direction =
            closestPoint -
            attackOrigin;


        float distance =
            direction.magnitude;

        if (distance <= 0.001f)
        {
            direction =
                transform.forward;

            distance =
                config.KatanaAttackRange;
        }
        else
        {
            direction /=
                distance;
        }


        float rayDistance =
            distance +
            0.1f;


        if (
            Physics.Raycast(
                attackOrigin,
                direction,
                out RaycastHit hit,
                rayDistance,
                config.KatanaAttackMask,
                QueryTriggerInteraction.Ignore
            )
        )
        {
            if (
                hit.collider ==
                targetCollider
            )
            {
                ProcessKatanaImpact(
                    hit.point,
                    hit.normal,
                    hit.collider.transform
                );

                return;
            }
        }

        SurfaceIdentifier surface =
            targetCollider.GetComponent<
                SurfaceIdentifier
            >();


        if (surface == null)
        {
            surface =
                targetCollider.GetComponentInParent<
                    SurfaceIdentifier
                >();
        }


        if (surface == null)
            return;


        Vector3 normal =
            (
                closestPoint -
                targetCollider.bounds.center
            ).normalized;


        if (
            normal.sqrMagnitude <=
            0.001f
        )
        {
            normal =
                -transform.forward;
        }


        surface.PlayCustomImpact(
            closestPoint,
            normal,
            targetCollider.transform,
            katanaImpactVfx,
            katanaDecalPrefab
        );
    }

    private void ProcessKatanaImpact(
        Vector3 point,
        Vector3 normal,
        Transform hitTransform)
    {
        SurfaceIdentifier surface =
            hitTransform.GetComponent<
                SurfaceIdentifier
            >();


        if (surface == null)
        {
            surface =
                hitTransform.GetComponentInParent<
                    SurfaceIdentifier
                >();
        }


        if (surface == null)
            return;


        surface.PlayCustomImpact(
            point,
            normal,
            hitTransform,
            katanaImpactVfx,
            katanaDecalPrefab
        );
    }

    public override void Reload()
    {
        //Не перезаряжается.
    }

    private void PlaySound(
        AudioClip clip)
    {
        if (clip == null)
            return;


        if (
            SoundService.Instance ==
            null
        )
        {
            return;
        }


        SoundService.Instance.Play2D(
            clip,
            SoundType.SFX,
            soundVolume,
            soundPitch
        );
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        attackPending =
            false;

        damagedTargets.Clear();
    }

    //GIZMOS
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