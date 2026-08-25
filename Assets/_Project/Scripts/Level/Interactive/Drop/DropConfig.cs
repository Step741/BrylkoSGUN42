using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "DropConfig",
    menuName = "Game/Pickups/Drop Config"
)]
public class DropConfig : ScriptableObject
{
    [Serializable]
    public class DropEntry
    {
        public PickupType pickupType;

        [Min(0f)]
        public float chance = 10f;
    }

    public enum PickupType
    {
        None,

        Health,

        RifleAmmo,
        ShotgunAmmo,
        GrenadeLauncherAmmo,

        Rifle,
        Shotgun,
        GrenadeLauncher,
        Railgun
    }


    [Header("Drops")]
    [SerializeField]
    private List<DropEntry> drops = new();


    public IReadOnlyList<DropEntry> Drops =>
        drops;
}