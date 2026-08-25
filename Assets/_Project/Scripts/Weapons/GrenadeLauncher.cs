using UnityEngine;
using Zenject;

public class GrenadeLauncher : WeaponBase
{
    [Header("References")]
    [SerializeField]
    private Transform muzzlePoint;

    [SerializeField]
    private ParticleSystem muzzleFlash;

    [SerializeField]
    private ProjectilePool projectilePool;

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

    private bool shotPending;


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


        if (projectilePool == null)
        {
            Debug.LogError(
                $"{name}: Projectile Pool is missing."
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


        if (config == null)
            return false;


        if (config.ProjectilePrefab == null)
        {
            Debug.LogError(
                $"{name}: Projectile Prefab is missing in WeaponConfig."
            );

            return false;
        }


        if (muzzlePoint == null)
        {
            Debug.LogError(
                $"{name}: Muzzle Point is missing."
            );

            return false;
        }


        if (playerCamera == null)
            return false;


        if (projectilePool == null)
            return false;


        if (Time.time < nextFireTime)
            return false;


        if (shotPending)
            return false;


        // Патрон расходуется
        // в момент нажатия.
        currentAmmo--;

        NotifyAmmoChanged();


        // Настоящий выстрел произойдёт
        // через Animation Event.
        shotPending = true;


        nextFireTime =
            Time.time +
            1f / config.FireRate;


        return true;
    }


    // =========================================================
    // FIRE PROJECTILE
    // =========================================================

    /// <summary>
    /// Вызывается Animation Event
    /// в кадре фактического выстрела.
    /// </summary>
    public void FireProjectile()
    {
        if (!shotPending)
            return;


        shotPending = false;


        if (config == null)
            return;


        if (muzzlePoint == null)
            return;


        if (playerCamera == null)
            return;


        if (projectilePool == null)
            return;


        // ==================================================
        // MUZZLE FLASH
        // ==================================================

        muzzleFlash?.Play();


        // ==================================================
        // SHELL EJECTION
        // ==================================================

        shellEjector?.Eject();


        // ==================================================
        // SHOOT SOUND
        // ==================================================

        PlaySound(
            shootSound
        );


        // ==================================================
        // RECOIL
        // ==================================================

        weaponRecoil?.AddRecoil();


        // ==================================================
        // DIRECTION
        //
        // Берём направление именно в момент
        // Animation Event.
        // ==================================================

        Vector3 direction =
            playerCamera.transform.forward;


        Quaternion rotation =
            Quaternion.LookRotation(
                direction
            );


        // ==================================================
        // GET PROJECTILE
        // ==================================================

        GrenadeProjectile projectile =
            projectilePool.GetProjectile(
                muzzlePoint.position,
                rotation
            );


        if (projectile == null)
        {
            Debug.LogError(
                $"{name}: Failed to get projectile from pool."
            );

            return;
        }


        // ==================================================
        // FIRE PROJECTILE
        // ==================================================

        projectile.Initialize(
            config,
            direction
        );
    }


    // =========================================================
    // RELOAD
    // =========================================================

    public override void Reload()
    {
        if (config == null)
            return;


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
                $"Grenade Launcher reload: " +
                $"{currentAmmo}/∞"
            );

            return;
        }


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
            $"Grenade Launcher reload: " +
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
        if (Time.time < nextEmptyClickTime)
            return;


        nextEmptyClickTime =
            Time.time +
            emptyClickCooldown;


        PlaySound(
            emptyClickSound
        );
    }


    // =========================================================
    // DISABLE
    // =========================================================

    private void OnDisable()
    {
        shotPending = false;
    }
}