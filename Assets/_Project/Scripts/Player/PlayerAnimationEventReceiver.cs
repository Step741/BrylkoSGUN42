using UnityEngine;

public class PlayerAnimationEventReceiver : MonoBehaviour
{
    private WeaponController weaponController;

    private void Awake()
    {
        weaponController =
            GetComponentInParent<WeaponController>();

        if (weaponController == null)
        {
        }
    }

    public void OnGrenadeFireAnimationEvent()
    {
        if (weaponController == null)
            return;

        weaponController.OnGrenadeFireAnimationEvent();
    }
    public void OnRailgunFireAnimationEvent()
    {
        if (weaponController == null)
            return;

        weaponController.OnRailgunFireAnimationEvent();
    }

    public void OnKatanaAttackAnimationEvent()
    {
        if (weaponController == null)
            return;

        weaponController.OnKatanaAttackAnimationEvent();
    }
}