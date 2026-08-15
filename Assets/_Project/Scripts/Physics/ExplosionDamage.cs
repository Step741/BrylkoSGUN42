using UnityEngine;

public class ExplosionDamage : MonoBehaviour
{
    [Header("Explosion")]
    [SerializeField] private float radius = 5f;
    [SerializeField] private float damage = 50f;
    [SerializeField] private float force = 700f;

    [Header("Layers")]
    [SerializeField] private LayerMask damageMask;
    [SerializeField] private LayerMask physicsMask;

    public void Explode(Vector3 position)
    {
        ApplyDamage(position);
        ApplyPhysics(position);
    }

    private void ApplyDamage(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(
            position,
            radius,
            damageMask,
            QueryTriggerInteraction.Ignore
        );

        foreach (Collider collider in colliders)
        {
            IDamageable damageable =
                collider.GetComponentInParent<IDamageable>();

            if (damageable == null)
                continue;

            float distance =
                Vector3.Distance(position, collider.ClosestPoint(position));

            float normalizedDistance =
                Mathf.Clamp01(distance / radius);

            float finalDamage =
                Mathf.Lerp(damage, 0f, normalizedDistance);

            damageable.TakeDamage(finalDamage);
        }
    }

    private void ApplyPhysics(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(
            position,
            radius,
            physicsMask,
            QueryTriggerInteraction.Ignore
        );

        foreach (Collider collider in colliders)
        {
            Rigidbody rb =
                collider.attachedRigidbody;

            if (rb == null)
                continue;

            rb.AddExplosionForce(
                force,
                position,
                radius,
                1f,
                ForceMode.Impulse
            );
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            radius
        );
    }
}