using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

public class InputManager : MonoBehaviour
{
    [SerializeField] 
    private Restart _restart;

    [Inject]
    private Controls _controls;

    private void Awake()
    {
        _controls.Game.Restart.Enable();

        _controls.Game.Restart.started += Restart_started;
        _controls.Game.Restart.performed += Restart_performed;
        _controls.Game.Restart.canceled += Restart_canceled;
    }

    private void Restart_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        _restart.HidePanel();
        _restart.StopFillPanel();
    }

    private void Restart_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (_restart.IsEnd)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void Restart_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        _restart.ShowPanel();
        _restart.StartFillPanel(2);
    }

    private void OnDestroy()
    {
        _controls.Game.Restart.started -= Restart_started;
        _controls.Game.Restart.performed -= Restart_performed;
        _controls.Game.Restart.canceled -= Restart_canceled;

        _controls.Game.Restart.Disable();
    }
}
