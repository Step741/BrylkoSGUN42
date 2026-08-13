using UnityEngine;

public class ShooterDeadState : EnemyState
{
    private readonly EnemyShooter shooter;

    public ShooterDeadState(Enemy enemy)
        : base(enemy)
    {
        shooter = enemy.GetComponent<EnemyShooter>();
    }

    public override void Enter()
    {
        if (shooter != null)
        {
            shooter.StopMoving();
        }

        Debug.Log(
            $"[{enemy.name}] State: Dead"
        );
    }

    public override void Tick()
    {
        // В состоянии Dead враг ничего не делает.
    }

    public override void Exit()
    {
        // Из Dead пока не выходим.
    }
}