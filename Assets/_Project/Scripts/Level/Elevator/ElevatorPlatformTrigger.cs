using UnityEngine;

public class ElevatorPlatformTrigger : MonoBehaviour
{
    [SerializeField] private ElevatorController elevator;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other))
            return;

        elevator.SetPlayer(other.transform);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other))
            return;

        elevator.ClearPlayer(other.transform);
    }

    private bool IsPlayer(Collider other)
    {
        return other.gameObject.layer == LayerMask.NameToLayer("Player");
    }
}