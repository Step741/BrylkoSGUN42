using System;
using System.Collections;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour, IWeapon
{
    [Header("Weapon")]

    [SerializeField]
    protected WeaponConfig config;


    protected int currentAmmo;
    protected int reserveAmmo;

    private Coroutine reloadCoroutine;

    public bool IsReloading { get; private set; }

    public bool CanShoot =>
        currentAmmo > 0 &&
        !IsReloading;


    public bool CanReload =>
        !IsReloading &&
        config != null &&
        currentAmmo < config.MagazineSize &&
        (InfiniteAmmo || reserveAmmo > 0);


    public int CurrentAmmo =>
        currentAmmo;


    public int ReserveAmmo =>
        reserveAmmo;


    public int MaxReserveAmmo =>
        config != null
            ? config.MaxReserveAmmo
            : 0;


    public int MagazineSize =>
        config != null
            ? config.MagazineSize
            : 0;


    public bool InfiniteAmmo =>
        config != null &&
        config.InfiniteAmmo;

    public event Action<int, int> AmmoChanged;

    protected virtual void Awake()
    {
        if (config == null)
        {
            return;
        }


        currentAmmo =
            config.MagazineSize;

        reserveAmmo =
            config.ReserveAmmo;


        NotifyAmmoChanged();
    }

    protected void NotifyAmmoChanged()
    {
        AmmoChanged?.Invoke(
            currentAmmo,
            reserveAmmo
        );
    }

    protected void SetAmmo(
        int magazineAmmo,
        int reserveAmmo)
    {
        currentAmmo =
            Mathf.Max(
                0,
                magazineAmmo
            );


        this.reserveAmmo =
            Mathf.Clamp(
                reserveAmmo,
                0,
                config != null
                    ? config.MaxReserveAmmo
                    : reserveAmmo
            );


        NotifyAmmoChanged();
    }

    public void AddReserveAmmo(
        int amount)
    {
        if (amount <= 0)
            return;

        if (InfiniteAmmo)
            return;


        int maxReserve =
            config != null
                ? config.MaxReserveAmmo
                : int.MaxValue;


        reserveAmmo =
            Mathf.Min(
                reserveAmmo + amount,
                maxReserve
            );


        NotifyAmmoChanged();
    }

    public abstract bool Shoot();

    public abstract void Reload();

    protected void StartReload(
        Action onComplete)
    {
        if (IsReloading)
            return;

        if (!CanReload)
            return;


        reloadCoroutine =
            StartCoroutine(
                ReloadRoutine(
                    onComplete
                )
            );
    }


    private IEnumerator ReloadRoutine(
        Action onComplete)
    {
        IsReloading = true;


        float reloadDuration =
            config != null
                ? config.ReloadDuration
                : 0f;


        if (reloadDuration > 0f)
        {
            yield return new WaitForSeconds(
                reloadDuration
            );
        }


        if (!isActiveAndEnabled)
        {
            IsReloading = false;

            reloadCoroutine = null;

            yield break;
        }


        onComplete?.Invoke();


        IsReloading = false;

        reloadCoroutine = null;
    }


    public void CancelReload()
    {
        if (
            reloadCoroutine != null
        )
        {
            StopCoroutine(
                reloadCoroutine
            );

            reloadCoroutine = null;
        }


        IsReloading = false;
    }

    public virtual void Equip()
    {
        gameObject.SetActive(
            true
        );


        NotifyAmmoChanged();
    }

    public virtual void Unequip()
    {
        CancelReload();


        gameObject.SetActive(
            false
        );
    }

    protected virtual void OnDisable()
    {
        CancelReload();
    }
}