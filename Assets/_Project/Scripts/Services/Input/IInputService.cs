using UnityEngine.InputSystem;

public interface IInputService
{
    InputAction Move { get; }
    InputAction Look { get; }
    InputAction Jump { get; }
    InputAction Sprint { get; }
    InputAction Crouch { get; }
    InputAction Aim { get; }
    InputAction Fire { get; }
    InputAction Reload { get; }
    InputAction Interact { get; }

    InputAction SwitchWeapon { get; }
    InputAction SelectWeapon { get; }

    InputAction Pause { get; }
    InputAction Submit { get; }

    void EnablePlayerInput();
    void EnableUIInput();
}