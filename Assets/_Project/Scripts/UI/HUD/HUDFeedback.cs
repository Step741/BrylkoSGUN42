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
    private Vector2 originalShakePosition;

    private Tween shakeTween;
    private Tween pulseTween;
    private Tween returnScaleTween;

    private void Awake()
    {
        if (healthBar != null)
        {
            originalScale =
                healthBar.transform.localScale;
        }

        if (hudShakeRoot != null)
        {
            originalShakePosition =
                hudShakeRoot.anchoredPosition;
        }
    }


    private void Start()
    {
        if (health == null)
        {
            return;
        }


        previousHealth =
            health.CurrentHealth;


        UpdateLowHealthPulse(
            health.CurrentHealth,
            health.MaxHealth
        );
    }


    private void OnEnable()
    {
        if (health != null)
        {
            health.HealthChanged +=
                OnHealthChanged;
        }
    }


    private void OnDisable()
    {
        if (health != null)
        {
            health.HealthChanged -=
                OnHealthChanged;
        }

        KillAllTweens();

        ResetVisualState();
    }


    private void OnDestroy()
    {
        KillAllTweens();
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


        previousHealth =
            currentHealth;
    }

    private void ShakeHUD()
    {
        if (hudShakeRoot == null)
            return;

        hudShakeRoot.DOKill();

        shakeTween?.Kill();


        shakeTween =
            hudShakeRoot
                .DOShakeAnchorPos(
                    shakeDuration,
                    shakeStrength,
                    shakeVibrato,
                    90f,
                    false,
                    true
                )
                .SetLink(
                    hudShakeRoot.gameObject
                )
                .OnKill(
                    () =>
                    {
                        shakeTween = null;
                    }
                )
                .OnComplete(
                    () =>
                    {
                        shakeTween = null;

                        if (hudShakeRoot != null)
                        {
                            hudShakeRoot.anchoredPosition =
                                originalShakePosition;
                        }
                    }
                );
    }

    private void UpdateLowHealthPulse(
        float currentHealth,
        float maxHealth)
    {
        if (
            healthBar == null ||
            maxHealth <= 0f
        )
        {
            return;
        }


        float healthPercent =
            currentHealth /
            maxHealth;


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
        if (healthBar == null)
            return;


        if (
            pulseTween != null &&
            pulseTween.IsActive()
        )
        {
            return;
        }

        returnScaleTween?.Kill();
        returnScaleTween = null;


        healthBar.transform.DOKill();


        pulseTween =
            healthBar.transform
                .DOScale(
                    originalScale * pulseScale,
                    pulseDuration
                )
                .SetLoops(
                    -1,
                    LoopType.Yoyo
                )
                .SetEase(
                    Ease.InOutSine
                )
                .SetLink(
                    healthBar.gameObject
                )
                .OnKill(
                    () =>
                    {
                        pulseTween = null;
                    }
                );
    }


    private void StopLowHealthPulse()
    {
        if (healthBar == null)
            return;


        if (
            pulseTween != null &&
            pulseTween.IsActive()
        )
        {
            pulseTween.Kill();
        }

        pulseTween = null;


        healthBar.transform.DOKill();


        returnScaleTween =
            healthBar.transform
                .DOScale(
                    originalScale,
                    pulseDuration
                )
                .SetEase(
                    Ease.OutSine
                )
                .SetLink(
                    healthBar.gameObject
                )
                .OnKill(
                    () =>
                    {
                        returnScaleTween = null;
                    }
                )
                .OnComplete(
                    () =>
                    {
                        returnScaleTween = null;
                    }
                );
    }

    // CLEANUP
    private void KillAllTweens()
    {
        shakeTween?.Kill();
        shakeTween = null;


        pulseTween?.Kill();
        pulseTween = null;


        returnScaleTween?.Kill();
        returnScaleTween = null;


        if (hudShakeRoot != null)
        {
            hudShakeRoot.DOKill();
        }


        if (healthBar != null)
        {
            healthBar.transform.DOKill();
        }
    }


    private void ResetVisualState()
    {
        if (hudShakeRoot != null)
        {
            hudShakeRoot.anchoredPosition =
                originalShakePosition;
        }


        if (healthBar != null)
        {
            healthBar.transform.localScale =
                originalScale;
        }
    }
}