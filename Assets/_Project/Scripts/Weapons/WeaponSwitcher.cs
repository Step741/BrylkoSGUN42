using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class WeaponSwitcher : MonoBehaviour
{
    [Header("Weapons")]
    [SerializeField]
    private WeaponBase[] weapons;

    [SerializeField]
    private int startWeaponIndex = 0;

    private IInputService inputService;

    private int currentWeaponIndex = -1;

    public WeaponBase CurrentWeapon =>
        currentWeaponIndex >= 0 &&
        currentWeaponIndex < weapons.Length
            ? weapons[currentWeaponIndex]
            : null;

    public event Action<WeaponBase> CurrentWeaponChanged;

    [Inject]
    private void Construct(IInputService inputService)
    {
        this.inputService = inputService;
    }

    private void Awake()
    {
        if (weapons == null || weapons.Length == 0)
        {
            Debug.LogError(
                "WeaponSwitcher: Weapons array is empty."
            );

            return;
        }

        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] == null)
            {
                Debug.LogError(
                    $"WeaponSwitcher: Weapon at index {i} is missing."
                );

                continue;
            }

            weapons[i].Unequip();
        }
    }

    private void OnEnable()
    {
        if (inputService != null)
        {
            SubscribeToInput();
        }
    }

    private void Start()
    {
        EquipWeapon(startWeaponIndex);

        SubscribeToInput();
    }

    private void OnDisable()
    {
        UnsubscribeFromInput();
    }

    private void Update()
    {
        HandleMouseWheel();
    }

    private void SubscribeToInput()
    {
        if (inputService == null)
            return;

        inputService.SelectWeapon.performed -=
            OnSelectWeapon;

        inputService.SelectWeapon.performed +=
            OnSelectWeapon;
    }

    private void UnsubscribeFromInput()
    {
        if (inputService == null)
            return;

        inputService.SelectWeapon.performed -=
            OnSelectWeapon;
    }

    private void OnSelectWeapon(
        InputAction.CallbackContext context)
    {
        string controlName =
            context.control.name;

        switch (controlName)
        {
            case "1":
                EquipWeapon(0);
                break;

            case "2":
                EquipWeapon(1);
                break;

            case "3":
                EquipWeapon(2);
                break;

            case "4":
                EquipWeapon(3);
                break;

            case "5":
                EquipWeapon(4);
                break;

            case "6":
                EquipWeapon(5);
                break;
        }
    }

    private void HandleMouseWheel()
    {
        float scroll =
            inputService.SwitchWeapon.ReadValue<float>();

        if (Mathf.Approximately(scroll, 0f))
            return;

        if (scroll > 0f)
        {
            SwitchToNextWeapon();
        }
        else
        {
            SwitchToPreviousWeapon();
        }
    }

    public void SwitchToNextWeapon()
    {
        if (weapons.Length <= 1)
            return;

        int nextIndex =
            (currentWeaponIndex + 1) %
            weapons.Length;

        EquipWeapon(nextIndex);
    }

    public void SwitchToPreviousWeapon()
    {
        if (weapons.Length <= 1)
            return;

        int previousIndex =
            currentWeaponIndex - 1;

        if (previousIndex < 0)
        {
            previousIndex =
                weapons.Length - 1;
        }

        EquipWeapon(previousIndex);
    }

    public void EquipWeapon(int index)
    {
        if (weapons == null ||
            index < 0 ||
            index >= weapons.Length)
        {
            return;
        }

        if (index == currentWeaponIndex)
            return;

        WeaponBase previousWeapon =
            CurrentWeapon;

        previousWeapon?.Unequip();

        currentWeaponIndex = index;

        WeaponBase newWeapon =
            CurrentWeapon;

        if (newWeapon == null)
        {
            Debug.LogError(
                $"WeaponSwitcher: Weapon at index {index} is missing."
            );

            currentWeaponIndex = -1;
            return;
        }

        newWeapon.Equip();

        CurrentWeaponChanged?.Invoke(
            newWeapon
        );
    }
}