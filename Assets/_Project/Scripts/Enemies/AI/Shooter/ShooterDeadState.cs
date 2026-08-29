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
    }

    public override void Tick()
    {
    }

    public override void Exit()
    {
    }
}