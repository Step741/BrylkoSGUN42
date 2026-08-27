using System.Collections;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class SpitProjectile : MonoBehaviour
{
    // ==========================================
    // PROJECTILE
    // ==========================================

    [Header("Projectile")]

    [SerializeField]
    private float lifetime = 5f;


    // ==========================================
    // VFX
    // ==========================================

    [Header("VFX")]

    [SerializeField]
    private ParticleSystem[] particleSystems;


    // ==========================================
    // COMPONENTS
    // ==========================================

    private Rigidbody rb;


    // ==========================================
    // POOL
    // ==========================================

    private SpitProjectilePool pool;


    // ==========================================
    // STATE
    // ==========================================

    private bool hasHit;
    private bool isReleased;

    private Transform ownerRoot;

    private Coroutine lifetimeCoroutine;


    // ==========================================
    // DAMAGE
    // ==========================================

    private float damageAmount;


    // ==========================================
    // UNITY
    // ==========================================

    private void Awake()
    {
        rb =
            GetComponent<Rigidbody>();


        if (
            particleSystems == null ||
            particleSystems.Length == 0
        )
        {
            particleSystems =
                GetComponentsInChildren<ParticleSystem>(
                    true
                );
        }
    }


    private void OnDisable()
    {
        if (rb != null)
        {
            rb.velocity =
                Vector3.zero;

            rb.angularVelocity =
                Vector3.zero;
        }


        hasHit =
            false;


        ownerRoot =
            null;


        if (lifetimeCoroutine != null)
        {
            StopCoroutine(
                lifetimeCoroutine
            );

            lifetimeCoroutine =
                null;
        }


        StopVFX();
    }


    // ==========================================
    // POOL
    // ==========================================

    public void SetPool(
        SpitProjectilePool newPool
    )
    {
        pool =
            newPool;
    }


    // ==========================================
    // LAUNCH
    // ==========================================

    public void Launch(
        Vector3 direction,
        float speed,
        float damage,
        Transform owner
    )
    {
        // ==========================================
        // RESET STATE
        // ==========================================

        isReleased =
            false;


        hasHit =
            false;


        ownerRoot =
            owner != null
                ? owner.root
                : null;


        damageAmount =
            damage;


        // ==========================================
        // STOP OLD LIFETIME
        // ==========================================

        if (lifetimeCoroutine != null)
        {
            StopCoroutine(
                lifetimeCoroutine
            );

            lifetimeCoroutine =
                null;
        }


        // ==========================================
        // INVALID DIRECTION
        // ==========================================

        if (
            direction.sqrMagnitude <=
            0.001f
        )
        {
            Release();

            return;
        }


        direction.Normalize();


        // ==========================================
        // RESET ROTATION
        // ==========================================

        transform.rotation =
            Quaternion.LookRotation(
                direction
            );


        // ==========================================
        // STOP PHYSICS FIRST
        // ==========================================

        if (rb != null)
        {
            rb.velocity =
                Vector3.zero;

            rb.angularVelocity =
                Vector3.zero;
        }


        // ==========================================
        // FORCE RESET VFX
        // ==========================================

        ResetAndPlayVFX();


        // ==========================================
        // LAUNCH ONLY AFTER VFX RESET
        // ==========================================

        if (rb != null)
        {
            rb.velocity =
                direction * speed;
        }


        // ==========================================
        // START LIFETIME
        // ==========================================

        lifetimeCoroutine =
            StartCoroutine(
                LifetimeRoutine()
            );
    }


    // ==========================================
    // VFX RESET + PLAY
    // ==========================================

    private void ResetAndPlayVFX()
    {
        if (
            particleSystems == null ||
            particleSystems.Length == 0
        )
        {
            particleSystems =
                GetComponentsInChildren<ParticleSystem>(
                    true
                );
        }


        foreach (
            ParticleSystem particleSystem
            in particleSystems
        )
        {
            if (particleSystem == null)
                continue;


            particleSystem.Stop(
                true,
                ParticleSystemStopBehavior
                    .StopEmittingAndClear
            );


            particleSystem.Simulate(
                0f,
                true,
                true
            );


            particleSystem.Play(
                true
            );
        }
    }


    // ==========================================
    // STOP VFX
    // ==========================================

    private void StopVFX()
    {
        if (particleSystems == null)
            return;


        foreach (
            ParticleSystem particleSystem
            in particleSystems
        )
        {
            if (particleSystem == null)
                continue;


            particleSystem.Stop(
                true,
                ParticleSystemStopBehavior
                    .StopEmittingAndClear
            );


            particleSystem.Simulate(
                0f,
                true,
                true
            );
        }
    }


    // ==========================================
    // LIFETIME
    // ==========================================

    private IEnumerator LifetimeRoutine()
    {
        yield return new WaitForSeconds(
            lifetime
        );


        Release();
    }


    // ==========================================
    // COLLISION
    // ==========================================

    private void OnCollisionEnter(
        Collision collision
    )
    {
        if (
            collision == null ||
            collision.collider == null
        )
        {
            Release();

            return;
        }


        HandleHit(
            collision.collider
        );
    }


    // ==========================================
    // TRIGGER
    // ==========================================

    private void OnTriggerEnter(
        Collider other
    )
    {
        if (other == null)
            return;


        // Trigger-коллайдеры обрабатываем
        // только если это DamageHitbox.
        //
        // Это позволяет попадать в HeadHitbox,
        // но не реагировать на другие Trigger-зоны.

        DamageHitbox hitbox =
            other.GetComponent<
                DamageHitbox
            >();


        if (hitbox == null)
            return;


        HandleHit(
            other
        );
    }


    // ==========================================
    // HANDLE HIT
    // ==========================================

    private void HandleHit(
        Collider hitCollider
    )
    {
        if (
            hasHit ||
            isReleased
        )
        {
            return;
        }


        if (hitCollider == null)
        {
            Release();

            return;
        }


        // ==========================================
        // IGNORE OWNER
        // ==========================================

        if (
            ownerRoot != null &&
            hitCollider.transform.root ==
            ownerRoot
        )
        {
            return;
        }


        hasHit =
            true;


        Vector3 damageSourcePosition =
            ownerRoot != null
                ? ownerRoot.position
                : transform.position;


        // ==========================================
        // DAMAGE HITBOX
        // ==========================================

        DamageHitbox hitbox =
            hitCollider.GetComponent<
                DamageHitbox
            >();


        if (hitbox != null)
        {
            hitbox.ApplyDamage(
                damageAmount,
                damageSourcePosition
            );
        }
        else
        {
            // ==========================================
            // HEALTH
            // ==========================================

            Health health =
                hitCollider.GetComponentInParent<
                    Health
                >();


            if (health != null)
            {
                health.TakeDamage(
                    damageAmount,
                    damageSourcePosition
                );
            }
            else
            {
                // ==========================================
                // IDAMAGEABLE
                // ==========================================

                IDamageable damageable =
                    hitCollider.GetComponentInParent<
                        IDamageable
                    >();


                if (damageable != null)
                {
                    damageable.TakeDamage(
                        damageAmount
                    );
                }
            }
        }


        Release();
    }


    // ==========================================
    // DAMAGE
    // ==========================================

    public void SetDamage(
        float damage
    )
    {
        damageAmount =
            damage;
    }


    // ==========================================
    // RELEASE
    // ==========================================

    private void Release()
    {
        if (isReleased)
            return;


        isReleased =
            true;


        if (lifetimeCoroutine != null)
        {
            StopCoroutine(
                lifetimeCoroutine
            );

            lifetimeCoroutine =
                null;
        }


        if (rb != null)
        {
            rb.velocity =
                Vector3.zero;

            rb.angularVelocity =
                Vector3.zero;
        }


        StopVFX();


        if (pool != null)
        {
            pool.Release(
                this
            );
        }
        else
        {
            gameObject.SetActive(
                false
            );
        }
    }
}