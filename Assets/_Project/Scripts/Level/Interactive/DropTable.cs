using UnityEngine;

public class DropTable
{
    private readonly IPickupFactory pickupFactory;


    // =========================================================
    // CONSTRUCTOR
    // =========================================================

    public DropTable(IPickupFactory pickupFactory)
    {
        this.pickupFactory = pickupFactory;
    }


    // =========================================================
    // ROLL
    // =========================================================

    public GameObject Roll(
        DropConfig config,
        Vector3 position)
    {
        if (config == null)
        {
            Debug.LogWarning(
                "[DropTable] DropConfig is not assigned."
            );

            return null;
        }

        if (config.Drops == null ||
            config.Drops.Count == 0)
        {
            Debug.LogWarning(
                "[DropTable] DropConfig contains no drops."
            );

            return null;
        }


        // -----------------------------------------------------
        // Calculate total weight
        // -----------------------------------------------------

        float totalChance = 0f;

        foreach (DropConfig.DropEntry entry in config.Drops)
        {
            if (entry == null)
                continue;

            if (entry.chance <= 0f)
                continue;

            totalChance += entry.chance;
        }


        if (totalChance <= 0f)
        {
            Debug.LogWarning(
                "[DropTable] Total drop chance is 0."
            );

            return null;
        }


        // -----------------------------------------------------
        // Random roll
        // -----------------------------------------------------

        float roll =
            Random.Range(0f, totalChance);

        float currentChance = 0f;


        // -----------------------------------------------------
        // Find result
        // -----------------------------------------------------

        foreach (DropConfig.DropEntry entry in config.Drops)
        {
            if (entry == null)
                continue;

            if (entry.chance <= 0f)
                continue;


            currentChance += entry.chance;


            if (roll <= currentChance)
            {
                return CreatePickup(
                    entry.pickupType,
                    position
                );
            }
        }


        return null;
    }


    // =========================================================
    // CREATE PICKUP
    // =========================================================

    private GameObject CreatePickup(
        DropConfig.PickupType pickupType,
        Vector3 position)
    {
        switch (pickupType)
        {
            case DropConfig.PickupType.None:
                return null;


            case DropConfig.PickupType.Health:
                return pickupFactory.CreateHealth(
                    position
                );


            case DropConfig.PickupType.RifleAmmo:
                return pickupFactory.CreateRifleAmmo(
                    position
                );


            case DropConfig.PickupType.ShotgunAmmo:
                return pickupFactory.CreateShotgunAmmo(
                    position
                );


            case DropConfig.PickupType.GrenadeLauncherAmmo:
                return pickupFactory.CreateGrenadeLauncherAmmo(
                    position
                );


            case DropConfig.PickupType.Rifle:
                return pickupFactory.CreateRifle(
                    position
                );


            case DropConfig.PickupType.Shotgun:
                return pickupFactory.CreateShotgun(
                    position
                );


            case DropConfig.PickupType.GrenadeLauncher:
                return pickupFactory.CreateGrenadeLauncher(
                    position
                );


            case DropConfig.PickupType.Railgun:
                return pickupFactory.CreateRailgun(
                    position
                );


            default:
                Debug.LogWarning(
                    $"[DropTable] Unsupported pickup type: {pickupType}"
                );

                return null;
        }
    }
}