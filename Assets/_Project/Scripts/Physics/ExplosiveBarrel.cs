using UnityEngine;

[RequireComponent(typeof(Health))]
public class ExplosiveBarrel : MonoBehaviour
{
    [Header("Explosion")]
    [SerializeField] private ExplosionDamage explosion;

    [Header("Chain Reaction")]
    [SerializeField] private float chainRadius = 12f;
    [SerializeField] private LayerMask barrelLayer;

    [Header("Effects")]
    [SerializeField] private GameObject explosionVfx;
    [SerializeField] private float vfxLifetime = 3f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip explosionSound;

    [Header("Debris")]
    [SerializeField] private GameObject debrisPrefab;
    [SerializeField] private float debrisLifetime = 5f;

    private Health health;
    private bool exploded;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        if (health == null)
            health = GetComponent<Health>();

        health.Died += OnDied;
    }

    private void OnDisable()
    {
        if (health != null)
            health.Died -= OnDied;
    }

    private void OnDied()
    {
        if (exploded)
            return;

        exploded = true;

        Explode();
    }

    private void Explode()
    {
        Vector3 explosionPosition = transform.position;

        // 1. Основной взрыв
        if (explosion != null)
        {
            explosion.Explode(explosionPosition);
        }

        // 2. VFX
        SpawnVfx(explosionPosition);

        // 3. Звук
        PlaySound();

        // 4. Цепная реакция
        TriggerChainReaction(explosionPosition);

        // 5. Отключаем коллайдеры бочки
        Collider[] colliders = GetComponentsInChildren<Collider>();

        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
        }

        // 6. Осколки
        SpawnDebris(explosionPosition);

        // 7. Уничтожаем бочку после окончания звука
        if (explosionSound != null)
        {
            Destroy(gameObject, explosionSound.length);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void TriggerChainReaction(Vector3 explosionPosition)
    {
        if (chainRadius <= 0f)
            return;

        Collider[] hits = Physics.OverlapSphere(
            explosionPosition,
            chainRadius,
            barrelLayer,
            QueryTriggerInteraction.Ignore
        );

        foreach (Collider hit in hits)
        {
            ExplosiveBarrel barrel =
                hit.GetComponentInParent<ExplosiveBarrel>();

            if (barrel == null)
                continue;

            // Не запускаем самого себя
            if (barrel == this)
                continue;

            // Уже взорвавшаяся бочка ничего не делает
            if (barrel.exploded)
                continue;

            barrel.TriggerChainExplosion();
        }
    }

    public void TriggerChainExplosion()
    {
        if (exploded)
            return;

        exploded = true;

        Explode();
    }

    private void SpawnVfx(Vector3 position)
    {
        if (explosionVfx == null)
            return;

        GameObject vfx =
            Instantiate(
                explosionVfx,
                position,
                Quaternion.identity
            );

        Destroy(vfx, vfxLifetime);
    }

    private void PlaySound()
    {
        if (explosionSound == null)
            return;

        AudioSource.PlayClipAtPoint(
            explosionSound,
            transform.position,
            1f
        );
    }

    private void SpawnDebris(Vector3 position)
    {
        if (debrisPrefab == null)
            return;

        GameObject debris =
            Instantiate(
                debrisPrefab,
                position,
                transform.rotation
            );

        Destroy(debris, debrisLifetime);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            chainRadius
        );
    }
}