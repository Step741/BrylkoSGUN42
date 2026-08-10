using System;
using UnityEngine.InputSystem;
using Zenject;

public class InputService : IInputService, IInitializable, IDisposable
{
    private readonly TPSInputActions inputActions;

    public InputAction Move => inputActions.Player.Move;
    public InputAction Look => inputActions.Player.Look;
    public InputAction Jump => inputActions.Player.Jump;
    public InputAction Sprint => inputActions.Player.Sprint;
    public InputAction Crouch => inputActions.Player.Crouch;
    public InputAction Aim => inputActions.Player.Aim;
    public InputAction Fire => inputActions.Player.Fire;
    public InputAction Reload => inputActions.Player.Reload;
    public InputAction SwitchWeapon => inputActions.Player.SwitchWeapon;

    public InputAction Pause => inputActions.UI.Pause;
    public InputAction Submit => inputActions.UI.Submit;

    public InputService()
    {
        inputActions = new TPSInputActions();
    }

    public void Initialize()
    {
        EnablePlayerInput();
    }

    public void EnablePlayerInput()
    {
        inputActions.UI.Disable();
        inputActions.Player.Enable();
    }

    public void EnableUIInput()
    {
        inputActions.Player.Disable();
        inputActions.UI.Enable();
    }

    public void Dispose()
    {
        inputActions.Dispose();
    }
}