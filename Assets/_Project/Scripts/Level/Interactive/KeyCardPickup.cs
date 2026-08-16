using UnityEngine;

public class KeyCardPickup : MonoBehaviour, IPickable
{
    [Header("Laser Barriers")]
    [SerializeField] private LaserBarrier[] _laserBarriers;

    [Header("Gas")]
    [SerializeField] private GasDamageZone _gasDamageZone;

    [Header("Interaction")]
    [SerializeField] private LayerMask _playerLayer;

    private bool _isPickedUp;

    private void OnTriggerEnter(Collider other)
    {
        if (_isPickedUp)
            return;

        if ((_playerLayer.value & (1 << other.gameObject.layer)) == 0)
            return;

        PickUp();
    }

    public void PickUp()
    {
        if (_isPickedUp)
            return;

        _isPickedUp = true;

        // Отключаем лазерные барьеры
        foreach (LaserBarrier barrier in _laserBarriers)
        {
            if (barrier != null)
                barrier.DisableBarrier();
        }

        // Активируем газ
        if (_gasDamageZone != null)
            _gasDamageZone.ActivateGas();

        Destroy(gameObject);
    }
}