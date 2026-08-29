using UnityEngine;

public class WeaponPickup : PickupBase
{
    public enum WeaponType
    {
        Rifle,
        Shotgun,
        GrenadeLauncher,
        Railgun
    }

    [Header("Weapon")]
    [SerializeField]
    private WeaponType weaponType;

    [Header("Duplicate Pickup")]
    [SerializeField]
    private int duplicateAmmoAmount = 30;

    [Header("Player")]
    [SerializeField]
    private WeaponSwitcher weaponSwitcher;

    protected override bool CanPickup(Transform player)
    {
        WeaponSwitcher switcher =
            FindWeaponSwitcher(player);

        if (switcher == null)
        {
            return false;
        }

        int weaponIndex =
            GetWeaponIndex();

        if (weaponIndex < 0)
            return false;

        if (!switcher.IsWeaponUnlocked(weaponIndex))
        {
            return true;
        }

        WeaponBase weapon =
            switcher.GetWeapon(weaponIndex);

        if (weapon == null)
        {
            return false;
        }


        //Бесконечные патроны, повторный подбор не нужен
        if (weapon.InfiniteAmmo)
            return false;


        //Запас уже максимальный
        if (weapon.ReserveAmmo >= weapon.MaxReserveAmmo)
            return false;


        return duplicateAmmoAmount > 0;
    }
    protected override void ApplyPickup()
    {
        if (PickupPlayer == null)
            return;


        WeaponSwitcher switcher =
            FindWeaponSwitcher(PickupPlayer);

        if (switcher == null)
        {
            return;
        }


        int weaponIndex =
            GetWeaponIndex();

        if (weaponIndex < 0)
            return;

        if (!switcher.IsWeaponUnlocked(weaponIndex))
        {
            bool unlocked =
                switcher.UnlockWeapon(weaponIndex);

            if (!unlocked)
                return;


            //После первого подбора сразу экипирует оружие
            switcher.EquipWeapon(weaponIndex);
            return;
        }

        WeaponBase weapon =
            switcher.GetWeapon(weaponIndex);

        if (weapon == null)
        {
            return;
        }


        if (weapon.InfiniteAmmo)
            return;


        int oldReserve =
            weapon.ReserveAmmo;


        weapon.AddReserveAmmo(
            duplicateAmmoAmount
        );


        int addedAmmo =
            weapon.ReserveAmmo - oldReserve;
    }

    private WeaponSwitcher FindWeaponSwitcher(
        Transform player)
    {
        if (weaponSwitcher != null)
            return weaponSwitcher;


        if (player == null)
            return null;


        WeaponSwitcher switcher =
            player.GetComponent<WeaponSwitcher>();


        if (switcher == null)
        {
            switcher =
                player.GetComponentInChildren<WeaponSwitcher>(
                    true
                );
        }


        return switcher;
    }

    private int GetWeaponIndex()
    {
        switch (weaponType)
        {
            case WeaponType.Rifle:
                return 2;

            case WeaponType.Shotgun:
                return 3;

            case WeaponType.GrenadeLauncher:
                return 4;

            case WeaponType.Railgun:
                return 5;
        }


        return -1;
    }
}