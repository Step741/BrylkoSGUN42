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


    // =========================================================
    // TRIGGER PICKUP
    // =========================================================

    private void OnTriggerEnter(Collider other)
    {
        if (isPickedUp)
            return;


        if (!IsPlayer(other.gameObject.layer))
            return;


        PickUp();
    }


    // =========================================================
    // PICKUP
    // =========================================================

    public void PickUp()
    {
        if (isPickedUp)
            return;


        PlayerInventory inventory =
            FindPlayerInventory();


        if (inventory == null)
        {
            Debug.LogWarning(
                $"[{name}] PlayerInventory not found."
            );

            return;
        }


        // -----------------------------------------------------
        // Уже есть карта?
        // -----------------------------------------------------

        if (!inventory.TryAddKeyCard())
        {
            // Карта уже есть.
            // Предмет остаётся на месте.

            return;
        }


        isPickedUp = true;


        // -----------------------------------------------------
        // FEEDBACK
        // -----------------------------------------------------

        if (pickupFeedback != null)
        {
            pickupFeedback.Play();
        }


        // -----------------------------------------------------
        // Отключаем лазерные барьеры
        // -----------------------------------------------------

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


        // -----------------------------------------------------
        // Активируем газ
        // -----------------------------------------------------

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


        Debug.Log(
            $"[{name}] Key card picked up."
        );


        Destroy(gameObject);
    }


    // =========================================================
    // PLAYER
    // =========================================================

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


    // =========================================================
    // LAYER
    // =========================================================

    private bool IsPlayer(int layer)
    {
        return
            (playerLayer.value &
            (1 << layer)) != 0;
    }
}