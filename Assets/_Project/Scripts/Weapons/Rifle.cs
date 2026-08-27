using UnityEngine;
using Zenject;

public class Rifle : WeaponBase
{
    // =========================================================
    // REFERENCES
    // =========================================================

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


    // =========================================================
    // WEAPON SOUNDS
    // =========================================================

    [Header("Weapon Sounds")]

    [SerializeField]
    private AudioClip shootSound;

    [SerializeField]
    private AudioClip emptyClickSound;

    [SerializeField]
    private AudioClip reloadSound;


    // =========================================================
    // SOUND SETTINGS
    // =========================================================

    [Header("Sound Settings")]

    [SerializeField]
    [Range(0f, 1f)]
    private float soundVolume = 1f;

    [SerializeField]
    private float soundPitch = 1f;


    // =========================================================
    // EMPTY CLICK
    // =========================================================

    [Header("Empty Click")]

    [SerializeField]
    private float emptyClickCooldown = 0.2f;


    // =========================================================
    // RECOIL
    // =========================================================

    [Header("Recoil")]

    [SerializeField]
    private WeaponRecoil weaponRecoil;

    [SerializeField]
    private CameraController cameraController;


    // =========================================================
    // COMPONENTS
    // =========================================================

    private Camera playerCamera;


    // =========================================================
    // STATE
    // =========================================================

    private float nextFireTime;

    private float nextEmptyClickTime;

    private float currentSpread;


    // =========================================================
    // INJECTION
    // =========================================================

    [Inject]
    private void Construct(
        Camera playerCamera)
    {
        this.playerCamera =
            playerCamera;
    }


    // =========================================================
    // UNITY
    // =========================================================

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


    // =========================================================
    // SHOOT
    // =========================================================

    public override bool Shoot()
    {
        // =====================================================
        // EMPTY MAGAZINE
        // =====================================================

        if (currentAmmo <= 0)
        {
            PlayEmptyClick();

            return false;
        }


        // =====================================================
        // CAN SHOOT
        // =====================================================

        if (!CanShoot)
            return false;


        if (playerCamera == null)
            return false;


        if (Time.time < nextFireTime)
            return false;


        // =====================================================
        // AMMO
        // =====================================================

        currentAmmo--;

        NotifyAmmoChanged();


        // =====================================================
        // SPREAD
        // =====================================================

        currentSpread =
            Mathf.Min(
                currentSpread +
                config.SpreadIncreasePerShot,
                config.MaxSpread
            );


        // =====================================================
        // MUZZLE FLASH
        // =====================================================

        MuzzleFlashPool.Instance?.Play(
            muzzleFlash,
            muzzlePoint
        );


        // =====================================================
        // SHELL EJECTION
        // =====================================================

        shellEjector?.Eject();


        // =====================================================
        // SHOOT SOUND
        // =====================================================

        PlaySound(
            shootSound
        );


        // =====================================================
        // WEAPON RECOIL
        // =====================================================

        weaponRecoil?.AddRecoil();


        // =====================================================
        // CAMERA RECOIL
        // =====================================================

        cameraController?.AddRecoil();


        // =====================================================
        // FIRE RATE
        // =====================================================

        nextFireTime =
            Time.time +
            1f /
            config.FireRate;


        // =====================================================
        // SPREAD DIRECTION
        // =====================================================

        Vector3 direction =
            GetSpreadDirection();


        // =====================================================
        // RAYCAST
        // =====================================================

        Ray ray =
            new Ray(
                playerCamera.transform.position,
                direction
            );


        // По умолчанию трассер летит
        // на максимальную дальность оружия.

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
            // =================================================
            // TRACER END POINT
            // =================================================

            tracerEndPoint =
                hit.point;


            // =================================================
            // SURFACE IMPACT
            // =================================================

            SurfaceImpactUtility.ProcessHit(
                hit
            );


            // =================================================
            // DAMAGE HITBOX
            // =================================================

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
                // =============================================
                // NORMAL DAMAGE
                // =============================================

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
                $"Rifle hit: {hit.collider.name}"
            );
        }


        // =====================================================
        // BULLET TRACER
        // =====================================================

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


        // =====================================================
        // DEBUG
        // =====================================================

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
    // SPREAD
    // =========================================================

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


        // =====================================================
        // INFINITE AMMO
        // =====================================================

        if (InfiniteAmmo)
        {
            currentAmmo =
                config.MagazineSize;

            NotifyAmmoChanged();


            PlaySound(
                reloadSound
            );


            Debug.Log(
                $"Rifle reload: {currentAmmo}/∞"
            );

            return;
        }


        // =====================================================
        // NORMAL AMMO
        // =====================================================

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
            $"Rifle reload: " +
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


    // =========================================================
    // EMPTY CLICK
    // =========================================================

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