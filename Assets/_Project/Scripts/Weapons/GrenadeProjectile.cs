using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class GrenadeProjectile : MonoBehaviour
{
    // =========================================================
    // COMPONENTS
    // =========================================================

    private Rigidbody rb;


    // =========================================================
    // CONFIG
    // =========================================================

    private WeaponConfig config;


    // =========================================================
    // POOL
    // =========================================================

    private ProjectilePool projectilePool;


    // =========================================================
    // LIFETIME
    // =========================================================

    private float lifeTimer;


    // =========================================================
    // EXPLOSION DELAY
    // =========================================================

    [Header("Explosion Delay")]

    [SerializeField]
    [Min(0f)]
    private float explosionDelayAfterFirstHit = 2f;

    private float explosionDelayTimer;

    private bool firstCollisionOccurred;


    // =========================================================
    // STATE
    // =========================================================

    private bool exploded;


    // =========================================================
    // EXPLOSION
    // =========================================================

    private Collider[] explosionResults;

    private readonly HashSet<IDamageable> damagedTargets =
        new HashSet<IDamageable>();


    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        rb =
            GetComponent<Rigidbody>();
    }


    // =========================================================
    // POOL
    // =========================================================

    public void SetPool(
        ProjectilePool pool)
    {
        projectilePool =
            pool;
    }


    // =========================================================
    // INITIALIZE
    // =========================================================

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
            Debug.LogError(
                $"{name}: WeaponConfig is missing."
            );

            ReturnToPool();

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


        // =====================================================
        // MAXIMUM LIFETIME
        // =====================================================

        lifeTimer =
            config.ProjectileLifetime;


        // =====================================================
        // RESET PHYSICS
        // =====================================================

        if (rb != null)
        {
            rb.velocity =
                Vector3.zero;

            rb.angularVelocity =
                Vector3.zero;


            // Запускаем гранату.
            rb.velocity =
                direction.normalized *
                config.ProjectileSpeed;
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (exploded)
            return;


        // =====================================================
        // MAXIMUM LIFETIME
        // =====================================================

        lifeTimer -=
            Time.deltaTime;


        if (lifeTimer <= 0f)
        {
            Explode();

            return;
        }


        // =====================================================
        // EXPLOSION AFTER FIRST COLLISION
        // =====================================================

        if (!firstCollisionOccurred)
            return;


        explosionDelayTimer -=
            Time.deltaTime;


        if (explosionDelayTimer <= 0f)
        {
            Explode();
        }
    }


    // =========================================================
    // COLLISION
    // =========================================================

    private void OnCollisionEnter(
        Collision collision)
    {
        if (exploded)
            return;


        // При первом касании поверхности
        // запускаем таймер взрыва.

        if (!firstCollisionOccurred)
        {
            firstCollisionOccurred =
                true;

            explosionDelayTimer =
                explosionDelayAfterFirstHit;
        }


        // Никакого Explode() здесь нет.
        // Rigidbody + Physic Material сами отвечают за отскок.
    }


    // =========================================================
    // EXPLOSION
    // =========================================================

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


        // ==================================================
        // VFX
        // ==================================================

        ExplosionVfxPool.Play(
            config.ExplosionVfxPrefab,
            explosionPosition,
            config.ExplosionVfxLifetime
        );


        // ==================================================
        // EXPLOSION SOUND
        // ==================================================

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


        // ==================================================
        // FIND TARGETS
        // ==================================================

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


            // --------------------------------------------------
            // Ближайшая точка объекта к взрыву
            // --------------------------------------------------

            Vector3 targetPoint =
                targetCollider.ClosestPoint(
                    explosionPosition
                );


            float distance =
                Vector3.Distance(
                    explosionPosition,
                    targetPoint
                );


            // --------------------------------------------------
            // Проверка препятствия
            // --------------------------------------------------

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


            // --------------------------------------------------
            // Получаем объект, способный получать урон
            // --------------------------------------------------

            IDamageable damageable =
                targetCollider.GetComponentInParent<
                    IDamageable
                >();


            if (damageable == null)
                continue;


            // --------------------------------------------------
            // Один объект получает урон только один раз
            // --------------------------------------------------

            if (
                !damagedTargets.Add(
                    damageable
                )
            )
            {
                continue;
            }


            // --------------------------------------------------
            // Урон зависит от расстояния
            // --------------------------------------------------

            float damageMultiplier =
                1f -
                Mathf.Clamp01(
                    distance /
                    config.ExplosionRadius
                );


            float finalDamage =
                config.ExplosionDamage *
                damageMultiplier;


            // ==================================================
            // EXPLOSION AUDIO EFFECT
            // ==================================================

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


            // ==================================================
            // DAMAGE
            // ==================================================

            damageable.TakeDamage(
                finalDamage
            );


            // ==================================================
            // STUN
            // ==================================================

            IStunnable stunnable =
                targetCollider.GetComponentInParent<
                    IStunnable
                >();


            if (stunnable != null)
            {
                stunnable.Stun();
            }


            // ==================================================
            // EXPLOSION FORCE
            // ==================================================

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


        // ==================================================
        // RETURN TO POOL
        // ==================================================

        ReturnToPool();
    }


    // =========================================================
    // RETURN TO POOL
    // =========================================================

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


    // =========================================================
    // GIZMOS
    // =========================================================

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