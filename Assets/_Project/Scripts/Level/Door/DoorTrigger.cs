using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    [SerializeField] private DoorController door;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other))
            return;

        door.PlayerEntered();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other))
            return;

        door.PlayerExited();
    }

    private bool IsPlayer(Collider other)
    {
        return other.gameObject.layer == LayerMask.NameToLayer("Player");
    }
}