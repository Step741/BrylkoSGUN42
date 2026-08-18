using UnityEngine;

public enum SurfaceType
{
    Concrete,
    Metal,
    Ground,
    Flesh
}

public class SurfaceIdentifier : MonoBehaviour
{
    [Header("Surface")]
    [SerializeField]
    private SurfaceType surfaceType =
        SurfaceType.Concrete;

    [Header("Impact VFX")]
    [SerializeField]
    private ParticleSystem impactVfx;

    [Header("Impact SFX")]
    [SerializeField]
    private AudioClip impactSound;

    [Header("Impact Decal")]
    [SerializeField]
    private GameObject decalPrefab;

    [SerializeField]
    private float decalLifetime = 8f;

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
        // SFX
        // ==========================================

        if (impactSound != null)
        {
            AudioSource.PlayClipAtPoint(
                impactSound,
                point
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
    }
}