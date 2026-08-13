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
        if (audioSource != null &&
            shootSound != null)
        {
            audioSource.PlayOneShot(
                shootSound
            );
        }

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

    private void FirePellets()
    {
        for (int i = 0;
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

        if (stunnable != null &&
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

        currentAmmo += ammoToLoad;
        reserveAmmo -= ammoToLoad;

        NotifyAmmoChanged();

        Debug.Log(
            $"Shotgun reload: " +
            $"{currentAmmo}/{reserveAmmo}"
        );
    }
}