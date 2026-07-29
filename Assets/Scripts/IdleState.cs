using UnityEngine;

public class IdleState : MonoBehaviour
{
    private CharacterAI ai;

    private float timer;

    private void Awake()
    {
        ai = GetComponent<CharacterAI>();
    }

    public void Enter()
    {
        timer = ai.IdleTime;

        ai.Agent.isStopped = true;
    }

    public void Tick()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            ai.ChangeState(CharacterState.Search);
        }
    }
}