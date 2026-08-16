using UnityEngine;

[CreateAssetMenu(
    fileName = "WeaponConfig",
    menuName = "Project/Weapons/Weapon Config"
)]
public class WeaponConfig : ScriptableObject
{
    [Header("Identity")]
    [SerializeField]
    private string weaponName;

    [Header("Damage")]
    [SerializeField]
    private float damage = 25f;

    [Header("Fire")]
    [SerializeField]
    private float fireRate = 4f;

    [Header("Ammo")]
    [SerializeField]
    private int magazineSize = 12;

    [SerializeField]
    private int maxReserveAmmo = 120;

    [SerializeField]
    private int reserveAmmo = 60;

    [SerializeField]
    private bool infiniteAmmo = false;

    [Header("Ballistics")]
    [SerializeField]
    private float range = 100f;

    [SerializeField]
    private float spread = 0f;

    [SerializeField]
    private float maxSpread = 5f;

    [SerializeField]
    private float spreadIncreasePerShot = 0.35f;

    [SerializeField]
    private float spreadRecoverySpeed = 2f;

    [Header("Shotgun")]
    [SerializeField]
    private int pellets = 10;

    [SerializeField]
    private float minDamageMultiplier = 0.25f;

    [SerializeField]
    private float maxDamageMultiplier = 1f;

    [SerializeField]
    private float knockbackForce = 4f;

    [Header("Grenade Launcher")]
    [SerializeField]
    private GameObject projectilePrefab;

    [SerializeField]
    private float projectileSpeed = 20f;

    [SerializeField]
    private float projectileLifetime = 8f;

    [SerializeField]
    private float explosionDamage = 80f;

    [SerializeField]
    private float explosionRadius = 5f;

    [SerializeField]
    private float explosionForce = 700f;

    [SerializeField]
    private float explosionUpwardModifier = 0.5f;

    [SerializeField]
    private int maxExplosionTargets = 32;

    [SerializeField]
    private LayerMask explosionTargetMask;

    [SerializeField]
    private LayerMask explosionObstacleMask;

    [Header("Explosion Effects")]
    [SerializeField]
    private GameObject explosionVfxPrefab;

    [SerializeField]
    private AudioClip explosionSound;

    [SerializeField]
    private float explosionVfxLifetime = 2f;

    [Header("Railgun")]
    [SerializeField]
    private int railgunMaxTargets = 3;

    [SerializeField]
    private float railgunHeatPerShot = 25f;

    [SerializeField]
    private float railgunMaxHeat = 100f;

    [SerializeField]
    private float railgunChargeTime = 2f;

    [SerializeField]
    private float railgunBeamDuration = 0.08f;

    [SerializeField]
    private LayerMask railgunHitMask;

    [Header("Katana")]
    [SerializeField]
    private float katanaAttackRange = 2f;

    [SerializeField]
    private float katanaAttackRadius = 1.2f;

    [SerializeField]
    private float katanaAttackCooldown = 0.8f;

    [SerializeField]
    private int katanaMaxTargets = 1;

    [SerializeField]
    private LayerMask katanaAttackMask;

    [Header("Recoil")]
    [SerializeField]
    private float recoil = 1f;


    // =========================
    // PUBLIC PROPERTIES
    // =========================

    public string WeaponName =>
        weaponName;

    public float Damage =>
        damage;

    public float FireRate =>
        fireRate;


    // =========================
    // AMMO
    // =========================

    public int MagazineSize =>
        magazineSize;

    public int ReserveAmmo =>
        reserveAmmo;

    public int MaxReserveAmmo =>
        maxReserveAmmo;

    public bool InfiniteAmmo =>
        infiniteAmmo;


    // =========================
    // BALLISTICS
    // =========================

    public float Range =>
        range;

    public float Spread =>
        spread;

    public float MaxSpread =>
        maxSpread;

    public float SpreadIncreasePerShot =>
        spreadIncreasePerShot;

    public float SpreadRecoverySpeed =>
        spreadRecoverySpeed;


    // =========================
    // SHOTGUN
    // =========================

    public int Pellets =>
        pellets;

    public float MinDamageMultiplier =>
        minDamageMultiplier;

    public float MaxDamageMultiplier =>
        maxDamageMultiplier;

    public float KnockbackForce =>
        knockbackForce;


    // =========================
    // GRENADE LAUNCHER
    // =========================

    public GameObject ProjectilePrefab =>
        projectilePrefab;

    public float ProjectileSpeed =>
        projectileSpeed;

    public float ProjectileLifetime =>
        projectileLifetime;

    public float ExplosionDamage =>
        explosionDamage;

    public float ExplosionRadius =>
        explosionRadius;

    public float ExplosionForce =>
        explosionForce;

    public float ExplosionUpwardModifier =>
        explosionUpwardModifier;

    public int MaxExplosionTargets =>
        maxExplosionTargets;

    public LayerMask ExplosionTargetMask =>
        explosionTargetMask;

    public LayerMask ExplosionObstacleMask =>
        explosionObstacleMask;


    // =========================
    // EXPLOSION EFFECTS
    // =========================

    public GameObject ExplosionVfxPrefab =>
        explosionVfxPrefab;

    public AudioClip ExplosionSound =>
        explosionSound;

    public float ExplosionVfxLifetime =>
        explosionVfxLifetime;


    // =========================
    // RAILGUN
    // =========================

    public int RailgunMaxTargets =>
        railgunMaxTargets;

    public float RailgunHeatPerShot =>
        railgunHeatPerShot;

    public float RailgunMaxHeat =>
        railgunMaxHeat;

    public float RailgunChargeTime =>
        railgunChargeTime;

    public float RailgunBeamDuration =>
        railgunBeamDuration;

    public LayerMask RailgunHitMask =>
        railgunHitMask;


    // =========================
    // KATANA
    // =========================

    public float KatanaAttackRange =>
        katanaAttackRange;

    public float KatanaAttackRadius =>
        katanaAttackRadius;

    public float KatanaAttackCooldown =>
        katanaAttackCooldown;

    public int KatanaMaxTargets =>
        katanaMaxTargets;

    public LayerMask KatanaAttackMask =>
        katanaAttackMask;


    // =========================
    // RECOIL
    // =========================

    public float Recoil =>
        recoil;
}