using UnityEngine;

public class ShooterCombatState : EnemyState
{
    private readonly EnemyShooter shooter;

    private readonly EnemySoundController
        enemySoundController;


    private float attackTimer;
    private int attackCount;

    private const int AttacksBeforeBackstep =
        3;


    public ShooterCombatState(Enemy enemy)
        : base(enemy)
    {
        shooter =
            enemy.GetComponent<EnemyShooter>();

        enemySoundController =
            enemy.GetComponent<EnemySoundController>();
    }


    public override void Enter()
    {
        if (shooter == null)
        {
            Debug.LogError(
                $"[{enemy.name}] ShooterCombatState: " +
                "EnemyShooter is missing."
            );

            return;
        }


        if (enemy.Vision == null)
        {
            Debug.LogError(
                $"[{enemy.name}] ShooterCombatState: " +
                "EnemyVision is missing."
            );

            return;
        }


        shooter.StopMoving();

        attackTimer = 0f;
        attackCount = 0;


        Debug.Log(
            $"[{enemy.name}] State: Combat"
        );
    }


    public override void Tick()
    {
        if (
            shooter == null ||
            enemy.Vision == null
        )
        {
            return;
        }


        Transform player =
            enemy.Vision.Player;


        if (player == null)
            return;


        // ==========================================
        // Низкое здоровье -> TakeCover
        // ==========================================

        if (shooter.NeedsToTakeCover())
        {
            enemy.StateMachine.ChangeState(
                new ShooterTakeCoverState(enemy)
            );

            return;
        }


        // ==========================================
        // Обновляем последнюю известную позицию.
        // ==========================================

        if (enemy.Vision.CanSeePlayer())
        {
            shooter.SetLastKnownPlayerPosition(
                player.position
            );
        }


        // ==========================================
        // Потеряли игрока -> Search
        // ==========================================

        if (!enemy.Vision.CanSeePlayer())
        {
            enemy.StateMachine.ChangeState(
                new ShooterSearchState(enemy)
            );

            return;
        }


        Vector3 direction =
            player.position -
            shooter.transform.position;


        float distance =
            direction.magnitude;


        // ==========================================
        // Игрок слишком далеко.
        // ==========================================

        if (
            distance >
            shooter.AttackDistance + 1f
        )
        {
            MoveToPlayer(player);

            return;
        }


        // ==========================================
        // Игрок слишком близко.
        // ==========================================

        if (
            distance <
            shooter.AttackDistance - 1f
        )
        {
            MoveAwayFromPlayer(player);

            return;
        }


        // ==========================================
        // Идеальная дистанция.
        // ==========================================

        shooter.StopMoving();

        LookAtPlayer(player);

        attackTimer -=
            Time.deltaTime;


        if (attackTimer <= 0f)
        {
            Attack(player);

            attackCount++;

            attackTimer =
                shooter.AttackCooldown;


            if (
                attackCount >=
                AttacksBeforeBackstep
            )
            {
                enemy.StateMachine.ChangeState(
                    new ShooterBackstepState(
                        enemy
                    )
                );

                return;
            }
        }
    }


    // ==========================================
    // PREDICTIVE SHOOTING
    // ==========================================

