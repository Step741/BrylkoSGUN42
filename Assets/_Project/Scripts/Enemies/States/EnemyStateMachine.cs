using UnityEngine;

public class EnemyStateMachine
{
    private EnemyState currentState;

    public EnemyState CurrentState => currentState;

    public void ChangeState(EnemyState newState)
    {
        if (newState == null)
        {
            Debug.LogError("EnemyStateMachine: new state is null.");
            return;
        }

        currentState?.Exit();

        currentState = newState;
        currentState.Enter();
    }

    public void Tick()
    {
        currentState?.Tick();
    }

    public void Stop()
    {
        currentState?.Exit();
        currentState = null;
    }
}