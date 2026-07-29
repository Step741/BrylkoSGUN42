using UnityEngine;

public class CollectState : MonoBehaviour
{
    private CharacterAI ai;

    [SerializeField]
    private float collectDistance = 1.2f;

    private void Awake()
    {
        ai = GetComponent<CharacterAI>();
    }

    public void Enter()
    {
        ai.Agent.isStopped = false;

        if (ai.TargetItem != null)
        {
            ai.Agent.SetDestination(ai.TargetItem.position);
        }
    }

    public void Tick()
    {
        if (ai.TargetItem == null)
        {
            ai.ChangeState(CharacterState.Idle);
            return;
        }

        ai.Agent.SetDestination(ai.TargetItem.position);

        float distance = Vector3.Distance(transform.position,ai.TargetItem.position);

        if (distance <= collectDistance)
        {
            Destroy(ai.TargetItem.gameObject);

            ai.TargetItem = null;

            ai.ChangeState(CharacterState.Idle);
        }
    }
}