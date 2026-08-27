using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class Railgun : WeaponBase
{
    // =========================================================
    // REFERENCES
    // =========================================================

    [Header("References")]

    [SerializeField]
    private Transform muzzlePoint;

    [SerializeField]
    private LineRenderer beam;


    // =========================================================
    // WEAPON SOUNDS
    // =========================================================

    [Header("Weapon Sounds")]

    [SerializeField]
    private AudioClip shootSound;

    [SerializeField]
    private AudioClip overheatClickSound;


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
    // OVERHEAT CLICK
    // =========================================================

    [Header("Overheat Click")]

    [SerializeField]
    private float overheatClickCooldown = 0.3f;


    // =========================================================
    // EFFECTS
    // =========================================================

    [Header("Effects")]

    [SerializeField]
    private ParticleSystem muzzleFlash;


    // =========================================================
    // RECOIL
    // =========================================================

    [Header("Recoil")]

    [SerializeField]
    private WeaponRecoil weaponRecoil;


    // =========================================================
    // COMPONENTS
    // =========================================================

    private Camera playerCamera;

    private RaycastHit[] hitBuffer;


    // =========================================================
    // STATE
    // =========================================================

    private float nextFireTime;

    private float currentHeat;

    private int shotsSinceCooldown;

    private bool isOverheated;

    private bool shotPending;

    private Vector3 pendingFireOrigin;

    private Vector3 pendingFireDirection;

    private float nextOverheatClickTime;


    // =========================================================
    // DAMAGE TRACKING
    // =========================================================

    private readonly HashSet<IDamageable>
        damagedTargets =
            new HashSet<IDamageable>();


    // =========================================================
    // PROPERTIES
    // =========================================================

    public float CurrentHeat =>
        currentHeat;


    public float MaxHeat =>
        config != null
            ? config.RailgunMaxHeat
            : 0f;


    public float HeatNormalized =>
        MaxHeat > 0f
            ? currentHeat /
              MaxHeat
            : 0f;


    public bool IsOverheated =>
        isOverheated;


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


        hitBuffer =
            new RaycastHit[32];


        if (beam != null)
        {
            beam.positionCount =
                2;

            beam.useWorldSpace =
                true;

            beam.enabled =
                false;
        }


        if (playerCamera == null)
        {
            Debug.LogError(
                $"{name}: Player Camera is missing."
            );
        }
    }


    // =========================================================
    // SHOOT
    // =========================================================

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


        // =====================================================
        // OVERHEAT
        // =====================================================

        if (isOverheated)
        {
            PlayOverheatClick();

            return false;
        }


        // =====================================================
        // SHOT PENDING
        // =====================================================

        // Не запускаем новую стрельбу,
        // пока текущая анимация
        // не дошла до Animation Event.

        if (shotPending)
            return false;


        // =====================================================
        // FIRE RATE
        // =====================================================

        if (Time.time < nextFireTime)
            return false;


        // =====================================================
        // SAVE AIM
        // =====================================================

        // Запоминаем направление прицела
        // в момент нажатия кнопки.

        pendingFireOrigin =
            playerCamera.transform.position;

        pendingFireDirection =
            playerCamera.transform.forward;


        shotPending =
            true;


        nextFireTime =
            Time.time +
            config.RailgunChargeTime;


        return true;
    }


    // =========================================================
    // FIRE RAILGUN
    // =========================================================

    /// <summary>
    /// Вызывается Animation Event
    /// в момент фактического выстрела.
    /// </summary>
    public void FireRailgun()
    {
        if (!shotPending)
            return;


        shotPending =
            false;


        if (config == null)
            return;


        if (muzzlePoint == null)
            return;


        // =====================================================
        // CLEAR DAMAGED TARGETS
        // =====================================================

        damagedTargets.Clear();


        // =====================================================
        // RAYCAST
        // =====================================================

        int hitCount =
            Physics.RaycastNonAlloc(
                pendingFireOrigin,
                pendingFireDirection,
                hitBuffer,
                config.Range,
                config.RailgunHitMask,
                QueryTriggerInteraction.Collide
            );


        Array.Sort(
            hitBuffer,
            0,
            hitCount,
            RaycastHitDistanceComparer.Instance
        );


        int damagedCount =
            0;


        Vector3 beamEnd =
            pendingFireOrigin +
            pendingFireDirection *
            config.Range;


        for (
            int i = 0;
            i < hitCount;
            i++
        )
        {
            RaycastHit hit =
                hitBuffer[i];


            if (hit.collider == null)
                continue;


            // =================================================
            // SURFACE IMPACT
            // =================================================

            SurfaceImpactUtility.ProcessHit(
                hit
            );


            // =================================================
            // DAMAGEABLE
            // =================================================

            IDamageable damageable =
                hit.collider.GetComponentInParent<
                    IDamageable
                >();


            if (damageable == null)
                continue;


            // =================================================
            // ONE DAMAGE PER TARGET
            // =================================================

            if (
                !damagedTargets.Add(
                    damageable
                )
            )
            {
                continue;
            }


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
                damageable.TakeDamage(
                    config.Damage
                );
            }


            damagedCount++;


            // Последняя поражённая цель —
            // визуальный конец луча.

            beamEnd =
                hit.point;


            Debug.Log(
                $"Railgun hit: " +
                $"{hit.collider.name} " +
                $"distance: {hit.distance:F1}"
            );


            if (
                damagedCount >=
                config.RailgunMaxTargets
            )
            {
                break;
            }
        }


        // =====================================================
        // VISUAL BEAM
        // =====================================================

        FireVisual(
            beamEnd
        );


        // =====================================================
        // OVERHEAT
        // =====================================================

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


        if (shotsSinceCooldown >= 4)
        {
            StartOverheat();
        }


        // =====================================================
        // EFFECTS
        // =====================================================

        MuzzleFlashPool.Instance?.Play(
            muzzleFlash,
            muzzlePoint
        );


        PlaySound(
            shootSound
        );


        weaponRecoil?.AddRecoil();
    }


    // =========================================================
    // OVERHEAT
    // =========================================================

    private void StartOverheat()
    {
        isOverheated =
            true;


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
        yield return new WaitForSeconds(
            5f
        );


        currentHeat =
            0f;

        shotsSinceCooldown =
            0;

        isOverheated =
            false;


        Debug.Log(
            "Railgun cooled down. Ready."
        );
    }


    // =========================================================
    // VISUAL BEAM
    // =========================================================

    private void FireVisual(
        Vector3 endPoint)
    {
        if (beam == null)
            return;


        beam.positionCount =
            2;


        beam.SetPosition(
            0,
            muzzlePoint.position
        );


        beam.SetPosition(
            1,
            endPoint
        );


        // Показываем луч.

        beam.enabled =
            true;


        // Если предыдущий луч ещё отображается,
        // отменяем старый таймер скрытия.

        CancelInvoke(
            nameof(HideBeam)
        );


        // Новый таймер начинается заново.
        // Длительность настраивается
        // в WeaponConfig.

        Invoke(
            nameof(HideBeam),
            config.RailgunBeamDuration
        );
    }


    private void HideBeam()
    {
        if (beam != null)
        {
            beam.enabled =
                false;
        }
    }


    // =========================================================
    // RELOAD
    // =========================================================

    public override void Reload()
    {
        // Обычной перезарядки нет.
        // После четырёх выстрелов —
        // автоматическое охлаждение.
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
    // OVERHEAT CLICK
    // =========================================================

    private void PlayOverheatClick()
    {
        if (
            Time.time <
            nextOverheatClickTime
        )
        {
            return;
        }


        nextOverheatClickTime =
            Time.time +
            overheatClickCooldown;


        PlaySound(
            overheatClickSound
        );
    }


    // =========================================================
    // DISABLE
    // =========================================================

    private void OnDisable()
    {
        shotPending =
            false;

        currentHeat =
            0f;

        shotsSinceCooldown =
            0;

        isOverheated =
            false;

        nextOverheatClickTime =
            0f;


        CancelInvoke(
            nameof(HideBeam)
        );


        if (beam != null)
        {
            beam.enabled =
                false;
        }


        StopAllCoroutines();
    }


    // =========================================================
    // RAYCAST COMPARER
    // =========================================================

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