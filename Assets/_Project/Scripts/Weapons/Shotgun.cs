using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class Shotgun : WeaponBase
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
    // TRACER SETTINGS
    // =========================================================

    [Header("Tracer Settings")]

    [SerializeField]
    [Min(0)]
    private int tracersPerShot = 3;


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


    // =========================================================
    // STUNNED TARGETS
    // =========================================================

    // Цели, которые уже были оглушены
    // текущим выстрелом.

    private readonly HashSet<IStunnable>
        stunnedTargets =
            new HashSet<IStunnable>();


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
        // NEW STUN TARGETS
        // =====================================================

        stunnedTargets.Clear();


        // =====================================================
        // FIRE PELLETS
        // =====================================================

        FirePellets();


        return true;
    }


    // =========================================================
    // PELLETS
    // =========================================================

    private void FirePellets()
    {
        int tracersCreated =
            0;


        for (
            int i = 0;
            i < config.Pellets;
            i++
        )
        {
            // =================================================
            // PELLET DIRECTION
            // =================================================

            Vector3 direction =
                GetPelletDirection();


            Ray ray =
                new Ray(
                    playerCamera.transform.position,
                    direction
                );


            // =================================================
            // TRACER END POINT
            // =================================================

            Vector3 tracerEndPoint =
                ray.origin +
                ray.direction *
                config.Range;


            // =================================================
            // HIT
            // =================================================

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


                // =============================================
                // SURFACE IMPACT
                // =============================================

                SurfaceImpactUtility.ProcessHit(
                    hit
                );


                // =============================================
                // DAMAGE
                // =============================================

                ApplyPelletDamage(
                    hit
                );


                // =============================================
                // KNOCKBACK
                // =============================================

                ApplyKnockback(
                    hit,
                    direction
                );
            }


            // =================================================
            // BULLET TRACER
            // =================================================

            if (
                tracersCreated <
                tracersPerShot &&
                bulletTracer != null &&
                muzzlePoint != null
            )
            {
                BulletTracerPool.Instance?.Play(
                    bulletTracer,
                    muzzlePoint.position,
                    tracerEndPoint
                );


                tracersCreated++;
            }


            // =================================================
            // DEBUG
            // =================================================

            Debug.DrawRay(
                ray.origin,
                ray.direction *
                config.Range,
                Color.red,
                1f
            );
        }
    }


    // =========================================================
    // SPREAD
    // =========================================================

    private Vector3 GetPelletDirection()
    {
        Vector2 spreadOffset =
            Random.insideUnitCircle *
            Mathf.Tan(
                config.Spread *
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


    // =========================================================
    // DAMAGE
    // =========================================================

    private void ApplyPelletDamage(
        RaycastHit hit)
    {
        float distanceMultiplier =
            GetDamageMultiplier(
                hit.distance
            );


        float damage =
            config.Damage *
            distanceMultiplier;


        // =====================================================
        // DAMAGE HITBOX
        // =====================================================

        DamageHitbox hitbox =
            hit.collider.GetComponent<
                DamageHitbox
            >();


        if (hitbox != null)
        {
            hitbox.ApplyDamage(
                damage,
                transform.position
            );
        }
        else
        {
            // =================================================
            // NORMAL DAMAGE
            // =================================================

            IDamageable damageable =
                hit.collider.GetComponentInParent<
                    IDamageable
                >();


            if (damageable != null)
            {
                damageable.TakeDamage(
                    damage
                );
            }
        }


        // =====================================================
        // STUN
        // =====================================================

        IStunnable stunnable =
            hit.collider.GetComponentInParent<
                IStunnable
            >();


        if (
            stunnable != null &&
            stunnedTargets.Add(
                stunnable
            )
        )
        {
            stunnable.Stun();
        }
    }


    // =========================================================
    // DAMAGE MULTIPLIER
    // =========================================================

    private float GetDamageMultiplier(
        float distance)
    {
        float normalizedDistance =
            Mathf.Clamp01(
                distance /
                config.Range
            );


        return Mathf.Lerp(
            config.MaxDamageMultiplier,
            config.MinDamageMultiplier,
            normalizedDistance
        );
    }


    // =========================================================
    // KNOCKBACK
    // =========================================================

    private void ApplyKnockback(
        RaycastHit hit,
        Vector3 direction)
    {
        Rigidbody targetRigidbody =
            hit.collider.GetComponentInParent<
                Rigidbody
            >();


        if (targetRigidbody == null)
            return;


        targetRigidbody.AddForce(
            direction *
            config.KnockbackForce,
            ForceMode.Impulse
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
                $"Shotgun reload: " +
                $"{currentAmmo}/∞"
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
            $"Shotgun reload: " +
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