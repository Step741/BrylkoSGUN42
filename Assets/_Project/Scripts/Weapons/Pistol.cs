using UnityEngine;
using Zenject;

public class Pistol : WeaponBase
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

    private Camera playerCamera;

    private float nextFireTime;

    [Inject]
    private void Construct(Camera playerCamera)
    {
        this.playerCamera = playerCamera;
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

        // Muzzle Flash
        muzzleFlash?.Play();

        // Shoot Sound
        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        // Recoil
        weaponRecoil?.AddRecoil();

        nextFireTime =
            Time.time + 1f / config.FireRate;

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
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
                $"Pistol hit: {hit.collider.name}"
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

    public override void Reload()
    {
        int missingAmmo =
            config.MagazineSize - currentAmmo;

        if (missingAmmo <= 0)
            return;

        // Бесконечный боезапас
        if (InfiniteAmmo)
        {
            currentAmmo =
                config.MagazineSize;

            NotifyAmmoChanged();

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

        currentAmmo += ammoToLoad;
        reserveAmmo -= ammoToLoad;

        NotifyAmmoChanged();

        Debug.Log(
            $"Pistol reload: {currentAmmo}/{reserveAmmo}"
        );
    }
}