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

    }

    public override bool Shoot()
    {
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
            return false;
        }


        if (muzzlePoint == null)
        {
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

        currentAmmo--;

        NotifyAmmoChanged();


        //Выстрел произойдёт через Animation Event
        shotPending = true;


        nextFireTime =
            Time.time +
            1f / config.FireRate;


        return true;
    }

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

        MuzzleFlashPool.Instance?.Play(
            muzzleFlash,
            muzzlePoint
        );

        shellEjector?.Eject();

        PlaySound(
            shootSound
        );

        weaponRecoil?.AddRecoil();

        Vector3 direction =
            playerCamera.transform.forward;


        Quaternion rotation =
            Quaternion.LookRotation(
                direction
            );

        GrenadeProjectile projectile =
            projectilePool.GetProjectile(
                muzzlePoint.position,
                rotation
            );


        if (projectile == null)
        {
            return;
        }

        projectile.Initialize(
            config,
            direction
        );
    }

    public override void Reload()
    {
        if (!CanReload)
            return;

        StartReload(
            CompleteReload
        );
    }


    private void CompleteReload()
    {
        int missingAmmo =
            config.MagazineSize -
            currentAmmo;


        if (missingAmmo <= 0)
            return;

        if (InfiniteAmmo)
        {
            currentAmmo =
                config.MagazineSize;

            NotifyAmmoChanged();


            PlaySound(
                reloadSound
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
    }

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

    protected override void OnDisable()
    {
        base.OnDisable();

        shotPending = false;
    }
}