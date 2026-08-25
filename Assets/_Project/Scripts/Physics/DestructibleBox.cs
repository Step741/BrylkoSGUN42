using UnityEngine;
using Zenject;

[RequireComponent(typeof(Health))]
public class DestructibleBox : MonoBehaviour
{
    [Header("Destruction")]
    [SerializeField]
    private GameObject destructionPrefab;

    [SerializeField]
    private float debrisLifetime = 5f;


    [Header("Break Sound")]
    [SerializeField]
    private AudioClip breakSound;

    [SerializeField]
    [Range(0f, 1f)]
    private float soundVolume = 1f;

    [SerializeField]
    private float soundPitch = 1f;


    [Header("3D Sound")]
    [SerializeField]
    private float minDistance = 3f;

    [SerializeField]
    private float maxDistance = 20f;


    [Header("Drop")]
    [SerializeField]
    private DropConfig dropConfig;


    private Health health;
    private DropTable dropTable;

    private bool destroyed;


    // =========================================================
    // ZENJECT
    // =========================================================

    [Inject]
    private void Construct(DropTable dropTable)
    {
        this.dropTable = dropTable;
    }


    // =========================================================
    // UNITY
    // =========================================================

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


    // =========================================================
    // DESTRUCTION
    // =========================================================

    private void OnDied()
    {
        if (destroyed)
            return;

        destroyed = true;


        // -----------------------------------------------------
        // DROP
        // -----------------------------------------------------

        SpawnDrop();


        // -----------------------------------------------------
        // BREAK SOUND
        // -----------------------------------------------------

        PlayBreakSound();


        // -----------------------------------------------------
        // DEBRIS
        // -----------------------------------------------------

        SpawnDebris();

        Destroy(gameObject);
    }


    // =========================================================
    // BREAK SOUND
    // =========================================================

    private void PlayBreakSound()
    {
        if (breakSound == null)
            return;

        if (SoundService.Instance == null)
            return;

        SoundService.Instance.Play3D(
            breakSound,
            transform.position,
            SoundType.SFX,
            soundVolume,
            soundPitch,
            minDistance,
            maxDistance
        );
    }


    // =========================================================
    // DROP
    // =========================================================

    private void SpawnDrop()
    {
        if (dropTable == null)
        {
            Debug.LogWarning(
                $"[{name}] DropTable is not injected.",
                this
            );

            return;
        }

        if (dropConfig == null)
        {
            Debug.LogWarning(
                $"[{name}] DropConfig is not assigned.",
                this
            );

            return;
        }

        dropTable.Roll(
            dropConfig,
            transform.position
        );
    }


    // =========================================================
    // DEBRIS
    // =========================================================

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