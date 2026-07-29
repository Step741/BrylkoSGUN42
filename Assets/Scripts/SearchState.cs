using UnityEngine;
using UnityEngine.AI;

public class SearchState : MonoBehaviour
{
    private CharacterAI ai;

    private void Awake()
    {
        ai = GetComponent<CharacterAI>();
    }

    public void Enter()
    {
        ai.Agent.isStopped = false;

        SetRandomDestination();
    }

    public void Tick()
    {
        Transform item = ai.ItemFinder.FindClosestItem(transform.position,ai.SearchRadius);

        if (item != null)
        {
            ai.TargetItem = item;
            ai.ChangeState(CharacterState.Collect);
            return;
        }

        if (!ai.Agent.pathPending && ai.Agent.remainingDistance <= ai.Agent.stoppingDistance)
        {
            SetRandomDestination();
        }
    }

    private void SetRandomDestination()
    {
        Vector3 randomPoint = transform.position + Random.insideUnitSphere * ai.WanderRadius;

        randomPoint.y = transform.position.y;

        NavMeshHit hit;

        if (NavMesh.SamplePosition(randomPoint,out hit,ai.WanderRadius,NavMesh.AllAreas))
        {
            ai.Agent.SetDestination(hit.position);
        }
    }
}