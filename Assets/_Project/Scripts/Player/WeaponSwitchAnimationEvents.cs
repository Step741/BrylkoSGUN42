using UnityEngine;

public class WeaponSwitchAnimationEvents : MonoBehaviour
{
    [SerializeField] private WeaponSwitcher weaponSwitcher;

    public void ApplyWeaponSwitch()
    {
        if (weaponSwitcher != null)
            weaponSwitcher.ApplyWeaponSwitch();
    }

    public void FinishWeaponSwitch()
    {
        if (weaponSwitcher != null)
            weaponSwitcher.FinishWeaponSwitch();
    }
}