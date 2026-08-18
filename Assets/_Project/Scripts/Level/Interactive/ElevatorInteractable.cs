using UnityEngine;

public class ElevatorInteractable : MonoBehaviour, IInteractable
{
    [Header("Interaction")]
    [SerializeField] private string interactionText = "PRESS - E";

    [Header("References")]
    [SerializeField] private ElevatorController elevatorController;

    public string GetInteractionText()
    {
        return interactionText;
    }

    public void Interact()
    {
        if (elevatorController == null)
        {
            Debug.LogWarning(
                $"[ElevatorInteractable] ElevatorController is not assigned on {gameObject.name}.",
                this
            );

            return;
        }

        elevatorController.Activate();
    }
}