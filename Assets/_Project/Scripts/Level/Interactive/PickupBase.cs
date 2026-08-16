using UnityEngine;

public abstract class PickupBase : MonoBehaviour, IPickable
{
    [Header("Pickup")]
    [SerializeField]
    private PickupAnimator pickupAnimator;

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
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isPickedUp)
            return;

        // Проверяем слой самого коллайдера игрока.
        if (!IsPlayer(other.gameObject.layer))
            return;

        // ВАЖНО:
        // Не используем other.transform.root.
        // После поездки на лифте Player становится
        // дочерним объектом платформы.
        //
        // Поэтому root может оказаться ElevatorPlatform.
        //
        // Нам нужен непосредственно Transform,
        // который вошёл в триггер.
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

        // Проверяем, можно ли подобрать объект,
        // ДО запуска анимации.
        if (!CanPickup(player))
            return;

        isPickedUp = true;

        pickupPlayer = player;

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

    /// <summary>
    /// Проверяет, можно ли подобрать объект.
    /// По умолчанию любой Pickup можно подобрать.
    /// </summary>
    protected virtual bool CanPickup(Transform player)
    {
        return true;
    }

    protected abstract void ApplyPickup();

    private bool IsPlayer(int layer)
    {
        return
            (playerLayer.value & (1 << layer)) != 0;
    }

    private Transform FindPlayerTransform()
    {
        Collider collider =
            GetComponent<Collider>();

        if (collider == null)
            return null;

        // Если сам Pickup находится в триггере
        // и рядом находится Player, ищем его.
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

    private void OnDisable()
    {
        isPickedUp = false;
        pickupPlayer = null;
    }
}