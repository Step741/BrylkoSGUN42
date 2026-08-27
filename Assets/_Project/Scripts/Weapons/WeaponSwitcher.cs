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
            Debug.LogError(
                "WeaponSwitcher: Weapons array is empty.",
                this
            );

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


        // Приводим массив разблокировки
        // к количеству оружия.

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


        // Проверяем оружие
        // и выключаем всё при старте.

        for (
            int i = 0;
            i < weapons.Length;
            i++
        )
        {
            if (weapons[i] == null)
            {
                Debug.LogError(
                    $"WeaponSwitcher: " +
                    $"Weapon at index {i} is missing.",
                    this
                );

                continue;
            }

            weapons[i].Unequip();
        }


        // Первые два оружия
        // доступны с начала.

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


        // Проверяем стартовое оружие.

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
        // Стартовое оружие экипируем
        // без анимации.

        EquipWeaponImmediate(
            startWeaponIndex
        );

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


    // =========================================================
    // INPUT
    // =========================================================

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


    // =========================================================
    // SWITCHING
    // =========================================================

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
            Debug.Log(
                $"WeaponSwitcher: " +
                $"Weapon {index} is locked."
            );

            return;
        }


        if (index == currentWeaponIndex)
            return;


        if (isSwitchingWeapon)
            return;


        // Если это первое оружие
        // и персонаж ещё ничего не держит.

        if (currentWeaponIndex < 0)
        {
            EquipWeaponImmediate(
                index
            );

            return;
        }


        // Запоминаем оружие,
        // которое нужно экипировать
        // в середине анимации.

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
            // Если контроллер анимации
            // не назначен — меняем сразу.

            ApplyWeaponSwitch();
            FinishWeaponSwitch();
        }
    }


    // =========================================================
    // ANIMATION EVENTS
    // =========================================================

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
            Debug.LogError(
                $"WeaponSwitcher: " +
                $"Weapon at index " +
                $"{currentWeaponIndex} is missing.",
                this
            );

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


    // =========================================================
    // IMMEDIATE EQUIP
    // =========================================================

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
            Debug.LogError(
                $"WeaponSwitcher: " +
                $"Weapon at index {index} is missing.",
                this
            );

            currentWeaponIndex =
                -1;

            return;
        }


        newWeapon.Equip();

        CurrentWeaponChanged?.Invoke(
            newWeapon
        );
    }


    // =========================================================
    // UNLOCK
    // =========================================================

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
            Debug.LogWarning(
                $"WeaponSwitcher: " +
                $"Cannot unlock missing weapon " +
                $"at index {index}."
            );

            return false;
        }


        if (unlockedWeapons[index])
            return false;


        unlockedWeapons[index] =
            true;


        Debug.Log(
            $"WeaponSwitcher: " +
            $"Unlocked weapon {index}: " +
            $"{weapons[index].name}"
        );

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


    // =========================================================
    // FIND NEXT / PREVIOUS
    // =========================================================

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