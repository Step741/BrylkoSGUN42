using UnityEngine;

public class AmmoPickup : PickupBase
{
    public enum AmmoType
    {
        Rifle,
        Shotgun,
        GrenadeLauncher
    }

    [Header("Ammo")]
    [SerializeField]
    private AmmoType ammoType;

    [SerializeField]
    private int amount = 10;

    [Header("Player")]
    [SerializeField]
    private WeaponSwitcher weaponSwitcher;

    protected override bool CanPickup(Transform player)
    {
        WeaponBase weapon =
            FindWeapon(player);

        if (weapon == null)
        {
            Debug.LogWarning(
                $"[{name}] Weapon for ammo type " +
                $"{ammoType} not found."
            );

            return false;
        }

        if (weapon.InfiniteAmmo)
            return false;

        if (weapon.ReserveAmmo >= GetMaxReserve(weapon))
            return false;

        return true;
    }

    protected override void ApplyPickup()
    {
        if (PickupPlayer == null)
            return;

        WeaponBase weapon =
            FindWeapon(PickupPlayer);

        if (weapon == null)
        {
            Debug.LogWarning(
                $"[{name}] Weapon for ammo type " +
                $"{ammoType} not found."
            );

            return;
        }

        weapon.AddReserveAmmo(amount);

        Debug.Log(
            $"[{name}] Ammo pickup: " +
            $"{ammoType} +{amount}."
        );
    }

    private WeaponBase FindWeapon(
        Transform player)
    {
        WeaponSwitcher switcher =
            weaponSwitcher;

        if (switcher == null)
        {
            switcher =
                player.GetComponent<WeaponSwitcher>();

            if (switcher == null)
            {
                switcher =
                    player.GetComponentInChildren<
                        WeaponSwitcher
                    >(true);
            }
        }

        if (switcher == null)
        {
            Debug.LogWarning(
                $"[{name}] WeaponSwitcher not found."
            );

            return null;
        }

        WeaponBase[] weapons =
            switcher.GetComponentsInChildren<
                WeaponBase
            >(true);

        foreach (WeaponBase weapon in weapons)
        {
            if (weapon == null)
                continue;

            switch (ammoType)
            {
                case AmmoType.Rifle:

                    if (weapon is Rifle)
                        return weapon;

                    break;

                case AmmoType.Shotgun:

                    if (weapon is Shotgun)
                        return weapon;

                    break;

                case AmmoType.GrenadeLauncher:

                    if (weapon is GrenadeLauncher)
                        return weapon;

                    break;
            }
        }

        return null;
    }

    private int GetMaxReserve(WeaponBase weapon)
    {
        return weapon != null
            ? weapon.MaxReserveAmmo
            : 0;
    }

}