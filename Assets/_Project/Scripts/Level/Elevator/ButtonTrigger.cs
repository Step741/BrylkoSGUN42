using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class ButtonTrigger : MonoBehaviour
{
    [SerializeField] private ElevatorController elevator;

    private IInputService inputService;
    private bool playerInside;

    [Inject]
    private void Construct(IInputService inputService)
    {
        this.inputService = inputService;

        inputService.Interact.performed += OnInteract;
    }

    private void OnDestroy()
    {
        if (inputService != null)
        {
            inputService.Interact.performed -= OnInteract;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other))
            return;

        playerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other))
            return;

        playerInside = false;
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (!playerInside)
            return;

        if (elevator == null)
        {
            return;
        }

        elevator.Activate();
    }

    private bool IsPlayer(Collider other)
    {
        return other.gameObject.layer == LayerMask.NameToLayer("Player");
    }
}