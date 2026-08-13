using UnityEngine;

public class MeleeDeadState : EnemyState
{
    private readonly EnemyMelee melee;

    public MeleeDeadState(Enemy enemy)
        : base(enemy)
    {
        melee = enemy.GetComponent<EnemyMelee>();
    }

    public override void Enter()
    {
        if (melee == null)
        {
            Debug.LogError(
                $"[{enemy.name}] MeleeDeadState: " +
                "EnemyMelee is missing."
            );

            return;
        }

        // Останавливаем движение.
        melee.CancelAttack();
        melee.StopMoving();

        Debug.Log(
            $"[{enemy.name}] State: Dead"
        );
    }

    public override void Tick()
    {
        // После смерти AI ничего не делает.
    }

    public override void Exit()
    {
        // Из Dead выходить не должны.
    }
}