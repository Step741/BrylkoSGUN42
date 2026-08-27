using System.Collections;
using UnityEngine;


[RequireComponent(typeof(TrailRenderer))]
public class GrenadeTrailController : MonoBehaviour
{
    // =========================================================
    // SETTINGS
    // =========================================================

    [Header("Trail Settings")]

    [SerializeField]
    [Min(0f)]
    private float startDelay = 0.5f;


    // =========================================================
    // COMPONENTS
    // =========================================================

    private TrailRenderer trail;


    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        trail =
            GetComponent<TrailRenderer>();
    }


    private void OnEnable()
    {
        StopAllCoroutines();

        DisableAndClearTrail();

        StartCoroutine(
            EnableTrailAfterDelay()
        );
    }


    private void OnDisable()
    {
        StopAllCoroutines();

        DisableAndClearTrail();
    }


    // =========================================================
    // ENABLE DELAY
    // =========================================================

    private IEnumerator EnableTrailAfterDelay()
    {
        if (startDelay > 0f)
        {
            yield return new WaitForSeconds(
                startDelay
            );
        }

        if (trail == null)
            yield break;


        trail.Clear();

        trail.emitting =
            true;
    }


    // =========================================================
    // CLEAR
    // =========================================================

    private void DisableAndClearTrail()
    {
        if (trail == null)
            return;


        trail.emitting =
            false;

        trail.Clear();
    }
}