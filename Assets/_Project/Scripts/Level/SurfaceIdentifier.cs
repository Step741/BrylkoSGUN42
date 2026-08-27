using UnityEngine;

public class SurfaceIdentifier : MonoBehaviour
{
    // =========================================================
    // SURFACE
    // =========================================================

    [Header("Surface")]

    [SerializeField]
    private SurfaceType surfaceType =
        SurfaceType.Concrete;


    // =========================================================
    // IMPACT VFX
    // =========================================================

    [Header("Impact VFX")]

    [SerializeField]
    private ParticleSystem impactVfx;


    // =========================================================
    // IMPACT DECAL
    // =========================================================

    [Header("Impact Decal")]

    [SerializeField]
    private GameObject decalPrefab;

    [SerializeField]
    private float decalLifetime =
        8f;


    // =========================================================
    // IMPACT SOUNDS
    // =========================================================

    [Header("Impact Sounds")]

    [SerializeField]
    private AudioClip concreteImpactSound;

    [SerializeField]
    private AudioClip metalImpactSound;

    [SerializeField]
    private AudioClip fleshImpactSound;


    // =========================================================
    // SOUND SETTINGS
    // =========================================================

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


    // =========================================================
    // 3D SOUND
    // =========================================================

    [Header("3D Sound")]

    [SerializeField]
    private float minDistance =
        2f;

    [SerializeField]
    private float maxDistance =
        20f;


    // =========================================================
    // PROPERTIES
    // =========================================================

    public SurfaceType SurfaceType =>
        surfaceType;


    // =========================================================
    // PLAY IMPACT
    // =========================================================

    public void PlayImpact(
        Vector3 point,
        Vector3 normal,
        Transform hitTransform)
    {
        // =====================================================
        // VFX
        // =====================================================

        if (
            impactVfx != null &&
            ImpactVfxPool.Instance != null
        )
        {
            ImpactVfxPool.Instance.Get(
                impactVfx,
                point,
                Quaternion.LookRotation(
                    normal
                )
            );
        }


        // =====================================================
        // DECAL
        // =====================================================

        if (
            decalPrefab != null &&
            DecalPool.Instance != null
        )
        {
            DecalPool.Instance.Get(
                decalPrefab,
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


        // =====================================================
        // IMPACT SOUND
        // =====================================================

        PlayImpactSound(
            point
        );
    }


    // =========================================================
    // IMPACT SOUND
    // =========================================================

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


    // =========================================================
    // GET IMPACT SOUND
    // =========================================================

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