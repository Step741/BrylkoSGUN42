using UnityEngine;
using Zenject;

public class Rifle : WeaponBase
{
    [Header("References")]
    [SerializeField]
    private LayerMask hitMask;

    [SerializeField]
    private ParticleSystem muzzleFlash;

    [Header("Audio")]
    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private AudioClip shootSound;

    [Header("Recoil")]
    [SerializeField]
    private WeaponRecoil weaponRecoil;

    [SerializeField]
    private CameraController cameraController;

    private Camera playerCamera;

    private float nextFireTime;

    private float currentSpread;

    [Inject]
    private void Construct(Camera playerCamera)
    {
        this.playerCamera = playerCamera;
    }

    protected override void Awake()
    {
        base.Awake();

        currentSpread =
            config != null
                ? config.Spread
                : 0f;

        if (playerCamera == null)
        {
            Debug.LogError(
                $"{name}: Player Camera is missing."
            );
        }

        if (cameraController == null)
        {
            Debug.LogWarning(
                $"{name}: CameraController is not assigned."
            );
        }
    }

    private void Update()
    {
        RecoverSpread();
    }

    public override bool Shoot()
    {
        if (!CanShoot)
            return false;

        if (playerCamera == null)
            return false;

        if (Time.time < nextFireTime)
            return false;

        currentAmmo--;

        NotifyAmmoChanged();

        // Увеличиваем разброс после каждого выстрела.
        currentSpread = Mathf.Min(
            currentSpread +
            config.SpreadIncreasePerShot,
            config.MaxSpread
        );

        // Muzzle Flash
        muzzleFlash?.Play();

        // Shoot Sound
        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        // Отдача оружия.
        weaponRecoil?.AddRecoil();

        // Отдача камеры.
        cameraController?.AddRecoil();

        nextFireTime =
            Time.time + 1f / config.FireRate;

        Vector3 direction =
            GetSpreadDirection();

        Ray ray = new Ray(
            playerCamera.transform.position,
            direction
        );

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            config.Range,
            hitMask,
            QueryTriggerInteraction.Ignore))
        {
            IDamageable damageable =
                hit.collider.GetComponentInParent<IDamageable>();

            if (damageable != null)
            {
                damageable.TakeDamage(
                    config.Damage
                );
            }

            Debug.Log(
                $"Rifle hit: {hit.collider.name}"
            );
        }

        Debug.DrawRay(
            ray.origin,
            ray.direction * config.Range,
            Color.red,
            1f
        );

        return true;
    }

    private Vector3 GetSpreadDirection()
    {
        Vector2 spreadOffset =
            Random.insideUnitCircle *
            Mathf.Tan(
                currentSpread *
                Mathf.Deg2Rad
            );

        Vector3 direction =
            playerCamera.transform.forward +
            playerCamera.transform.right *
            spreadOffset.x +
            playerCamera.transform.up *
            spreadOffset.y;

        return direction.normalized;
    }

    private void RecoverSpread()
    {
        if (config == null)
            return;

        currentSpread = Mathf.MoveTowards(
            currentSpread,
            config.Spread,
            config.SpreadRecoverySpeed *
            Time.deltaTime
        );
    }

    public override void Reload()
    {
        int missingAmmo =
            config.MagazineSize - currentAmmo;

        if (missingAmmo <= 0)
            return;

        if (reserveAmmo <= 0)
            return;

        int ammoToLoad =
            Mathf.Min(
                missingAmmo,
                reserveAmmo
            );

        currentAmmo += ammoToLoad;
        reserveAmmo -= ammoToLoad;

        NotifyAmmoChanged();

        Debug.Log(
            $"Rifle reload: {currentAmmo}/{reserveAmmo}"
        );
    }
}