using UnityEngine;

[RequireComponent(typeof(Health))]
public class ExplosiveBarrel : MonoBehaviour
{
    [Header("Explosion")]

    [SerializeField]
    private ExplosionDamage explosion;


    [Header("Explosion Audio Effect")]

    [SerializeField]
    private float audioEffectRadius = 10f;

    [SerializeField]
    private LayerMask playerLayer;


    [Header("Chain Reaction")]

    [SerializeField]
    private float chainRadius = 12f;

    [SerializeField]
    private LayerMask barrelLayer;


    [Header("Effects")]

    [SerializeField]
    private GameObject explosionVfx;

    [SerializeField]
    private float vfxLifetime = 3f;


    [Header("Explosion Sound")]

    [SerializeField]
    private AudioClip explosionSound;

    [SerializeField]
    [Range(0f, 1f)]
    private float soundVolume = 1f;

    [SerializeField]
    private float soundPitch = 1f;


    [Header("3D Sound")]

    [SerializeField]
    private float minDistance = 5f;

    [SerializeField]
    private float maxDistance = 30f;


    [Header("Debris")]

    [SerializeField]
    private GameObject debrisPrefab;

    [SerializeField]
    private float debrisLifetime = 5f;


    private Health health;

    private bool exploded;


    private void Awake()
    {
        health =
            GetComponent<Health>();
    }


    private void OnEnable()
    {
        if (health == null)
        {
            health =
                GetComponent<Health>();
        }


        health.Died +=
            OnDied;
    }


    private void OnDisable()
    {
        if (health != null)
        {
            health.Died -=
                OnDied;
        }
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
        Vector3 explosionPosition =
            transform.position;


        // ==================================================
        // 1. ОСНОВНОЙ ВЗРЫВ
        // ==================================================

        if (explosion != null)
        {
            explosion.Explode(
                explosionPosition
            );
        }


        // ==================================================
        // 2. EXPLOSION AUDIO EFFECT
        // ==================================================

        PlayExplosionAudioEffect(
            explosionPosition
        );


        // ==================================================
        // 3. VFX
        // ==================================================

        SpawnVfx(
            explosionPosition
        );


        // ==================================================
        // 4. ЗВУК
        // ==================================================

        PlaySound(
            explosionPosition
        );


        // ==================================================
        // 5. ЦЕПНАЯ РЕАКЦИЯ
        // ==================================================

        TriggerChainReaction(
            explosionPosition
        );


        // ==================================================
        // 6. ОТКЛЮЧАЕМ КОЛЛАЙДЕРЫ БОЧКИ
        // ==================================================

        Collider[] colliders =
            GetComponentsInChildren<
                Collider
            >();


        foreach (
            Collider collider
            in colliders
        )
        {
            collider.enabled =
                false;
        }


        // ==================================================
        // 7. ОСКОЛКИ
        // ==================================================

        SpawnDebris(
            explosionPosition
        );


        // ==================================================
        // 8. УНИЧТОЖАЕМ БОЧКУ
        // ==================================================

        Destroy(
            gameObject
        );
    }


    // =========================================================
    // EXPLOSION AUDIO EFFECT
    // =========================================================

    private void PlayExplosionAudioEffect(
        Vector3 explosionPosition)
    {
        if (audioEffectRadius <= 0f)
            return;


        Collider[] hits =
            Physics.OverlapSphere(
                explosionPosition,
                audioEffectRadius,
                playerLayer,
                QueryTriggerInteraction.Ignore
            );


        foreach (
            Collider hit
            in hits
        )
        {
            if (hit == null)
                continue;


            PlayerAudioEffects
                playerAudioEffects =
                    hit.GetComponentInParent<
                        PlayerAudioEffects
                    >();


            if (
                playerAudioEffects ==
                null
            )
            {
                continue;
            }


            Vector3 playerPoint =
                hit.ClosestPoint(
                    explosionPosition
                );


            float distance =
                Vector3.Distance(
                    explosionPosition,
                    playerPoint
                );


            float intensity =
                1f -
                Mathf.Clamp01(
                    distance /
                    audioEffectRadius
                );


            playerAudioEffects
                .PlayExplosionEffect(
                    intensity
                );


            // Игрок найден,
            // повторно искать не нужно.
            break;
        }
    }


    // =========================================================
    // CHAIN REACTION
    // =========================================================

    private void TriggerChainReaction(
        Vector3 explosionPosition)
    {
        if (chainRadius <= 0f)
            return;


        Collider[] hits =
            Physics.OverlapSphere(
                explosionPosition,
                chainRadius,
                barrelLayer,
                QueryTriggerInteraction.Ignore
            );


        foreach (
            Collider hit
            in hits
        )
        {
            ExplosiveBarrel barrel =
                hit.GetComponentInParent<
                    ExplosiveBarrel
                >();


            if (barrel == null)
                continue;


            // Не запускаем самого себя.
            if (barrel == this)
                continue;


            // Уже взорвавшаяся бочка
            // ничего не делает.
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


    // =========================================================
    // VFX
    // =========================================================

    private void SpawnVfx(
        Vector3 position)
    {
        if (explosionVfx == null)
            return;


        GameObject vfx =
            Instantiate(
                explosionVfx,
                position,
                Quaternion.identity
            );


        Destroy(
            vfx,
            vfxLifetime
        );
    }


    // =========================================================
    // SOUND
    // =========================================================

    private void PlaySound(
        Vector3 position)
    {
        if (explosionSound == null)
            return;


        if (SoundService.Instance == null)
            return;


        SoundService.Instance.Play3D(
            explosionSound,
            position,
            SoundType.SFX,
            soundVolume,
            soundPitch,
            minDistance,
            maxDistance
        );
    }


    // =========================================================
    // DEBRIS
    // =========================================================

    private void SpawnDebris(
        Vector3 position)
    {
        if (debrisPrefab == null)
            return;


        GameObject debris =
            Instantiate(
                debrisPrefab,
                position,
                transform.rotation
            );


        Destroy(
            debris,
            debrisLifetime
        );
    }


    // =========================================================
    // GIZMOS
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        Gizmos.color =
            Color.red;


        Gizmos.DrawWireSphere(
            transform.position,
            chainRadius
        );


        Gizmos.color =
            Color.cyan;


        Gizmos.DrawWireSphere(
            transform.position,
            audioEffectRadius
        );
    }
}