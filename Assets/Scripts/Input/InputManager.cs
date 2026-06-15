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
        _controls.Game.Restart.canceled += Restart_canceled;

        _restart.OnFillComplete += RestartScene;
    }

    private void Restart_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        _restart.ShowPanel();
        _restart.StartFillPanel(2f);
    }

    private void Restart_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        _restart.HidePanel();
        _restart.StopFillPanel();
    }

    private void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnDestroy()
    {
        _controls.Game.Restart.started -= Restart_started;
        _controls.Game.Restart.canceled -= Restart_canceled;

        _restart.OnFillComplete -= RestartScene;

        _controls.Game.Restart.Disable();
    }
}