    private void Attack(
        Transform player)
    {
        // ==========================================
        // ATTACK SOUND
        // ==========================================

        if (enemySoundController != null)
        {
            enemySoundController.PlayAttack();
        }


        Vector3 shooterPosition =
            shooter.transform.position;


        Vector3 playerPosition =
            player.position;


        // ------------------------------------------
        // Получаем скорость игрока.
        // ------------------------------------------

        Vector3 playerVelocity =
            GetPlayerVelocity(
                player
            );


        // ------------------------------------------
        // Начальная дистанция.
        // ------------------------------------------

        float distance =
            Vector3.Distance(
                shooterPosition,
                playerPosition
            );


        // ------------------------------------------
        // Время полёта снаряда.
        //
        // t = distance / speed
        // ------------------------------------------

        float projectileSpeed =
            Mathf.Max(
                shooter.ProjectileSpeed,
                0.01f
            );


        float flightTime =
            distance /
            projectileSpeed;


        // ------------------------------------------
        // Предсказываем позицию игрока.
        //
        // P = P0 + V * t
        // ------------------------------------------

        Vector3 predictedPosition =
            playerPosition +
            playerVelocity *
            flightTime;


        // ------------------------------------------
        // Дополнительный разброс.
        // ------------------------------------------

        Vector3 aimDirection =
            predictedPosition -
            shooterPosition;


        if (
            aimDirection.sqrMagnitude <
            0.001f
        )
        {
            return;
        }


        aimDirection.Normalize();


        aimDirection =
            ApplySpread(
                aimDirection,
                shooter.AimSpread
            );


        // ------------------------------------------
        // Стреляем.
        // ------------------------------------------

        if (
            Physics.Raycast(
                shooterPosition,
                aimDirection,
                out RaycastHit hit,
                shooter.AttackDistance,
                shooter.HitMask,
                QueryTriggerInteraction.Ignore
            )
        )
        {
            IDamageable damageable =
                hit.collider.GetComponent<
                    IDamageable>();


            if (damageable == null)
            {
                damageable =
                    hit.collider.GetComponentInParent<
                        IDamageable>();
            }


            if (damageable != null)
            {
                // ==================================
                // Если цель имеет Health,
                // передаём также позицию источника
                // урона для DamageDirectionIndicator.
                // ==================================

                Health health =
                    hit.collider.GetComponentInParent<
                        Health>();


                if (health != null)
                {
                    health.TakeDamage(
                        shooter.AttackDamage,
                        shooter.transform.position
                    );
                }
                else
                {
                    damageable.TakeDamage(
                        shooter.AttackDamage
                    );
                }


                Debug.Log(
                    $"[{enemy.name}] " +
                    $"Predictive shot HIT player. " +
                    $"Flight time: {flightTime:F2}s"
                );
            }
            else
            {
                Debug.Log(
                    $"[{enemy.name}] Shot hit " +
                    $"{hit.collider.name}, but target " +
                    "is not damageable."
                );
            }
        }
        else
        {
            Debug.Log(
                $"[{enemy.name}] Predictive shot MISS."
            );
        }


        Debug.DrawRay(
            shooterPosition,
            aimDirection *
            shooter.AttackDistance,
            Color.red,
            0.5f
        );
    }


    private Vector3 GetPlayerVelocity(
        Transform player)
    {
        CharacterController controller =
            player.GetComponent<
                CharacterController>();


        if (controller != null)
        {
            return controller.velocity;
        }


        Rigidbody rigidbody =
            player.GetComponent<Rigidbody>();


        if (rigidbody != null)
        {
            return rigidbody.velocity;
        }


        return Vector3.zero;
    }


    private Vector3 ApplySpread(
        Vector3 direction,
        float spreadAngle)
    {
        if (spreadAngle <= 0f)
            return direction;


        Quaternion spreadRotation =
            Quaternion.Euler(
                Random.Range(
                    -spreadAngle,
                    spreadAngle
                ),
                Random.Range(
                    -spreadAngle,
                    spreadAngle
                ),
                0f
            );


        return spreadRotation *
               direction;
    }


    // ==========================================
    // MOVEMENT
    // ==========================================

    private void MoveToPlayer(
        Transform player)
    {
        if (
            shooter.Agent == null ||
            !shooter.Agent.isOnNavMesh
        )
        {
            return;
        }


        shooter.Agent.isStopped =
            false;


        shooter.Agent.SetDestination(
            player.position
        );


        LookAtPlayer(player);
    }


    private void MoveAwayFromPlayer(
        Transform player)
    {
        if (
            shooter.Agent == null ||
            !shooter.Agent.isOnNavMesh
        )
        {
            return;
        }


        Vector3 direction =
            shooter.transform.position -
            player.position;


        direction.y = 0f;


        if (
            direction.sqrMagnitude <
            0.001f
        )
        {
            return;
        }


        direction.Normalize();


        Vector3 targetPosition =
            shooter.transform.position +
            direction * 3f;


        shooter.Agent.isStopped =
            false;


        shooter.Agent.SetDestination(
            targetPosition
        );


        LookAtPlayer(player);
    }


    private void LookAtPlayer(
        Transform player)
    {
        Vector3 direction =
            player.position -
            shooter.transform.position;


        direction.y = 0f;


        if (
            direction.sqrMagnitude <
            0.001f
        )
        {
            return;
        }


        Quaternion targetRotation =
            Quaternion.LookRotation(
                direction
            );


        shooter.transform.rotation =
            Quaternion.Slerp(
                shooter.transform.rotation,
                targetRotation,
                Time.deltaTime * 8f
            );
    }


    public override void Exit()
    {
        if (shooter != null)
        {
            shooter.StopMoving();
        }
    }
}