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

    private bool exploded;

    private Collider[] explosionResults;

    private readonly HashSet<IDamageable> damagedTargets =
        new HashSet<IDamageable>();


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }


    // =========================================================
    // POOL
    // =========================================================

    /// <summary>
    /// Передаёт снаряду ссылку на его пул.
    /// Вызывается ProjectilePool при создании снаряда.
    /// </summary>
    public void SetPool(
        ProjectilePool pool)
    {
        projectilePool = pool;
    }


    // =========================================================
    // INITIALIZE
    // =========================================================

    /// <summary>
    /// Инициализация гранаты перед выстрелом.
    /// </summary>
    public void Initialize(
        WeaponConfig weaponConfig,
        Vector3 direction)
    {
        config = weaponConfig;

        exploded = false;

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


        lifeTimer =
            config.ProjectileLifetime;


        rb.velocity =
            direction.normalized *
            config.ProjectileSpeed;


        rb.angularVelocity =
            Vector3.zero;
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (exploded)
            return;


        lifeTimer -=
            Time.deltaTime;


        if (lifeTimer <= 0f)
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


        Explode();
    }


    // =========================================================
    // EXPLOSION
    // =========================================================

    private void Explode()
    {
        if (exploded)
            return;


        exploded = true;


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
            //
            // Центр взрыва = 100%
            // Край радиуса = 0%
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


            // --------------------------------------------------
            // Взрывная сила
            // --------------------------------------------------

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
        rb.velocity =
            Vector3.zero;

        rb.angularVelocity =
            Vector3.zero;


        damagedTargets.Clear();


        if (projectilePool != null)
        {
            projectilePool.Release(
                this
            );

            return;
        }


        // Запасной вариант,
        // если объект почему-то был создан
        // не через пул.
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