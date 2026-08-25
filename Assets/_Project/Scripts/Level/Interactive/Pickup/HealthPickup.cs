using UnityEngine;

public class HealthPickup : PickupBase
{
    [Header("Health")]
    [SerializeField]
    private float healAmount = 25f;

    protected override bool CanPickup(Transform player)
    {
        if (player == null)
            return false;

        Health health =
            player.GetComponent<Health>();

        if (health == null)
        {
            health =
                player.GetComponentInChildren<Health>(
                    true
                );
        }

        if (health == null)
        {
            Debug.LogWarning(
                $"[{name}] Health component not found."
            );

            return false;
        }

        // Мёртвый игрок аптечку подобрать не может.
        if (health.IsDead)
            return false;

        // Если здоровье полное —
        // аптечка остаётся на месте.
        if (health.CurrentHealth >= health.MaxHealth)
        {
            return false;
        }

        return true;
    }

    protected override void ApplyPickup()
    {
        if (PickupPlayer == null)
            return;

        Health health =
            PickupPlayer.GetComponent<Health>();

        if (health == null)
        {
            health =
                PickupPlayer.GetComponentInChildren<Health>(
                    true
                );
        }

        if (health == null)
        {
            Debug.LogWarning(
                $"[{name}] Health component not found."
            );

            return;
        }

        if (health.IsDead)
            return;

        if (health.CurrentHealth >= health.MaxHealth)
        {
            Debug.Log(
                $"[{name}] Player health is already full."
            );

            return;
        }

        health.Heal(healAmount);

        Debug.Log(
            $"[{name}] Health pickup: +{healAmount} HP."
        );
    }
}