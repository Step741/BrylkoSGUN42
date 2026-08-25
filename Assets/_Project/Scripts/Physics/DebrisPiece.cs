using UnityEngine;

public class DebrisPiece : MonoBehaviour
{
    [Header("Explosion")]
    [SerializeField] private float explosionForce = 2.5f;
    [SerializeField] private float explosionRadius = 1.5f;
    [SerializeField] private float upwardModifier = 0.4f;


    [Header("Impact Sounds")]
    [SerializeField] private AudioClip[] impactSounds;

    [SerializeField]
    [Range(0f, 1f)]
    private float impactVolume = 1f;

    [SerializeField]
    private Vector2 pitchRange =
        new Vector2(0.95f, 1.05f);


    [Header("Impact Detection")]
    [SerializeField]
    private float minimumImpactVelocity = 1.5f;

    [SerializeField]
    private float impactCooldown = 0.15f;


    [Header("3D Sound")]
    [SerializeField]
    private float minDistance = 2f;

    [SerializeField]
    private float maxDistance = 15f;


    private Rigidbody rb;

    private float nextImpactSoundTime;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }


    // =========================================================
    // EXPLOSION
    // =========================================================

    public void Explode(Vector3 explosionPosition)
    {
        if (rb == null)
            return;

        rb.AddExplosionForce(
            explosionForce,
            explosionPosition,
            explosionRadius,
            upwardModifier,
            ForceMode.Impulse
        );
    }


    // =========================================================
    // COLLISION SOUND
    // =========================================================

    private void OnCollisionEnter(
        Collision collision)
    {
        if (Time.time < nextImpactSoundTime)
            return;

        float impactVelocity =
            collision.relativeVelocity.magnitude;

        if (
            impactVelocity <
            minimumImpactVelocity
        )
        {
            return;
        }

        PlayImpactSound(collision);
    }


    private void PlayImpactSound(
        Collision collision)
    {
        if (
            impactSounds == null ||
            impactSounds.Length == 0
        )
        {
            return;
        }

        if (SoundService.Instance == null)
            return;


        AudioClip clip =
            impactSounds[
                Random.Range(
                    0,
                    impactSounds.Length
                )
            ];

        if (clip == null)
            return;


        Vector3 impactPosition =
            transform.position;

        if (
            collision.contactCount > 0
        )
        {
            impactPosition =
                collision.GetContact(0).point;
        }


        float pitch =
            Random.Range(
                pitchRange.x,
                pitchRange.y
            );


        SoundService.Instance.Play3D(
            clip,
            impactPosition,
            SoundType.SFX,
            impactVolume,
            pitch,
            minDistance,
            maxDistance
        );


        nextImpactSoundTime =
            Time.time +
            impactCooldown;
    }
}