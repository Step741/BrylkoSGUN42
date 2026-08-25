using UnityEngine;

public interface IPickupFactory
{
    GameObject CreateHealth(Vector3 position);

    GameObject CreateRifleAmmo(Vector3 position);

    GameObject CreateShotgunAmmo(Vector3 position);

    GameObject CreateGrenadeLauncherAmmo(Vector3 position);

    GameObject CreateRifle(Vector3 position);

    GameObject CreateShotgun(Vector3 position);

    GameObject CreateGrenadeLauncher(Vector3 position);

    GameObject CreateRailgun(Vector3 position);
}