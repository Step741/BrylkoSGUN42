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


    // Цели, которые уже были оглушены
    // текущим выстрелом.
    private readonly HashSet<IStunnable>
        stunnedTargets =
            new HashSet<IStunnable>();


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
        PlaySound(shootSound);


        // Weapon Recoil
        weaponRecoil?.AddRecoil();


        // Camera Recoil
        cameraController?.AddRecoil();


        nextFireTime =
            Time.time +
            1f / config.FireRate;


        // Новая цель оглушения
        // для текущего выстрела.
        stunnedTargets.Clear();


        FirePellets();


        return true;
    }


    // =========================================================
    // PELLETS
    // =========================================================

    private void FirePellets()
    {
        for (
            int i = 0;
            i < config.Pellets;
            i++)
        {
            Vector3 direction =
                GetPelletDirection();


            Ray ray =
                new Ray(
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
                // Surface Impact
                SurfaceImpactUtility.ProcessHit(hit);


                ApplyPelletDamage(hit);


                ApplyKnockback(
                    hit,
                    direction
                );
            }


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
        IDamageable damageable =
            hit.collider.GetComponentInParent<
                IDamageable
            >();


        if (damageable == null)
            return;


        float distanceMultiplier =
            GetDamageMultiplier(
                hit.distance
            );


        float damage =
            config.Damage *
            distanceMultiplier;


        damageable.TakeDamage(
            damage
        );


        // ==========================================
        // STUN
        // ==========================================

        IStunnable stunnable =
            hit.collider.GetComponentInParent<
                IStunnable
            >();


        if (
            stunnable != null &&
            stunnedTargets.Add(stunnable))
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


        // Бесконечный боезапас
        if (InfiniteAmmo)
        {
            currentAmmo =
                config.MagazineSize;

            NotifyAmmoChanged();

            PlaySound(reloadSound);

            Debug.Log(
                $"Shotgun reload: " +
                $"{currentAmmo}/∞"
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


        PlaySound(reloadSound);


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


    private void PlayEmptyClick()
    {
        if (Time.time < nextEmptyClickTime)
            return;


        nextEmptyClickTime =
            Time.time +
            emptyClickCooldown;


        PlaySound(emptyClickSound);
    }
}