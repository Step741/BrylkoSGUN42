using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AudioZone : MonoBehaviour
{
    [Header("Player Detection")]

    [SerializeField]
    private LayerMask playerLayer;


    private Collider zoneCollider;

    private bool playerInside;


    private void Awake()
    {
        zoneCollider =
            GetComponent<Collider>();


        if (zoneCollider != null)
        {
            zoneCollider.isTrigger =
                true;
        }
    }


    private void OnTriggerEnter(
        Collider other)
    {
        if (!IsPlayer(other))
            return;


        //Защита от повторного входа, если у игрока несколько Collider.
        if (playerInside)
            return;


        playerInside = true;


        AudioSnapshotController controller =
            AudioSnapshotController.Instance;


        if (controller == null)
            return;


        controller.EnterIndoorZone();
    }


    private void OnTriggerExit(
        Collider other)
    {
        if (!IsPlayer(other))
            return;


        if (!playerInside)
            return;


        playerInside = false;


        AudioSnapshotController controller =
            AudioSnapshotController.Instance;


        if (controller == null)
            return;


        controller.ExitIndoorZone();
    }

    private bool IsPlayer(
        Collider other)
    {
        return (
            playerLayer.value &
            (1 << other.gameObject.layer)
        ) != 0;
    }
}