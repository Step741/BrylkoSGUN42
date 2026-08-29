using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class WeaponSwitcher : MonoBehaviour
{
    [Header("Weapons")]

    [SerializeField]
    private WeaponBase[] weapons;

    [Header("Start")]

    [SerializeField]
    private int startWeaponIndex = 0;

    [Header("Unlocked Weapons")]

    [SerializeField]
    private bool[] unlockedWeapons;

    [Header("Animation")]

    [SerializeField]
    private PlayerAnimationController
        animationController;

    private IInputService inputService;

    private int currentWeaponIndex = -1;

    private int pendingWeaponIndex = -1;

    private bool isSwitchingWeapon;

    public WeaponBase CurrentWeapon =>
        currentWeaponIndex >= 0 &&
        currentWeaponIndex < weapons.Length
            ? weapons[currentWeaponIndex]
            : null;


    public event Action<WeaponBase>
        CurrentWeaponChanged;

    [Inject]
    private void Construct(
        IInputService inputService)
    {
        this.inputService =
            inputService;
    }

    private void Awake()
    {
        if (
            weapons == null ||
            weapons.Length == 0
        )
        {
            return;
        }


        if (
            animationController == null
        )
        {
            animationController =
                GetComponent<
                    PlayerAnimationController
                >();
        }

        if (
            unlockedWeapons == null ||
            unlockedWeapons.Length !=
            weapons.Length
        )
        {
            unlockedWeapons =
                new bool[
                    weapons.Length
                ];
        }

        for (
            int i = 0;
            i < weapons.Length;
            i++
        )
        {
            if (weapons[i] == null)
            {
                continue;
            }

            weapons[i].Unequip();
        }

        if (weapons.Length > 0)
        {
            unlockedWeapons[0] =
                true;
        }


        if (weapons.Length > 1)
        {
            unlockedWeapons[1] =
                true;
        }

        if (
            !IsWeaponUnlocked(
                startWeaponIndex
            )
        )
        {
            startWeaponIndex =
                FindFirstUnlockedWeapon();
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
        EquipWeaponImmediate(
            startWeaponIndex
        );

        SubscribeToInput();
    }


    private void OnDisable()
    {
        CancelCurrentWeaponReload();

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
        if (inputService == null)
            return;


        float scroll =
            inputService.SwitchWeapon
                .ReadValue<float>();


        if (
            Mathf.Approximately(
                scroll,
                0f
            )
        )
        {
            return;
        }


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
        if (
            weapons == null ||
            weapons.Length <= 1
        )
        {
            return;
        }


        if (isSwitchingWeapon)
            return;


        int nextIndex =
            FindNextUnlockedWeapon(
                currentWeaponIndex
            );


        if (nextIndex >= 0)
        {
            EquipWeapon(
                nextIndex
            );
        }
    }


    public void SwitchToPreviousWeapon()
    {
        if (
            weapons == null ||
            weapons.Length <= 1
        )
        {
            return;
        }


        if (isSwitchingWeapon)
            return;


        int previousIndex =
            FindPreviousUnlockedWeapon(
                currentWeaponIndex
            );


        if (previousIndex >= 0)
        {
            EquipWeapon(
                previousIndex
            );
        }
    }


    public void EquipWeapon(
        int index)
    {
        if (
            weapons == null ||
            index < 0 ||
            index >= weapons.Length
        )
        {
            return;
        }


        if (!IsWeaponUnlocked(index))
        {
            return;
        }


        if (index == currentWeaponIndex)
            return;


        if (isSwitchingWeapon)
            return;

        CancelCurrentWeaponReload();

        if (currentWeaponIndex < 0)
        {
            EquipWeaponImmediate(
                index
            );

            return;
        }

        pendingWeaponIndex =
            index;

        isSwitchingWeapon =
            true;


        if (
            animationController != null
        )
        {
            animationController
                .PlayWeaponSwitch();
        }
        else
        {
            ApplyWeaponSwitch();

            FinishWeaponSwitch();
        }
    }

    public void CancelCurrentWeaponReload()
    {
        CurrentWeapon?.CancelReload();
    }

    public void ApplyWeaponSwitch()
    {
        if (
            pendingWeaponIndex < 0 ||
            pendingWeaponIndex >=
            weapons.Length
        )
        {
            return;
        }


        WeaponBase previousWeapon =
            CurrentWeapon;


        previousWeapon?.Unequip();


        currentWeaponIndex =
            pendingWeaponIndex;


        WeaponBase newWeapon =
            CurrentWeapon;


        if (newWeapon == null)
        {
            currentWeaponIndex = -1;

            return;
        }


        newWeapon.Equip();


        CurrentWeaponChanged?.Invoke(
            newWeapon
        );


        pendingWeaponIndex =
            -1;
    }


    public void FinishWeaponSwitch()
    {
        pendingWeaponIndex =
            -1;

        isSwitchingWeapon =
            false;
    }

    private void EquipWeaponImmediate(
        int index)
    {
        if (
            weapons == null ||
            index < 0 ||
            index >= weapons.Length
        )
        {
            return;
        }


        WeaponBase previousWeapon =
            CurrentWeapon;


        previousWeapon?.Unequip();


        currentWeaponIndex =
            index;


        WeaponBase newWeapon =
            CurrentWeapon;


        if (newWeapon == null)
        {
            currentWeaponIndex =
                -1;

            return;
        }


        newWeapon.Equip();


        CurrentWeaponChanged?.Invoke(
            newWeapon
        );
    }
    public bool IsWeaponUnlocked(
        int index)
    {
        if (
            weapons == null ||
            index < 0 ||
            index >= weapons.Length
        )
        {
            return false;
        }


        if (
            unlockedWeapons == null ||
            index >=
            unlockedWeapons.Length
        )
        {
            return false;
        }


        return unlockedWeapons[index];
    }


    public bool UnlockWeapon(
        int index)
    {
        if (
            weapons == null ||
            index < 0 ||
            index >= weapons.Length
        )
        {
            return false;
        }


        if (weapons[index] == null)
        {
            return false;
        }


        if (unlockedWeapons[index])
            return false;


        unlockedWeapons[index] =
            true;

        return true;
    }


    public WeaponBase GetWeapon(
        int index)
    {
        if (
            weapons == null ||
            index < 0 ||
            index >= weapons.Length
        )
        {
            return null;
        }


        return weapons[index];
    }

    private int FindNextUnlockedWeapon(
        int currentIndex)
    {
        if (
            weapons == null ||
            weapons.Length == 0
        )
        {
            return -1;
        }


        for (
            int step = 1;
            step <= weapons.Length;
            step++
        )
        {
            int index =
                (
                    currentIndex +
                    step
                )
                %
                weapons.Length;


            if (IsWeaponUnlocked(index))
            {
                return index;
            }
        }


        return -1;
    }


    private int FindPreviousUnlockedWeapon(
        int currentIndex)
    {
        if (
            weapons == null ||
            weapons.Length == 0
        )
        {
            return -1;
        }


        for (
            int step = 1;
            step <= weapons.Length;
            step++
        )
        {
            int index =
                currentIndex -
                step;


            if (index < 0)
            {
                index +=
                    weapons.Length;
            }


            if (IsWeaponUnlocked(index))
            {
                return index;
            }
        }


        return -1;
    }


    private int FindFirstUnlockedWeapon()
    {
        if (weapons == null)
            return -1;


        for (
            int i = 0;
            i < weapons.Length;
            i++
        )
        {
            if (IsWeaponUnlocked(i))
            {
                return i;
            }
        }


        return -1;
    }
}