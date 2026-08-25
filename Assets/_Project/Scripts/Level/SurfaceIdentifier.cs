using UnityEngine;

public class SurfaceIdentifier : MonoBehaviour
{
    [Header("Surface")]
    [SerializeField]
    private SurfaceType surfaceType =
        SurfaceType.Concrete;


    [Header("Impact VFX")]
    [SerializeField]
    private ParticleSystem impactVfx;


    [Header("Impact Decal")]
    [SerializeField]
    private GameObject decalPrefab;

    [SerializeField]
    private float decalLifetime = 8f;


    [Header("Impact Sounds")]
    [SerializeField]
    private AudioClip concreteImpactSound;

    [SerializeField]
    private AudioClip metalImpactSound;

    [SerializeField]
    private AudioClip fleshImpactSound;


    [Header("Sound Settings")]
    [SerializeField]
    [Range(0f, 1f)]
    private float impactVolume = 1f;

    [SerializeField]
    private Vector2 pitchRange =
        new Vector2(0.95f, 1.05f);


    [Header("3D Sound")]
    [SerializeField]
    private float minDistance = 2f;

    [SerializeField]
    private float maxDistance = 20f;


    public SurfaceType SurfaceType =>
        surfaceType;


    public void PlayImpact(
        Vector3 point,
        Vector3 normal)
    {
        // ==========================================
        // VFX
        // ==========================================

        if (impactVfx != null)
        {
            ParticleSystem vfx =
                Instantiate(
                    impactVfx,
                    point,
                    Quaternion.LookRotation(normal)
                );

            Destroy(
                vfx.gameObject,
                3f
            );
        }


        // ==========================================
        // DECAL
        // ==========================================

        if (decalPrefab != null)
        {
            GameObject decal =
                Instantiate(
                    decalPrefab,
                    point + normal * 0.002f,
                    Quaternion.LookRotation(normal)
                );

            if (decalLifetime > 0f)
            {
                Destroy(
                    decal,
                    decalLifetime
                );
            }
        }


        // ==========================================
        // IMPACT SOUND
        // ==========================================

        PlayImpactSound(point);
    }


    private void PlayImpactSound(
        Vector3 point)
    {
        if (SoundService.Instance == null)
            return;


        AudioClip impactSound =
            GetImpactSound();

        if (impactSound == null)
            return;


        float pitch =
            Random.Range(
                pitchRange.x,
                pitchRange.y
            );


        SoundService.Instance.Play3D(
            impactSound,
            point,
            SoundType.SFX,
            impactVolume,
            pitch,
            minDistance,
            maxDistance
        );
    }


    private AudioClip GetImpactSound()
    {
        switch (surfaceType)
        {
            case SurfaceType.Concrete:
                return concreteImpactSound;

            case SurfaceType.Metal:
                return metalImpactSound;

            case SurfaceType.Flesh:
                return fleshImpactSound;

            default:
                return null;
        }
    }
}