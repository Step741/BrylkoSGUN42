using UnityEngine;


public class DamageHitbox : MonoBehaviour
{
    [Header("Damage")]

    [SerializeField]
    private float damageMultiplier = 1f;

    private Health health;

    private void Awake()
    {
        health =
            GetComponentInParent<Health>();
    }

    public void ApplyDamage(
        float damage,
        Vector3 damageSourcePosition
    )
    {
        if (health == null)
            return;


        float finalDamage =
            damage *
            damageMultiplier;


        health.TakeDamage(
            finalDamage,
            damageSourcePosition
        );
    }
}