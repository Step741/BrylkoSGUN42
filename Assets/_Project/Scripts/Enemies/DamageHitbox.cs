using UnityEngine;


public class DamageHitbox : MonoBehaviour
{
    // ==========================================
    // DAMAGE
    // ==========================================

    [Header("Damage")]

    [SerializeField]
    private float damageMultiplier = 1f;


    // ==========================================
    // HEALTH
    // ==========================================

    private Health health;


    // ==========================================
    // UNITY
    // ==========================================

    private void Awake()
    {
        health =
            GetComponentInParent<Health>();


        if (health == null)
        {
            Debug.LogError(
                $"[{name}] DamageHitbox could not find Health in parent."
            );
        }
    }


    // ==========================================
    // APPLY DAMAGE
    // ==========================================

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