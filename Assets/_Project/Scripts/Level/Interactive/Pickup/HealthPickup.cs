using UnityEngine;

public class HealthPickup : PickupBase
{
    [Header("Health")]

    [SerializeField]
    private float healAmount = 25f;

    protected override bool CanPickup(
        Transform player
    )
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
            return false;
        }

        if (health.IsDead)
            return false;

        if (
            health.CurrentHealth >=
            health.MaxHealth
        )
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
            return;
        }


        if (health.IsDead)
            return;


        if (
            health.CurrentHealth >=
            health.MaxHealth
        )
        {
            return;
        }

        health.HealOverTime(
            healAmount
        );
    }
}