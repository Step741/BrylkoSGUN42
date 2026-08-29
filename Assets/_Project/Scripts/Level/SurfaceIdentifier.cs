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
    private float decalLifetime =
        8f;

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
    private float impactVolume =
        1f;

    [SerializeField]
    private Vector2 pitchRange =
        new Vector2(
            0.95f,
            1.05f
        );

    [Header("3D Sound")]

    [SerializeField]
    private float minDistance =
        2f;

    [SerializeField]
    private float maxDistance =
        20f;

    public SurfaceType SurfaceType =>
        surfaceType;

    public void PlayImpact(
        Vector3 point,
        Vector3 normal,
        Transform hitTransform)
    {
        PlayImpactInternal(
            point,
            normal,
            hitTransform,
            impactVfx,
            decalPrefab,
            true
        );
    }

    public void PlayCustomImpact(
        Vector3 point,
        Vector3 normal,
        Transform hitTransform,
        ParticleSystem customImpactVfx,
        GameObject customDecalPrefab
    )
    {
        PlayImpactInternal(
            point,
            normal,
            hitTransform,
            customImpactVfx,
            customDecalPrefab,
            true
        );
    }

    private void PlayImpactInternal(
        Vector3 point,
        Vector3 normal,
        Transform hitTransform,
        ParticleSystem selectedImpactVfx,
        GameObject selectedDecalPrefab,
        bool playSound
    )
    {
        if (
            selectedImpactVfx != null &&
            ImpactVfxPool.Instance != null
        )
        {
            ImpactVfxPool.Instance.Get(
                selectedImpactVfx,
                point,
                Quaternion.LookRotation(
                    normal
                )
            );
        }

        if (
            selectedDecalPrefab != null &&
            DecalPool.Instance != null
        )
        {
            DecalPool.Instance.Get(
                selectedDecalPrefab,
                point +
                normal *
                0.002f,
                Quaternion.LookRotation(
                    normal
                ),
                decalLifetime,
                hitTransform
            );
        }

        if (playSound)
        {
            PlayImpactSound(
                point
            );
        }
    }

    private void PlayImpactSound(
        Vector3 point)
    {
        if (
            SoundService.Instance ==
            null
        )
        {
            return;
        }


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
        switch (
            surfaceType
        )
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