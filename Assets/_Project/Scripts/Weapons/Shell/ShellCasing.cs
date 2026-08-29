using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ShellCasing : MonoBehaviour
{
    [Header("Impact Sounds")]
    [SerializeField]
    private AudioClip[] impactSounds;

    [SerializeField]
    [Range(0f, 1f)]
    private float impactVolume = 1f;

    [SerializeField]
    private Vector2 pitchRange =
        new Vector2(0.95f, 1.05f);


    [Header("Impact Detection")]
    [SerializeField]
    private float minimumImpactVelocity = 1f;

    [SerializeField]
    private float impactCooldown = 0.1f;


    [Header("3D Sound")]
    [SerializeField]
    private float minDistance = 1f;

    [SerializeField]
    private float maxDistance = 12f;


    private ShellPool pool;
    private Rigidbody rb;

    private float nextImpactSoundTime;
    private float releaseTime;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void SetPool(ShellPool pool)
    {
        this.pool = pool;
    }


    public void Activate(
        Vector3 position,
        Quaternion rotation,
        float lifetime)
    {
        ResetPhysics();

        transform.SetPositionAndRotation(
            position,
            rotation
        );

        nextImpactSoundTime = 0f;

        releaseTime =
            Time.time + lifetime;
    }


    public Rigidbody GetRigidbody()
    {
        return rb;
    }


    private void Update()
    {
        if (releaseTime <= 0f)
            return;

        if (Time.time >= releaseTime)
        {
            ReleaseToPool();
        }
    }


    private void ReleaseToPool()
    {
        releaseTime = 0f;

        if (pool != null)
        {
            pool.Release(this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }


    private void ResetPhysics()
    {
        if (rb == null)
            return;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.Sleep();
    }


    private void OnDisable()
    {
        ResetPhysics();

        nextImpactSoundTime = 0f;
        releaseTime = 0f;
    }

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

        if (collision.contactCount > 0)
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