using UnityEngine;

public class EnemyMinimapMarker : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private MinimapMarker minimapMarker;

    [SerializeField]
    private FullscreenMapMarker fullscreenMapMarker;

    [SerializeField]
    private EnemyVision enemyVision;

    [SerializeField]
    private Health health;


    [Header("Settings")]
    [SerializeField]
    [Min(0f)]
    private float hideDelay = 2f;


    private float lastTimeSeen = float.NegativeInfinity;

    private void Awake()
    {
        if (minimapMarker == null)
            minimapMarker = GetComponent<MinimapMarker>();


        if (enemyVision == null)
            enemyVision = GetComponent<EnemyVision>();


        if (health == null)
            health = GetComponent<Health>();
    }


    private void OnEnable()
    {
        if (health != null)
        {
            health.Died += OnEnemyDied;
        }
    }


    private void Start()
    {
        SetMarkerVisible(false);
    }


    private void Update()
    {
        if (health != null && health.IsDead)
        {
            SetMarkerVisible(false);
            return;
        }

        if (enemyVision == null)
        {
            SetMarkerVisible(false);
            return;
        }


        bool canSeePlayer =
            enemyVision.CanSeePlayer();

        if (canSeePlayer)
        {
            lastTimeSeen = Time.time;

            SetMarkerVisible(true);

            return;
        }

        bool shouldRemainVisible =
            Time.time - lastTimeSeen <= hideDelay;


        SetMarkerVisible(
            shouldRemainVisible
        );
    }


    private void OnDisable()
    {
        if (health != null)
        {
            health.Died -= OnEnemyDied;
        }
    }

    private void OnEnemyDied()
    {
        SetMarkerVisible(false);
    }

    private void SetMarkerVisible(bool value)
    {
        if (minimapMarker != null)
        {
            minimapMarker.SetVisible(value);
        }


        if (fullscreenMapMarker != null)
        {
            fullscreenMapMarker.SetVisible(value);
        }
    }
}