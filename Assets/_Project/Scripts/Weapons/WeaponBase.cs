using System;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour, IWeapon
{
    [Header("Weapon")]
    [SerializeField]
    protected WeaponConfig config;

    protected int currentAmmo;
    protected int reserveAmmo;


    // =========================
    // AMMO STATE
    // =========================

    public bool CanShoot =>
        currentAmmo > 0;

    public bool CanReload =>
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


    // =========================
    // EVENT
    // =========================

    public event Action<int, int> AmmoChanged;


    // =========================
    // INITIALIZATION
    // =========================

    protected virtual void Awake()
    {
        if (config == null)
        {
            Debug.LogError(
                $"{name}: WeaponConfig is missing.",
                this
            );

            return;
        }

        currentAmmo = config.MagazineSize;
        reserveAmmo = config.ReserveAmmo;

        NotifyAmmoChanged();
    }


    // =========================
    // AMMO NOTIFICATION
    // =========================

    protected void NotifyAmmoChanged()
    {
        AmmoChanged?.Invoke(
            currentAmmo,
            reserveAmmo
        );
    }


    // =========================
    // SET AMMO
    // =========================

    protected void SetAmmo(
        int magazineAmmo,
        int reserveAmmo)
    {
        currentAmmo =
            Mathf.Max(0, magazineAmmo);

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


    // =========================
    // ADD RESERVE AMMO
    // =========================

    public void AddReserveAmmo(int amount)
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

        Debug.Log(
            $"[{name}] Ammo added: +{amount}. " +
            $"Reserve: {reserveAmmo}/{maxReserve}"
        );
    }


    // =========================
    // WEAPON ACTIONS
    // =========================

    public abstract bool Shoot();

    public abstract void Reload();


    // =========================
    // EQUIP
    // =========================

    public virtual void Equip()
    {
        gameObject.SetActive(true);

        NotifyAmmoChanged();
    }


    // =========================
    // UNEQUIP
    // =========================

    public virtual void Unequip()
    {
        gameObject.SetActive(false);
    }
}