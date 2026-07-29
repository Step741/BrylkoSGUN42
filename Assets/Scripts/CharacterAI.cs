using UnityEngine;
using UnityEngine.AI;
using Zenject;

public class CharacterAI : MonoBehaviour
{
    [SerializeField]
    private float searchRadius = 5f;

    [SerializeField]
    private float wanderRadius = 8f;

    [SerializeField]
    private float idleTime = 5f;

    private NavMeshAgent agent;

    private IdleState idleState;
    private SearchState searchState;
    private CollectState collectState;

    private CharacterState currentState;

    private Transform targetItem;

    private IItemFinder itemFinder;

    public NavMeshAgent Agent => agent;

    public float SearchRadius => searchRadius;

    public float WanderRadius => wanderRadius;

    public float IdleTime => idleTime;

    public Transform TargetItem
    {
        get => targetItem;
        set => targetItem = value;
    }

    public IItemFinder ItemFinder => itemFinder;

    public CharacterState CurrentState => currentState;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        idleState = GetComponent<IdleState>();
        searchState = GetComponent<SearchState>();
        collectState = GetComponent<CollectState>();
    }

    [Inject]
    public void Construct(IItemFinder itemFinder)
    {
        this.itemFinder = itemFinder;
    }

    private void Start()
    {
        ChangeState(CharacterState.Idle);
    }

    public void ChangeState(CharacterState newState)
    {
        currentState = newState;

        switch (currentState)
        {
            case CharacterState.Idle:
                idleState.Enter();
                break;

            case CharacterState.Search:
                searchState.Enter();
                break;

            case CharacterState.Collect:
                collectState.Enter();
                break;
        }
    }

    private void Update()
    {
        switch (currentState)
        {
            case CharacterState.Idle:
                idleState.Tick();
                break;

            case CharacterState.Search:
                searchState.Tick();
                break;

            case CharacterState.Collect:
                collectState.Tick();
                break;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, searchRadius);
    }
}