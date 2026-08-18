using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class Railgun : WeaponBase
{
    [Header("References")]
    [SerializeField]
    private Transform muzzlePoint;

    [SerializeField]
    private LineRenderer beam;

    [Header("Audio")]
    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private AudioClip shootSound;

    [Header("Effects")]
    [SerializeField]
    private ParticleSystem muzzleFlash;

    [Header("Recoil")]
    [SerializeField]
    private WeaponRecoil weaponRecoil;

    private Camera playerCamera;

    private RaycastHit[] hitBuffer;

    private float nextFireTime;

    private float currentHeat;

    // Количество выстрелов после последнего охлаждения.
    private int shotsSinceCooldown;

    // Рельсотрон перегрет.
    private bool isOverheated;

    // Ожидает Animation Event.
    private bool shotPending;

    // Запоминаем прицел в момент нажатия.
    private Vector3 pendingFireOrigin;

    private Vector3 pendingFireDirection;

    private readonly HashSet<IDamageable> damagedTargets =
        new HashSet<IDamageable>();

    public float CurrentHeat =>
        currentHeat;

    public float MaxHeat =>
        config != null
            ? config.RailgunMaxHeat
            : 0f;

    public float HeatNormalized =>
        MaxHeat > 0f
            ? currentHeat / MaxHeat
            : 0f;

    public bool IsOverheated =>
        isOverheated;

    [Inject]
    private void Construct(Camera playerCamera)
    {
        this.playerCamera = playerCamera;
    }

    protected override void Awake()
    {
        base.Awake();

        hitBuffer =
            new RaycastHit[32];

        if (beam != null)
        {
            beam.positionCount = 2;
            beam.useWorldSpace = true;
            beam.enabled = false;
        }

        if (playerCamera == null)
        {
            Debug.LogError(
                $"{name}: Player Camera is missing."
            );
        }
    }

    /// <summary>
    /// Вызывается WeaponController при нажатии ЛКМ.
    /// Настоящий выстрел происходит через Animation Event.
    /// </summary>
    public override bool Shoot()
    {
        if (config == null)
            return false;

        if (playerCamera == null)
            return false;

        if (muzzlePoint == null)
        {
            Debug.LogError(
                $"{name}: Muzzle Point is missing."
            );

            return false;
        }

        // Перегретый рельсотрон не стреляет.
        if (isOverheated)
            return false;

        // Не запускаем новую стрельбу,
        // пока текущая анимация не дошла до Event.
        if (shotPending)
            return false;

        // Небольшая задержка между выстрелами.
        if (Time.time < nextFireTime)
            return false;

        // Запоминаем направление прицела
        // именно в момент нажатия.
        pendingFireOrigin =
            playerCamera.transform.position;

        pendingFireDirection =
            playerCamera.transform.forward;

        shotPending = true;

        // ChargeTime теперь отвечает только
        // за задержку между выстрелами.
        nextFireTime =
            Time.time +
            config.RailgunChargeTime;

        return true;
    }

    /// <summary>
    /// Вызывается Animation Event
    /// в момент фактического выстрела.
    /// </summary>
    public void FireRailgun()
    {
        if (!shotPending)
            return;

        shotPending = false;

        if (config == null)
            return;

        if (muzzlePoint == null)
            return;

        damagedTargets.Clear();

        int hitCount =
            Physics.RaycastNonAlloc(
                pendingFireOrigin,
                pendingFireDirection,
                hitBuffer,
                config.Range,
                config.RailgunHitMask,
                QueryTriggerInteraction.Ignore
            );

        Array.Sort(
            hitBuffer,
            0,
            hitCount,
            RaycastHitDistanceComparer.Instance
        );

        int damagedCount = 0;

        Vector3 beamEnd =
            pendingFireOrigin +
            pendingFireDirection *
            config.Range;

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit =
                hitBuffer[i];

            if (hit.collider == null)
                continue;

            // Surface Impact
            SurfaceImpactUtility.ProcessHit(hit);

            IDamageable damageable =
                hit.collider.GetComponentInParent<
                    IDamageable
                >();

            if (damageable == null)
                continue;

            // Один объект получает урон
            // только один раз за выстрел.
            if (!damagedTargets.Add(
                    damageable))
            {
                continue;
            }

            damageable.TakeDamage(
                config.Damage
            );

            damagedCount++;

            beamEnd =
                hit.point;

            Debug.Log(
                $"Railgun hit: " +
                $"{hit.collider.name} " +
                $"distance: {hit.distance:F1}"
            );

            if (damagedCount >=
                config.RailgunMaxTargets)
            {
                break;
            }
        }

        // Визуальный луч.
        FireVisual(
            beamEnd
        );

        // =========================================
        // НАКОПЛЕНИЕ ПЕРЕГРЕВА
        // =========================================

        shotsSinceCooldown++;

        currentHeat =
            shotsSinceCooldown *
            config.RailgunHeatPerShot;

        currentHeat =
            Mathf.Min(
                currentHeat,
                config.RailgunMaxHeat
            );

        Debug.Log(
            $"Railgun shot " +
            $"{shotsSinceCooldown}/4 | " +
            $"Heat: {currentHeat}"
        );

        // Четвёртый выстрел вызывает перегрев.
        if (shotsSinceCooldown >= 4)
        {
            StartOverheat();
        }

        // Вспышка.
        muzzleFlash?.Play();

        // Звук.
        if (audioSource != null &&
            shootSound != null)
        {
            audioSource.PlayOneShot(
                shootSound
            );
        }

        // Отдача.
        weaponRecoil?.AddRecoil();
    }

    private void StartOverheat()
    {
        isOverheated = true;

        Debug.Log(
            "RAILGUN OVERHEATED! " +
            "Cooling for 5 seconds."
        );

        StartCoroutine(
            CooldownAfterOverheat()
        );
    }

    private IEnumerator CooldownAfterOverheat()
    {
        // Ровно 5 секунд перегрева.
        yield return new WaitForSeconds(5f);

        currentHeat = 0f;

        shotsSinceCooldown = 0;

        isOverheated = false;

        Debug.Log(
            "Railgun cooled down. Ready."
        );
    }

    private void FireVisual(
        Vector3 endPoint)
    {
        if (beam == null)
            return;

        beam.positionCount = 2;

        beam.SetPosition(
            0,
            muzzlePoint.position
        );

        beam.SetPosition(
            1,
            endPoint
        );

        beam.enabled = true;

        CancelInvoke(
            nameof(HideBeam)
        );

        Invoke(
            nameof(HideBeam),
            config.RailgunBeamDuration
        );
    }

    private void HideBeam()
    {
        if (beam != null)
        {
            beam.enabled = false;
        }
    }

    public override void Reload()
    {
        // Обычной перезарядки нет.
        // После четырёх выстрелов —
        // автоматическое охлаждение.
    }

    private void OnDisable()
    {
        shotPending = false;

        currentHeat = 0f;

        shotsSinceCooldown = 0;

        isOverheated = false;

        if (beam != null)
        {
            beam.enabled = false;
        }

        StopAllCoroutines();
    }

    private sealed class RaycastHitDistanceComparer
        : IComparer<RaycastHit>
    {
        public static readonly
            RaycastHitDistanceComparer Instance =
                new RaycastHitDistanceComparer();

        public int Compare(
            RaycastHit a,
            RaycastHit b)
        {
            return a.distance.CompareTo(
                b.distance
            );
        }
    }
}