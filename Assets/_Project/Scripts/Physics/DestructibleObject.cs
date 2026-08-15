using UnityEngine;

[RequireComponent(typeof(Health))]
public class DestructibleObject : MonoBehaviour
{
    [Header("Destruction")]
    [SerializeField] private GameObject destructionPrefab;
    [SerializeField] private float debrisLifetime = 5f;

    private Health health;
    private bool destroyed;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        if (health == null)
            health = GetComponent<Health>();

        health.Died += OnDied;
    }

    private void OnDisable()
    {
        if (health != null)
            health.Died -= OnDied;
    }

    private void OnDied()
    {
        if (destroyed)
            return;

        destroyed = true;

        SpawnDebris();

        Destroy(gameObject);
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

        DebrisPiece[] pieces = debris.GetComponentsInChildren<DebrisPiece>();

        foreach (DebrisPiece piece in pieces)
        {
            piece.Explode(transform.position);
        }

        Destroy(debris, debrisLifetime);
    }
}