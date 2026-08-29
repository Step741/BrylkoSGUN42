using System.Collections;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class SpitProjectile : MonoBehaviour
{
    [Header("Projectile")]

    [SerializeField]
    private float lifetime = 5f;

    [Header("VFX")]

    [SerializeField]
    private ParticleSystem[] particleSystems;

    private Rigidbody rb;

    private SpitProjectilePool pool;

    private bool hasHit;
    private bool isReleased;

    private Transform ownerRoot;

    private Coroutine lifetimeCoroutine;

    private float damageAmount;

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

    public void SetPool(
        SpitProjectilePool newPool
    )
    {
        pool =
            newPool;
    }

    public void Launch(
        Vector3 direction,
        float speed,
        float damage,
        Transform owner
    )
    {
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

        if (lifetimeCoroutine != null)
        {
            StopCoroutine(
                lifetimeCoroutine
            );

            lifetimeCoroutine =
                null;
        }

        if (
            direction.sqrMagnitude <=
            0.001f
        )
        {
            Release();

            return;
        }


        direction.Normalize();

        transform.rotation =
            Quaternion.LookRotation(
                direction
            );

        if (rb != null)
        {
            rb.velocity =
                Vector3.zero;

            rb.angularVelocity =
                Vector3.zero;
        }

        ResetAndPlayVFX();

        if (rb != null)
        {
            rb.velocity =
                direction * speed;
        }

        lifetimeCoroutine =
            StartCoroutine(
                LifetimeRoutine()
            );
    }

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

    private IEnumerator LifetimeRoutine()
    {
        yield return new WaitForSeconds(
            lifetime
        );


        Release();
    }

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

    private void OnTriggerEnter(
        Collider other
    )
    {
        if (other == null)
            return;

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

    public void SetDamage(
        float damage
    )
    {
        damageAmount =
            damage;
    }

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