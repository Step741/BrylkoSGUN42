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

        Debug.Log("[Elevator] Player entered button zone.");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other))
            return;

        playerInside = false;

        Debug.Log("[Elevator] Player left button zone.");
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (!playerInside)
            return;

        Debug.Log("[Elevator] Interact pressed.");

        if (elevator == null)
        {
            Debug.LogError("[ButtonTrigger] Elevator is not assigned.", this);
            return;
        }

        elevator.Activate();
    }

    private bool IsPlayer(Collider other)
    {
        return other.gameObject.layer == LayerMask.NameToLayer("Player");
    }
}