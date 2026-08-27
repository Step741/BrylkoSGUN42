using UnityEngine;
using Zenject;

public class Pistol : WeaponBase
{
    [Header("References")]

    [SerializeField]
    private LayerMask hitMask;

    [SerializeField]
    private ParticleSystem muzzleFlash;

    [SerializeField]
    private ShellEjector shellEjector;


    [Header("Weapon Sounds")]

    [SerializeField]
    private AudioClip shootSound;

    [SerializeField]
    private AudioClip emptyClickSound;

    [SerializeField]
    private AudioClip reloadSound;


    [Header("Sound Settings")]

    [SerializeField]
    [Range(0f, 1f)]
    private float soundVolume = 1f;

    [SerializeField]
    private float soundPitch = 1f;


    [Header("Empty Click")]

    [SerializeField]
    private float emptyClickCooldown = 0.2f;


    [Header("Recoil")]

    [SerializeField]
    private WeaponRecoil weaponRecoil;


    private Camera playerCamera;

    private float nextFireTime;
    private float nextEmptyClickTime;


    [Inject]
    private void Construct(
        Camera playerCamera)
    {
        this.playerCamera =
            playerCamera;
    }


    protected override void Awake()
    {
        base.Awake();


        if (playerCamera == null)
        {
            Debug.LogError(
                $"{name}: Player Camera is missing."
            );
        }
    }


    // =========================================================
    // SHOOT
    // =========================================================

    public override bool Shoot()
    {
        // Пустой магазин
        if (currentAmmo <= 0)
        {
            PlayEmptyClick();

            return false;
        }


        if (!CanShoot)
            return false;


        if (playerCamera == null)
            return false;


        if (Time.time < nextFireTime)
            return false;


        currentAmmo--;

        NotifyAmmoChanged();


        // Muzzle Flash
        muzzleFlash?.Play();


        // Shell Ejection
        shellEjector?.Eject();


        // Shoot Sound
        PlaySound(
            shootSound
        );


        // Recoil
        weaponRecoil?.AddRecoil();


        nextFireTime =
            Time.time +
            1f / config.FireRate;


        Ray ray =
            new Ray(
                playerCamera.transform.position,
                playerCamera.transform.forward
            );


        if (
            Physics.Raycast(
                ray,
                out RaycastHit hit,
                config.Range,
                hitMask,
                QueryTriggerInteraction.Collide
            )
        )
        {
            // Surface Impact
            SurfaceImpactUtility.ProcessHit(
                hit
            );


            // ==========================================
            // DAMAGE HITBOX
            // ==========================================

            DamageHitbox hitbox =
                hit.collider.GetComponent<
                    DamageHitbox
                >();


            if (hitbox != null)
            {
                hitbox.ApplyDamage(
                    config.Damage,
                    transform.position
                );
            }
            else
            {
                // ==========================================
                // NORMAL DAMAGE
                // ==========================================

                IDamageable damageable =
                    hit.collider.GetComponentInParent<
                        IDamageable
                    >();


                if (damageable != null)
                {
                    damageable.TakeDamage(
                        config.Damage
                    );
                }
            }


            Debug.Log(
                $"Pistol hit: {hit.collider.name}"
            );
        }


        Debug.DrawRay(
            ray.origin,
            ray.direction *
            config.Range,
            Color.red,
            1f
        );


        return true;
    }


    // =========================================================
    // RELOAD
    // =========================================================

    public override void Reload()
    {
        int missingAmmo =
            config.MagazineSize -
            currentAmmo;


        if (missingAmmo <= 0)
            return;


        // Бесконечный боезапас
        if (InfiniteAmmo)
        {
            currentAmmo =
                config.MagazineSize;

            NotifyAmmoChanged();


            PlaySound(
                reloadSound
            );


            Debug.Log(
                $"Pistol reload: {currentAmmo}/∞"
            );

            return;
        }


        // Обычный боезапас
        if (reserveAmmo <= 0)
            return;


        int ammoToLoad =
            Mathf.Min(
                missingAmmo,
                reserveAmmo
            );


        currentAmmo +=
            ammoToLoad;

        reserveAmmo -=
            ammoToLoad;


        NotifyAmmoChanged();


        PlaySound(
            reloadSound
        );


        Debug.Log(
            $"Pistol reload: " +
            $"{currentAmmo}/{reserveAmmo}"
        );
    }


    // =========================================================
    // SOUND
    // =========================================================

    private void PlaySound(
        AudioClip clip)
    {
        if (clip == null)
            return;


        if (SoundService.Instance == null)
            return;


        SoundService.Instance.Play2D(
            clip,
            SoundType.SFX,
            soundVolume,
            soundPitch
        );
    }


    private void PlayEmptyClick()
    {
        if (
            Time.time <
            nextEmptyClickTime
        )
        {
            return;
        }


        nextEmptyClickTime =
            Time.time +
            emptyClickCooldown;


        PlaySound(
            emptyClickSound
        );
    }
}