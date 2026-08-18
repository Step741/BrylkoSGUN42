using UnityEngine;

[CreateAssetMenu(
    fileName = "PickupFactoryConfig",
    menuName = "Game/Pickups/Pickup Factory Config"
)]
public class PickupFactoryConfig : ScriptableObject
{
    [Header("Health")]
    public GameObject healthPickupPrefab;

    [Header("Ammo")]
    public GameObject rifleAmmoPrefab;
    public GameObject shotgunAmmoPrefab;
    public GameObject grenadeLauncherAmmoPrefab;

    [Header("Weapons")]
    public GameObject riflePickupPrefab;
    public GameObject shotgunPickupPrefab;
    public GameObject grenadeLauncherPickupPrefab;
    public GameObject railgunPickupPrefab;
}