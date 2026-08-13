using System;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour, IWeapon
{
    [Header("Weapon")]
    [SerializeField]
    protected WeaponConfig config;

    protected int currentAmmo;
    protected int reserveAmmo;

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
            Debug.LogError(
                $"{name}: WeaponConfig is missing."
            );

            return;
        }

        currentAmmo = config.MagazineSize;
        reserveAmmo = config.ReserveAmmo;

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
            Mathf.Max(0, magazineAmmo);

        this.reserveAmmo =
            Mathf.Max(0, reserveAmmo);

        NotifyAmmoChanged();
    }

    public abstract bool Shoot();

    public abstract void Reload();

    public virtual void Equip()
    {
        gameObject.SetActive(true);

        NotifyAmmoChanged();
    }

    public virtual void Unequip()
    {
        gameObject.SetActive(false);
    }
}