using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class AmmoHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private WeaponSwitcher weaponSwitcher;

    [SerializeField]
    private TMP_Text ammoText;

    [Header("Railgun Heat")]
    [SerializeField]
    private Slider railgunHeatBar;

    [SerializeField]
    private GameObject railgunHeatRoot;

    private WeaponBase currentWeapon;

    private Railgun currentRailgun;

    private void OnEnable()
    {
        if (weaponSwitcher != null)
        {
            weaponSwitcher.CurrentWeaponChanged +=
                OnCurrentWeaponChanged;
        }
    }

    private void Start()
    {
        if (weaponSwitcher == null)
        {
            Debug.LogError(
                "AmmoHUD: WeaponSwitcher reference is missing."
            );

            return;
        }

        if (ammoText == null)
        {
            Debug.LogError(
                "AmmoHUD: Ammo Text reference is missing."
            );

            return;
        }

        if (railgunHeatBar == null)
        {
            Debug.LogError(
                "AmmoHUD: Railgun Heat Bar reference is missing."
            );
        }

        if (railgunHeatRoot == null &&
            railgunHeatBar != null)
        {
            railgunHeatRoot =
                railgunHeatBar.gameObject;
        }

        OnCurrentWeaponChanged(
            weaponSwitcher.CurrentWeapon
        );
    }

    private void OnDisable()
    {
        if (weaponSwitcher != null)
        {
            weaponSwitcher.CurrentWeaponChanged -=
                OnCurrentWeaponChanged;
        }

        UnsubscribeFromWeapon();
        UnsubscribeFromRailgun();

        HideRailgunHeat();
    }

    private void Update()
    {
        if (currentRailgun == null)
            return;

        UpdateRailgunHeat();
    }

    private void OnCurrentWeaponChanged(
        WeaponBase newWeapon)
    {
        UnsubscribeFromWeapon();
        UnsubscribeFromRailgun();

        currentWeapon = newWeapon;

        if (currentWeapon == null)
        {
            ClearAmmoUI();
            HideRailgunHeat();
            return;
        }

        // =========================================
        // РЕЛЬСОТРОН
        // =========================================

        if (currentWeapon is Railgun railgun)
        {
            currentRailgun = railgun;

            ShowRailgunHeat();

            UpdateRailgunHeat();

            // Патроны для рельсотрона не показываем.
            ClearAmmoUI();

            return;
        }

        // =========================================
        // ОБЫЧНОЕ ОРУЖИЕ
        // =========================================

        HideRailgunHeat();

        currentWeapon.AmmoChanged +=
            OnAmmoChanged;

        UpdateAmmoUI(
            currentWeapon.CurrentAmmo,
            currentWeapon.ReserveAmmo
        );
    }

    private void UnsubscribeFromWeapon()
    {
        if (currentWeapon == null)
            return;

        currentWeapon.AmmoChanged -=
            OnAmmoChanged;

        currentWeapon = null;
    }

    private void UnsubscribeFromRailgun()
    {
        currentRailgun = null;
    }

    private void OnAmmoChanged(
        int currentAmmo,
        int reserveAmmo)
    {
        UpdateAmmoUI(
            currentAmmo,
            reserveAmmo
        );
    }

    private void UpdateAmmoUI(
        int currentAmmo,
        int reserveAmmo)
    {
        if (ammoText == null)
            return;

        ammoText.text =
            $"{currentAmmo} / {reserveAmmo}";
    }

    private void ClearAmmoUI()
    {
        if (ammoText == null)
            return;

        ammoText.text = "— / —";
    }

    private void ShowRailgunHeat()
    {
        if (railgunHeatRoot != null)
        {
            railgunHeatRoot.SetActive(true);
        }
    }

    private void HideRailgunHeat()
    {
        if (railgunHeatRoot != null)
        {
            railgunHeatRoot.SetActive(false);
        }
    }

    private void UpdateRailgunHeat()
    {
        if (railgunHeatBar == null ||
            currentRailgun == null)
        {
            return;
        }

        railgunHeatBar.minValue = 0f;
        railgunHeatBar.maxValue = 1f;

        railgunHeatBar.value =
            currentRailgun.HeatNormalized;
    }
}