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


    [Header("Audio")]
    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private AudioClip shootSound;


    [Header("Recoil")]
    [SerializeField]
    private WeaponRecoil weaponRecoil;


    private Camera playerCamera;

    private float nextFireTime;

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


    public override bool Shoot()
    {
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


        // Патрон расходуем
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
        // Вспышка
        // ==================================================

        muzzleFlash?.Play();


        // ==================================================
        // Звук
        // ==================================================

        if (audioSource != null &&
            shootSound != null)
        {
            audioSource.PlayOneShot(
                shootSound
            );
        }


        // ==================================================
        // Отдача
        // ==================================================

        weaponRecoil?.AddRecoil();


        // ==================================================
        // Направление
        //
        // Берём именно в момент Animation Event,
        // чтобы не было проблемы первого выстрела
        // до поднятия оружия.
        // ==================================================

        Vector3 direction =
            playerCamera.transform.forward;


        Quaternion rotation =
            Quaternion.LookRotation(
                direction
            );


        // ==================================================
        // Получаем гранату из пула
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
        // Запускаем гранату
        // ==================================================

        projectile.Initialize(
            config,
            direction
        );
    }


    public override void Reload()
    {
        if (config == null)
            return;


        int missingAmmo =
            config.MagazineSize -
            currentAmmo;


        if (missingAmmo <= 0)
            return;


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


        Debug.Log(
            $"Grenade Launcher reload: " +
            $"{currentAmmo}/{reserveAmmo}"
        );
    }


    private void OnDisable()
    {
        shotPending = false;
    }
}