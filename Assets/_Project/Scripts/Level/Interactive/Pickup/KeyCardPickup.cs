using System;
using UnityEngine;

public class KeyCardPickup : MonoBehaviour, IPickable
{
    [Header("Laser Barriers")]
    [SerializeField]
    private LaserBarrier[] laserBarriers;


    [Header("Gas")]
    [SerializeField]
    private GasDamageZone[] gasZones;


    [Header("Feedback")]
    [SerializeField]
    private PickupFeedback pickupFeedback;


    [Header("Interaction")]
    [SerializeField]
    private LayerMask playerLayer;


    private bool isPickedUp;

    public event Action PickedUp;

    private void OnTriggerEnter(Collider other)
    {
        if (isPickedUp)
            return;

        if (!IsPlayer(other.gameObject.layer))
            return;

        PickUp();
    }

    public void PickUp()
    {
        if (isPickedUp)
            return;


        PlayerInventory inventory =
            FindPlayerInventory();


        if (inventory == null)
        {
            return;
        }

        if (!inventory.TryAddKeyCard())
        {
            return;
        }


        isPickedUp = true;

        if (pickupFeedback != null)
        {
            pickupFeedback.Play();
        }

        if (laserBarriers != null)
        {
            foreach (LaserBarrier barrier in laserBarriers)
            {
                if (barrier != null)
                {
                    barrier.DisableBarrier();
                }
            }
        }

        if (gasZones != null)
        {
            foreach (GasDamageZone gasZone in gasZones)
            {
                if (gasZone != null)
                {
                    gasZone.ActivateGas();
                }
            }
        }

        PickedUp?.Invoke();


        Destroy(gameObject);
    }

    private PlayerInventory FindPlayerInventory()
    {
        Collider[] colliders =
            Physics.OverlapSphere(
                transform.position,
                1.5f,
                playerLayer,
                QueryTriggerInteraction.Ignore
            );


        foreach (Collider collider in colliders)
        {
            if (collider == null)
                continue;


            PlayerInventory inventory =
                collider.GetComponentInParent<PlayerInventory>();


            if (inventory != null)
                return inventory;
        }


        return null;
    }

    private bool IsPlayer(int layer)
    {
        return
            (playerLayer.value &
            (1 << layer)) != 0;
    }
}