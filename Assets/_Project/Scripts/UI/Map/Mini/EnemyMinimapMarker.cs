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


    // =========================================================
    // UNITY
    // =========================================================

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
        // В начале игры враг ещё не обнаружил игрока.
        SetMarkerVisible(false);
    }


    private void Update()
    {
        // =====================================================
        // DEATH CHECK
        // =====================================================

        if (health != null && health.IsDead)
        {
            SetMarkerVisible(false);
            return;
        }


        // =====================================================
        // VISION CHECK
        // =====================================================

        if (enemyVision == null)
        {
            SetMarkerVisible(false);
            return;
        }


        bool canSeePlayer =
            enemyVision.CanSeePlayer();


        // -----------------------------------------------------
        // Враг видит игрока.
        // -----------------------------------------------------

        if (canSeePlayer)
        {
            lastTimeSeen = Time.time;

            SetMarkerVisible(true);

            return;
        }


        // -----------------------------------------------------
        // Враг потерял игрока.
        //
        // Маркер остаётся видимым hideDelay секунд
        // после последнего обнаружения.
        // -----------------------------------------------------

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


    // =========================================================
    // DEATH
    // =========================================================

    private void OnEnemyDied()
    {
        SetMarkerVisible(false);
    }


    // =========================================================
    // VISIBILITY
    // =========================================================

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