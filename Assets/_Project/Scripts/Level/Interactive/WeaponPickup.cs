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

    [Header("Player")]
    [SerializeField]
    private WeaponSwitcher weaponSwitcher;


    protected override bool CanPickup(Transform player)
    {
        WeaponSwitcher switcher =
            FindWeaponSwitcher(player);

        if (switcher == null)
        {
            Debug.LogWarning(
                $"[{name}] WeaponSwitcher not found."
            );

            return false;
        }

        int weaponIndex =
            GetWeaponIndex();

        if (weaponIndex < 0)
            return false;

        // Уже подобрано.
        if (switcher.IsWeaponUnlocked(
                weaponIndex))
        {
            return false;
        }

        return true;
    }


    protected override void ApplyPickup()
    {
        if (PickupPlayer == null)
            return;

        WeaponSwitcher switcher =
            FindWeaponSwitcher(PickupPlayer);

        if (switcher == null)
        {
            Debug.LogWarning(
                $"[{name}] WeaponSwitcher not found."
            );

            return;
        }

        int weaponIndex =
            GetWeaponIndex();

        if (weaponIndex < 0)
            return;

        bool unlocked =
            switcher.UnlockWeapon(
                weaponIndex
            );

        if (!unlocked)
            return;

        // После подбора сразу экипируем оружие.
        switcher.EquipWeapon(
            weaponIndex
        );

        Debug.Log(
            $"[{name}] Weapon picked up: " +
            $"{weaponType}"
        );
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
                player.GetComponentInChildren<
                    WeaponSwitcher
                >(true);
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