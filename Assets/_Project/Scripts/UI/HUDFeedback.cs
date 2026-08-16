using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HUDFeedback : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health health;
    [SerializeField] private RectTransform hudShakeRoot;
    [SerializeField] private Slider healthBar;

    [Header("Damage Shake")]
    [SerializeField] private float shakeDuration = 0.25f;
    [SerializeField] private float shakeStrength = 8f;
    [SerializeField] private int shakeVibrato = 20;

    [Header("Low HP Pulse")]
    [SerializeField, Range(0f, 1f)]
    private float lowHealthPercent = 0.3f;

    [SerializeField] private float pulseScale = 1.08f;
    [SerializeField] private float pulseDuration = 0.35f;

    private float previousHealth;
    private Vector3 originalScale;
    private Tween pulseTween;

    private void Awake()
    {
        if (healthBar != null)
        {
            originalScale = healthBar.transform.localScale;
        }
    }

    private void Start()
    {
        if (health == null)
        {
            Debug.LogError("HUDFeedback: Health reference is missing.");
            return;
        }

        previousHealth = health.CurrentHealth;

        UpdateLowHealthPulse(
            health.CurrentHealth,
            health.MaxHealth
        );
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.HealthChanged += OnHealthChanged;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.HealthChanged -= OnHealthChanged;
        }

        pulseTween?.Kill();

        if (healthBar != null)
        {
            healthBar.transform.localScale = originalScale;
        }
    }

    private void OnHealthChanged(
        float currentHealth,
        float maxHealth)
    {
        if (currentHealth < previousHealth)
        {
            ShakeHUD();
        }

        UpdateLowHealthPulse(
            currentHealth,
            maxHealth
        );

        previousHealth = currentHealth;
    }

    private void ShakeHUD()
    {
        if (hudShakeRoot == null)
            return;

        hudShakeRoot.DOKill();

        hudShakeRoot.DOShakeAnchorPos(
            shakeDuration,
            shakeStrength,
            shakeVibrato,
            90f,
            false,
            true
        );
    }

    private void UpdateLowHealthPulse(
        float currentHealth,
        float maxHealth)
    {
        if (healthBar == null || maxHealth <= 0f)
            return;

        float healthPercent = currentHealth / maxHealth;

        if (healthPercent <= lowHealthPercent)
        {
            StartLowHealthPulse();
        }
        else
        {
            StopLowHealthPulse();
        }
    }

    private void StartLowHealthPulse()
    {
        if (pulseTween != null && pulseTween.IsActive())
            return;

        pulseTween = healthBar.transform
            .DOScale(
                originalScale * pulseScale,
                pulseDuration
            )
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void StopLowHealthPulse()
    {
        if (pulseTween == null)
            return;

        pulseTween.Kill();
        pulseTween = null;

        healthBar.transform
            .DOScale(
                originalScale,
                pulseDuration
            )
            .SetEase(Ease.OutSine);
    }
}