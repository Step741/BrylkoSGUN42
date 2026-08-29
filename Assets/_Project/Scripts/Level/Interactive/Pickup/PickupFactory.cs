using UnityEngine;

public class PickupFactory : IPickupFactory
{
    private readonly PickupFactoryConfig config;

    public PickupFactory(PickupFactoryConfig config)
    {
        this.config = config;
    }

    public GameObject CreateHealth(Vector3 position)
    {
        return Create(
            config.healthPickupPrefab,
            position,
            "Health"
        );
    }

    public GameObject CreateRifleAmmo(Vector3 position)
    {
        return Create(
            config.rifleAmmoPrefab,
            position,
            "Rifle Ammo"
        );
    }


    public GameObject CreateShotgunAmmo(Vector3 position)
    {
        return Create(
            config.shotgunAmmoPrefab,
            position,
            "Shotgun Ammo"
        );
    }


    public GameObject CreateGrenadeLauncherAmmo(
        Vector3 position)
    {
        return Create(
            config.grenadeLauncherAmmoPrefab,
            position,
            "Grenade Launcher Ammo"
        );
    }

    public GameObject CreateRifle(Vector3 position)
    {
        return Create(
            config.riflePickupPrefab,
            position,
            "Rifle"
        );
    }


    public GameObject CreateShotgun(Vector3 position)
    {
        return Create(
            config.shotgunPickupPrefab,
            position,
            "Shotgun"
        );
    }


    public GameObject CreateGrenadeLauncher(
        Vector3 position)
    {
        return Create(
            config.grenadeLauncherPickupPrefab,
            position,
            "Grenade Launcher"
        );
    }


    public GameObject CreateRailgun(Vector3 position)
    {
        return Create(
            config.railgunPickupPrefab,
            position,
            "Railgun"
        );
    }

    private GameObject Create(
        GameObject prefab,
        Vector3 position,
        string pickupName)
    {
        if (prefab == null)
        {
            return null;
        }

        return Object.Instantiate(
            prefab,
            position,
            Quaternion.identity
        );
    }
}