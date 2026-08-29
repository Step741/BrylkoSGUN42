using UnityEngine;
using Zenject;

public class GameInputController : MonoBehaviour
{
    private IInputService inputService;


    [Inject]
    private void Construct(
        IInputService inputService)
    {
        this.inputService = inputService;
    }


    private void Start()
    {
        inputService.EnablePlayerInput();

        //Блокирует курсор для управления камерой
        Cursor.lockState =
            CursorLockMode.Locked;

        Cursor.visible =
            false;
    }
}