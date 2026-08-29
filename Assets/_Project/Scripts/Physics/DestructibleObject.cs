using UnityEngine;
using Zenject;

[RequireComponent(typeof(Health))]
public class DestructibleObject : MonoBehaviour
{
    [Header("Destruction")]
    [SerializeField]
    private GameObject destructionPrefab;

    [SerializeField]
    private float debrisLifetime = 5f;

    [Header("Drop")]
    [SerializeField]
    private DropConfig dropConfig;

    private Health health;
    private DropTable dropTable;

    private bool destroyed;

    [Inject]
    private void Construct(DropTable dropTable)
    {
        this.dropTable = dropTable;
    }

    private void Awake()
    {
        health = GetComponent<Health>();
    }


    private void OnEnable()
    {
        if (health == null)
        {
            health = GetComponent<Health>();
        }

        if (health != null)
        {
            health.Died += OnDied;
        }
    }


    private void OnDisable()
    {
        if (health != null)
        {
            health.Died -= OnDied;
        }
    }

    private void OnDied()
    {
        if (destroyed)
            return;

        destroyed = true;

        SpawnDrop();

        SpawnDebris();

        Destroy(gameObject);
    }

    private void SpawnDrop()
    {
        if (dropTable == null)
        {
            return;
        }

        if (dropConfig == null)
        {
            return;
        }

        dropTable.Roll(
            dropConfig,
            transform.position
        );
    }

    private void SpawnDebris()
    {
        if (destructionPrefab == null)
            return;


        GameObject debris = Instantiate(
            destructionPrefab,
            transform.position,
            transform.rotation
        );


        DebrisPiece[] pieces =
            debris.GetComponentsInChildren<DebrisPiece>();


        foreach (DebrisPiece piece in pieces)
        {
            piece.Explode(transform.position);
        }


        Destroy(
            debris,
            debrisLifetime
        );
    }
}