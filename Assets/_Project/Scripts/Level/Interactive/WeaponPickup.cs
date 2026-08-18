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
    [Tooltip("Количество патронов, которое добавляется при повторном подборе оружия.")]
    [SerializeField]
    private int duplicateAmmoAmount = 30;

    [Header("Player")]
    [SerializeField]
    private WeaponSwitcher weaponSwitcher;


    // =========================================================
    // CAN PICKUP
    // =========================================================

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


        // =====================================================
        // FIRST PICKUP
        // =====================================================

        if (!switcher.IsWeaponUnlocked(weaponIndex))
        {
            return true;
        }


        // =====================================================
        // DUPLICATE PICKUP
        // =====================================================

        WeaponBase weapon =
            switcher.GetWeapon(weaponIndex);

        if (weapon == null)
        {
            Debug.LogWarning(
                $"[{name}] Weapon at index {weaponIndex} not found."
            );

            return false;
        }


        // Бесконечные патроны — повторный подбор не нужен.
        if (weapon.InfiniteAmmo)
            return false;


        // Запас уже максимальный.
        if (weapon.ReserveAmmo >= weapon.MaxReserveAmmo)
            return false;


        return duplicateAmmoAmount > 0;
    }


    // =========================================================
    // APPLY PICKUP
    // =========================================================

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


        // =====================================================
        // FIRST PICKUP
        // =====================================================

        if (!switcher.IsWeaponUnlocked(weaponIndex))
        {
            bool unlocked =
                switcher.UnlockWeapon(weaponIndex);

            if (!unlocked)
                return;


            // После первого подбора сразу экипируем оружие.
            switcher.EquipWeapon(weaponIndex);

            Debug.Log(
                $"[{name}] Weapon picked up: {weaponType}"
            );

            return;
        }


        // =====================================================
        // DUPLICATE PICKUP → AMMO
        // =====================================================

        WeaponBase weapon =
            switcher.GetWeapon(weaponIndex);

        if (weapon == null)
        {
            Debug.LogWarning(
                $"[{name}] Weapon at index {weaponIndex} not found."
            );

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


        Debug.Log(
            $"[{name}] Duplicate weapon pickup: " +
            $"{weaponType} → +{addedAmmo} ammo. " +
            $"Reserve: {weapon.ReserveAmmo}/{weapon.MaxReserveAmmo}"
        );
    }


    // =========================================================
    // FIND WEAPON SWITCHER
    // =========================================================

    private WeaponSwitcher FindWeaponSwitcher(
        Transform player)
    {
        // Если ссылка задана вручную — используем её.
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


    // =========================================================
    // WEAPON INDEX
    // =========================================================

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