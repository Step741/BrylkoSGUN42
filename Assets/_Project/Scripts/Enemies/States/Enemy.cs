using UnityEngine;
using Zenject;

public class Enemy : MonoBehaviour
{
    [SerializeField] private EnemyVision vision;

    private EnemyStateMachine stateMachine;
    private bool isDead;

    public EnemyStateMachine StateMachine => stateMachine;
    public EnemyVision Vision => vision;
    public bool IsDead => isDead;

    [Inject]
    private void Construct(EnemyStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    private void Start()
    {
        EnemyShooter shooter =
            GetComponent<EnemyShooter>();

        if (shooter != null)
        {
            stateMachine.ChangeState(
                new ShooterPatrolState(this)
            );

            return;
        }

        EnemyMelee melee =
            GetComponent<EnemyMelee>();

        if (melee != null)
        {
            stateMachine.ChangeState(
                new MeleeIdleState(this)
            );

            return;
        }
    }

    private void Update()
    {
        if (isDead)
            return;

        stateMachine.Tick();
    }

    public void MarkAsDead()
    {
        isDead = true;
    }

    private void OnDestroy()
    {
        stateMachine?.Stop();
    }
}