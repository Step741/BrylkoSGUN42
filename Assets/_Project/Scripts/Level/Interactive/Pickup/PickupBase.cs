using UnityEngine;

public abstract class PickupBase : MonoBehaviour, IPickable
{
    [Header("Pickup")]
    [SerializeField]
    private PickupAnimator pickupAnimator;

    [SerializeField]
    private PickupFeedback pickupFeedback;


    [Header("Player")]
    [SerializeField]
    private LayerMask playerLayer;


    private bool isPickedUp;

    private Transform pickupPlayer;


    protected Transform PickupPlayer =>
        pickupPlayer;

    protected virtual void Awake()
    {
        if (pickupAnimator == null)
        {
            pickupAnimator =
                GetComponentInChildren<PickupAnimator>();
        }

        if (pickupFeedback == null)
        {
            pickupFeedback =
                GetComponentInChildren<PickupFeedback>();
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (isPickedUp)
            return;


        //Проверяет слой самого коллайдера игрока
        if (!IsPlayer(other.gameObject.layer))
            return;

        Transform player =
            other.transform;


        TryPickUp(player);
    }


    public void PickUp()
    {
        if (isPickedUp)
            return;


        Transform player =
            FindPlayerTransform();


        if (player == null)
            return;


        TryPickUp(player);
    }

    private void TryPickUp(Transform player)
    {
        if (isPickedUp)
            return;


        if (player == null)
            return;

        if (!CanPickup(player))
            return;


        isPickedUp = true;

        pickupPlayer = player;

        pickupFeedback?.Play();

        if (pickupAnimator == null)
        {
            CompletePickup();
            return;
        }


        pickupAnimator.PlayPickupAnimation(
            player,
            CompletePickup
        );
    }

    private void CompletePickup()
    {
        ApplyPickup();

        Destroy(gameObject);
    }

    protected virtual bool CanPickup(
        Transform player)
    {
        return true;
    }

    protected abstract void ApplyPickup();

    private bool IsPlayer(int layer)
    {
        return
            (playerLayer.value &
            (1 << layer)) != 0;
    }


    private Transform FindPlayerTransform()
    {
        Collider collider =
            GetComponent<Collider>();


        if (collider == null)
            return null;

        Collider[] colliders =
            Physics.OverlapSphere(
                collider.bounds.center,
                collider.bounds.extents.magnitude,
                playerLayer
            );


        if (colliders == null ||
            colliders.Length == 0)
        {
            return null;
        }


        return colliders[0].transform;
    }

    //CLEANUP
    private void OnDisable()
    {
        isPickedUp = false;
        pickupPlayer = null;
    }
}