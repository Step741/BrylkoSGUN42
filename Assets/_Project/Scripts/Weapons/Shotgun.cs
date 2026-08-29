using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class Shotgun : WeaponBase
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

    [Header("Tracer Settings")]

    [SerializeField]
    [Min(0)]
    private int tracersPerShot = 3;

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

    private readonly HashSet<IStunnable>
        stunnedTargets =
            new HashSet<IStunnable>();

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


        if (playerCamera == null)
            return false;


        if (Time.time < nextFireTime)
            return false;

        currentAmmo--;

        NotifyAmmoChanged();

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

        stunnedTargets.Clear();

        FirePellets();


        return true;
    }

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
            Vector3 direction =
                GetPelletDirection();


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

                ApplyPelletDamage(
                    hit
                );

                ApplyKnockback(
                    hit,
                    direction
                );
            }

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
        }
    }

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