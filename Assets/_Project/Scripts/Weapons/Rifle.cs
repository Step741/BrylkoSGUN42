using UnityEngine;
using Zenject;

public class Rifle : WeaponBase
{
    [Header("References")]

    [SerializeField]
    private LayerMask hitMask;

    [SerializeField]
    private ParticleSystem muzzleFlash;

    [SerializeField]
    private Transform muzzlePoint;

    [SerializeField]
    private BulletTracer bulletTracer;

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

    [SerializeField]
    private CameraController cameraController;

    private Camera playerCamera;

    private float nextFireTime;

    private float nextEmptyClickTime;

    private float currentSpread;

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


        currentSpread =
            config != null
                ? config.Spread
                : 0f;
    }


    private void Update()
    {
        RecoverSpread();
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


        if (playerCamera == null)
            return false;


        if (Time.time < nextFireTime)
            return false;

        currentAmmo--;

        NotifyAmmoChanged();

        currentSpread =
            Mathf.Min(
                currentSpread +
                config.SpreadIncreasePerShot,
                config.MaxSpread
            );

        MuzzleFlashPool.Instance?.Play(
            muzzleFlash,
            muzzlePoint
        );

        shellEjector?.Eject();

        PlaySound(
            shootSound
        );

        weaponRecoil?.AddRecoil();

        cameraController?.AddRecoil();

        nextFireTime =
            Time.time +
            1f /
            config.FireRate;

        Vector3 direction =
            GetSpreadDirection();

        Ray ray =
            new Ray(
                playerCamera.transform.position,
                direction
            );

        Vector3 tracerEndPoint =
            ray.origin +
            ray.direction *
            config.Range;


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
            tracerEndPoint =
                hit.point;

            SurfaceImpactUtility.ProcessHit(
                hit
            );

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
        }

        if (
            bulletTracer != null &&
            muzzlePoint != null
        )
        {
            BulletTracerPool.Instance?.Play(
                bulletTracer,
                muzzlePoint.position,
                tracerEndPoint
            );
        }

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


        currentSpread =
            Mathf.MoveTowards(
                currentSpread,
                config.Spread,
                config.SpreadRecoverySpeed *
                Time.deltaTime
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