using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class GrenadeProjectile : MonoBehaviour
{
    private Rigidbody rb;

    private WeaponConfig config;

    private ProjectilePool projectilePool;

    private float lifeTimer;

    [Header("Explosion Delay")]

    [SerializeField]
    [Min(0f)]
    private float explosionDelayAfterFirstHit = 2f;

    private float explosionDelayTimer;

    private bool firstCollisionOccurred;

    private bool exploded;

    private Collider[] explosionResults;

    private readonly HashSet<IDamageable> damagedTargets =
        new HashSet<IDamageable>();

    private void Awake()
    {
        rb =
            GetComponent<Rigidbody>();
    }

    public void SetPool(
        ProjectilePool pool)
    {
        projectilePool =
            pool;
    }

    public void Initialize(
        WeaponConfig weaponConfig,
        Vector3 direction)
    {
        config =
            weaponConfig;

        exploded =
            false;

        firstCollisionOccurred =
            false;

        explosionDelayTimer =
            0f;

        damagedTargets.Clear();


        if (config == null)
        {
            return;
        }


        int maxTargets =
            Mathf.Max(
                1,
                config.MaxExplosionTargets
            );


        if (
            explosionResults == null ||
            explosionResults.Length != maxTargets
        )
        {
            explosionResults =
                new Collider[maxTargets];
        }

        lifeTimer =
            config.ProjectileLifetime;

        if (rb != null)
        {
            rb.velocity =
                Vector3.zero;

            rb.angularVelocity =
                Vector3.zero;


            //Запускает гранату
            rb.velocity =
                direction.normalized *
                config.ProjectileSpeed;
        }
    }

    private void Update()
    {
        if (exploded)
            return;

        lifeTimer -=
            Time.deltaTime;


        if (lifeTimer <= 0f)
        {
            Explode();

            return;
        }

        if (!firstCollisionOccurred)
            return;


        explosionDelayTimer -=
            Time.deltaTime;


        if (explosionDelayTimer <= 0f)
        {
            Explode();
        }
    }

    private void OnCollisionEnter(
        Collision collision)
    {
        if (exploded)
            return;

        if (!firstCollisionOccurred)
        {
            firstCollisionOccurred =
                true;

            explosionDelayTimer =
                explosionDelayAfterFirstHit;
        }
    }

    private void Explode()
    {
        if (exploded)
            return;


        exploded =
            true;


        if (config == null)
        {
            ReturnToPool();

            return;
        }


        Vector3 explosionPosition =
            transform.position;

        ExplosionVfxPool.Play(
            config.ExplosionVfxPrefab,
            explosionPosition,
            config.ExplosionVfxLifetime
        );


        if (
            SoundService.Instance != null &&
            config.ExplosionSound != null
        )
        {
            SoundService.Instance.Play3D(
                config.ExplosionSound,
                explosionPosition,
                SoundType.SFX
            );
        }

        int hitCount =
            Physics.OverlapSphereNonAlloc(
                explosionPosition,
                config.ExplosionRadius,
                explosionResults,
                config.ExplosionTargetMask,
                QueryTriggerInteraction.Ignore
            );


        for (int i = 0; i < hitCount; i++)
        {
            Collider targetCollider =
                explosionResults[i];


            if (targetCollider == null)
                continue;

            Vector3 targetPoint =
                targetCollider.ClosestPoint(
                    explosionPosition
                );


            float distance =
                Vector3.Distance(
                    explosionPosition,
                    targetPoint
                );

            if (
                Physics.Linecast(
                    explosionPosition,
                    targetPoint,
                    config.ExplosionObstacleMask,
                    QueryTriggerInteraction.Ignore
                )
            )
            {
                continue;
            }

            IDamageable damageable =
                targetCollider.GetComponentInParent<
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

            float damageMultiplier =
                1f -
                Mathf.Clamp01(
                    distance /
                    config.ExplosionRadius
                );


            float finalDamage =
                config.ExplosionDamage *
                damageMultiplier;

            PlayerAudioEffects playerAudioEffects =
                targetCollider.GetComponentInParent<
                    PlayerAudioEffects
                >();


            if (playerAudioEffects != null)
            {
                playerAudioEffects.PlayExplosionEffect(
                    damageMultiplier
                );
            }

            damageable.TakeDamage(
                finalDamage
            );

            IStunnable stunnable =
                targetCollider.GetComponentInParent<
                    IStunnable
                >();


            if (stunnable != null)
            {
                stunnable.Stun();
            }

            Rigidbody targetRigidbody =
                targetCollider.GetComponentInParent<
                    Rigidbody
                >();


            if (targetRigidbody != null)
            {
                targetRigidbody.AddExplosionForce(
                    config.ExplosionForce,
                    explosionPosition,
                    config.ExplosionRadius,
                    config.ExplosionUpwardModifier,
                    ForceMode.Impulse
                );
            }
        }

        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (rb != null)
        {
            rb.velocity =
                Vector3.zero;

            rb.angularVelocity =
                Vector3.zero;
        }


        damagedTargets.Clear();

        firstCollisionOccurred =
            false;

        explosionDelayTimer =
            0f;


        if (projectilePool != null)
        {
            projectilePool.Release(
                this
            );

            return;
        }


        Destroy(
            gameObject
        );
    }

    //GIZMOS
    private void OnDrawGizmosSelected()
    {
        if (config == null)
            return;


        Gizmos.DrawWireSphere(
            transform.position,
            config.ExplosionRadius
        );
    }
}