using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class PlayerAudioEffects : MonoBehaviour
{
    [Header("References")]

    [SerializeField]
    private Health health;

    [SerializeField]
    private AudioMixer audioMixer;


    [Header("Mixer Parameters")]

    [SerializeField]
    private string sfxLowPassParameter =
        "SFXLowPassCutoff";

    [SerializeField]
    private string ambientLowPassParameter =
        "AmbientLowPassCutoff";


    [Header("Low Health Settings")]

    [Range(0f, 1f)]
    [SerializeField]
    private float lowHealthThreshold = 0.4f;

    [SerializeField]
    private float normalCutoff = 22000f;

    [SerializeField]
    private float criticalHealthCutoff = 1200f;


    [Header("Health Smoothing")]

    [SerializeField]
    [Min(0.01f)]
    private float transitionSpeed = 4f;


    [Header("Explosion Audio Effect")]

    [SerializeField]
    private float explosionCriticalCutoff = 300f;

    [SerializeField]
    [Min(0f)]
    private float explosionHoldDuration = 0.6f;

    [SerializeField]
    [Min(0.01f)]
    private float explosionRecoveryDuration = 2.5f;

    [Range(0f, 1f)]
    [SerializeField]
    private float minimumExplosionIntensity = 0.05f;


    private float healthTargetCutoff;
    private float explosionCutoff;

    private float currentCutoff;


    private Coroutine explosionRoutine;


    // ==========================================
    // INITIALIZATION
    // ==========================================

    private void Awake()
    {
        if (health == null)
        {
            health =
                GetComponent<Health>();
        }


        healthTargetCutoff =
            normalCutoff;

        explosionCutoff =
            normalCutoff;

        currentCutoff =
            normalCutoff;


        ApplyLowPass(
            normalCutoff
        );
    }


    private void OnEnable()
    {
        if (health != null)
        {
            health.HealthChanged +=
                HandleHealthChanged;
        }
    }


    private void Start()
    {
        if (health == null)
        {
            ApplyLowPass(
                normalCutoff
            );

            return;
        }


        UpdateLowHealthEffect(
            health.CurrentHealth,
            health.MaxHealth
        );
    }


    private void OnDisable()
    {
        if (health != null)
        {
            health.HealthChanged -=
                HandleHealthChanged;
        }


        if (explosionRoutine != null)
        {
            StopCoroutine(
                explosionRoutine
            );

            explosionRoutine =
                null;
        }


        ApplyLowPass(
            normalCutoff
        );
    }


    private void Update()
    {
        if (audioMixer == null)
            return;


        // Берём наиболее сильный
        // из двух эффектов.
        float finalTargetCutoff =
            Mathf.Min(
                healthTargetCutoff,
                explosionCutoff
            );


        currentCutoff =
            Mathf.Lerp(
                currentCutoff,
                finalTargetCutoff,
                transitionSpeed *
                Time.deltaTime
            );


        ApplyLowPass(
            currentCutoff
        );
    }


    // ==========================================
    // HEALTH LOW PASS
    // ==========================================

    private void HandleHealthChanged(
        float currentHealth,
        float maxHealth)
    {
        UpdateLowHealthEffect(
            currentHealth,
            maxHealth
        );
    }


    private void UpdateLowHealthEffect(
        float currentHealth,
        float maxHealth)
    {
        if (maxHealth <= 0f)
        {
            healthTargetCutoff =
                normalCutoff;

            return;
        }


        float healthPercent =
            currentHealth /
            maxHealth;


        if (
            healthPercent >=
            lowHealthThreshold
        )
        {
            healthTargetCutoff =
                normalCutoff;

            return;
        }


        float lowHealthProgress =
            Mathf.InverseLerp(
                lowHealthThreshold,
                0f,
                healthPercent
            );


        healthTargetCutoff =
            Mathf.Lerp(
                normalCutoff,
                criticalHealthCutoff,
                lowHealthProgress
            );
    }


    // ==========================================
    // EXPLOSION EFFECT
    // ==========================================

    public void PlayExplosionEffect(
        float intensity)
    {
        intensity =
            Mathf.Clamp01(
                intensity
            );


        if (
            intensity <
            minimumExplosionIntensity
        )
        {
            return;
        }


        if (explosionRoutine != null)
        {
            StopCoroutine(
                explosionRoutine
            );
        }


        explosionRoutine =
            StartCoroutine(
                ExplosionEffectRoutine(
                    intensity
                )
            );
    }


    private IEnumerator ExplosionEffectRoutine(
        float intensity)
    {
        float targetExplosionCutoff =
            Mathf.Lerp(
                normalCutoff,
                explosionCriticalCutoff,
                intensity
            );


        // ==========================================
        // МГНОВЕННЫЙ УДАР ОТ ВЗРЫВА
        // ==========================================

        explosionCutoff =
            targetExplosionCutoff;


        // Сразу применяем сильнейший
        // из активных эффектов,
        // не ожидая общего сглаживания.
        currentCutoff =
            Mathf.Min(
                healthTargetCutoff,
                explosionCutoff
            );


        ApplyLowPass(
            currentCutoff
        );


        // ==========================================
        // HOLD
        // ==========================================

        if (explosionHoldDuration > 0f)
        {
            yield return new WaitForSeconds(
                explosionHoldDuration
            );
        }


        // ==========================================
        // RECOVERY
        // ==========================================

        float startCutoff =
            explosionCutoff;

        float timer =
            0f;


        while (
            timer <
            explosionRecoveryDuration
        )
        {
            timer +=
                Time.deltaTime;


            float progress =
                Mathf.Clamp01(
                    timer /
                    explosionRecoveryDuration
                );


            explosionCutoff =
                Mathf.Lerp(
                    startCutoff,
                    normalCutoff,
                    progress
                );


            yield return null;
        }


        explosionCutoff =
            normalCutoff;

        explosionRoutine =
            null;
    }


    // ==========================================
    // MIXER
    // ==========================================

    private void ApplyLowPass(
        float cutoff)
    {
        if (audioMixer == null)
            return;


        audioMixer.SetFloat(
            sfxLowPassParameter,
            cutoff
        );


        audioMixer.SetFloat(
            ambientLowPassParameter,
            cutoff
        );
    }


    // ==========================================
    // CLEANUP
    // ==========================================

    private void OnDestroy()
    {
        ApplyLowPass(
            normalCutoff
        );
    }
}