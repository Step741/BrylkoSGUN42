using UnityEngine;
using Zenject;

public class WeaponController : MonoBehaviour
{
    [Header("References")]

    [SerializeField]
    private WeaponSwitcher weaponSwitcher;

    [SerializeField]
    private PlayerAnimationController playerAnimationController;

    [SerializeField]
    private DynamicCrosshair dynamicCrosshair;


    private IInputService inputService;


    [Inject]
    private void Construct(
        IInputService inputService)
    {
        this.inputService =
            inputService;
    }


    private void Update()
    {
        HandleFire();

        HandleReload();
    }


    private void HandleFire()
    {
        if (weaponSwitcher == null)
            return;


        WeaponBase currentWeapon =
            weaponSwitcher.CurrentWeapon;

        if (currentWeapon == null)
            return;


        bool firePressed =
            inputService.Fire.WasPressedThisFrame();

        bool fireHeld =
            inputService.Fire.IsPressed();


        bool shouldShoot =
            currentWeapon is Rifle
                ? fireHeld
                : firePressed;


        if (!shouldShoot)
            return;


        bool shot =
            currentWeapon.Shoot();


        if (!shot)
            return;

        dynamicCrosshair
            ?.AddFireSpread();


        if (currentWeapon is Rifle)
        {
            playerAnimationController
                ?.PlayRifleShoot();
        }
        else if (currentWeapon is Shotgun)
        {
            playerAnimationController
                ?.PlayShotgunShoot();
        }
        else if (currentWeapon is GrenadeLauncher)
        {
            playerAnimationController
                ?.PlayGrenadeShoot();
        }
        else if (currentWeapon is Railgun)
        {
            playerAnimationController
                ?.PlayRailgunShoot();
        }
        else if (currentWeapon is Katana)
        {
            playerAnimationController
                ?.PlayKatanaAttack();
        }
        else
        {
            playerAnimationController
                ?.PlayShoot();
        }
    }


    private void HandleReload()
    {
        if (weaponSwitcher == null)
            return;


        WeaponBase currentWeapon =
            weaponSwitcher.CurrentWeapon;

        if (currentWeapon == null)
            return;


        bool reloadPressed =
            inputService.Reload
                .WasPressedThisFrame();


        if (!reloadPressed)
            return;


        if (!currentWeapon.CanReload)
            return;


        currentWeapon.Reload();


        playerAnimationController
            ?.PlayReload();
    }


    public void OnGrenadeFireAnimationEvent()
    {
        if (weaponSwitcher == null)
            return;


        WeaponBase currentWeapon =
            weaponSwitcher.CurrentWeapon;


        if (
            currentWeapon is
            GrenadeLauncher grenadeLauncher)
        {
            grenadeLauncher
                .FireProjectile();
        }
    }


    public void OnRailgunFireAnimationEvent()
    {
        if (weaponSwitcher == null)
            return;


        WeaponBase currentWeapon =
            weaponSwitcher.CurrentWeapon;


        if (
            currentWeapon is
            Railgun railgun)
        {
            railgun
                .FireRailgun();
        }
    }


    public void OnKatanaAttackAnimationEvent()
    {
        if (weaponSwitcher == null)
            return;


        WeaponBase currentWeapon =
            weaponSwitcher.CurrentWeapon;


        if (
            currentWeapon is
            Katana katana)
        {
            katana
                .Attack();
        }
    }
}