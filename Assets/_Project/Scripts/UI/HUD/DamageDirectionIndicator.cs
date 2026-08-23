using UnityEngine;
using DG.Tweening;

public class DamageDirectionIndicator : MonoBehaviour
{
    [Header("References")]

    [SerializeField]
    private Health playerHealth;

    [SerializeField]
    private Transform playerTransform;

    [SerializeField]
    private Camera playerCamera;

    [SerializeField]
    private RectTransform indicator;

    [SerializeField]
    private CanvasGroup canvasGroup;


    [Header("Settings")]

    [SerializeField]
    [Tooltip("Как долго индикатор остаётся видимым.")]
    private float displayTime = 0.8f;

    [SerializeField]
    [Tooltip("Длительность плавного появления.")]
    private float fadeInDuration = 0.08f;

    [SerializeField]
    [Tooltip("Длительность плавного исчезновения.")]
    private float fadeOutDuration = 0.25f;

    [SerializeField]
    private float distanceFromCenter = 400f;


    [Header("DOTween Punch")]

    [SerializeField]
    [Tooltip("Сила увеличения индикатора при попадании.")]
    private float punchScale = 0.12f;

    [SerializeField]
    [Tooltip("Длительность эффекта.")]
    private float punchDuration = 0.18f;


    private Sequence damageSequence;


    private void Awake()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        if (indicator != null)
        {
            indicator.localScale = Vector3.one;
        }
    }


    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.DamageReceived +=
                ShowDamageDirection;
        }
    }


    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.DamageReceived -=
                ShowDamageDirection;
        }

        KillAnimation();
    }


    // =========================================================
    // DAMAGE DIRECTION
    // =========================================================

    private void ShowDamageDirection(
        Vector3 damageSourcePosition
    )
    {
        if (
            playerTransform == null ||
            indicator == null ||
            playerCamera == null ||
            canvasGroup == null
        )
        {
            return;
        }


        // -----------------------------------------------------
        // ОРИГИНАЛЬНАЯ ЛОГИКА НАПРАВЛЕНИЯ
        // -----------------------------------------------------

        Vector3 worldDirection =
            damageSourcePosition -
            playerTransform.position;

        worldDirection.y = 0f;

        if (worldDirection.sqrMagnitude < 0.001f)
        {
            return;
        }

        worldDirection.Normalize();


        Vector3 cameraForward =
            playerCamera.transform.forward;

        Vector3 cameraRight =
            playerCamera.transform.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();


        float forwardDot =
            Vector3.Dot(
                cameraForward,
                worldDirection
            );

        float rightDot =
            Vector3.Dot(
                cameraRight,
                worldDirection
            );


        Vector2 screenDirection =
            new Vector2(
                rightDot,
                forwardDot
            ).normalized;


        indicator.anchoredPosition =
            screenDirection *
            distanceFromCenter;


        float angle =
            Mathf.Atan2(
                screenDirection.y,
                screenDirection.x
            ) *
            Mathf.Rad2Deg;


        indicator.localRotation =
            Quaternion.Euler(
                0f,
                0f,
                angle - 90f
            );


        // -----------------------------------------------------
        // DOTWEEN ANIMATION
        // -----------------------------------------------------

        PlayDamageAnimation();
    }


    // =========================================================
    // ANIMATION
    // =========================================================

    private void PlayDamageAnimation()
    {
        // Останавливаем только предыдущую
        // последовательность.
        if (damageSequence != null &&
            damageSequence.IsActive())
        {
            damageSequence.Kill();
        }


        // ВАЖНО:
        // Не вызываем canvasGroup.DOKill()
        // или indicator.DOKill(), потому что
        // tween'ы будут принадлежать Sequence.

        canvasGroup.alpha = 0f;

        indicator.localScale =
            Vector3.one;


        damageSequence =
            DOTween.Sequence();


        // Появление.
        damageSequence.Append(
            canvasGroup
                .DOFade(
                    1f,
                    fadeInDuration
                )
                .SetEase(
                    Ease.OutQuad
                )
        );


        // Punch запускаем отдельно внутри
        // этой же последовательности.
        damageSequence.Join(
            indicator
                .DOPunchScale(
                    Vector3.one * punchScale,
                    punchDuration,
                    1,
                    0.5f
                )
        );


        // Время отображения.
        damageSequence.AppendInterval(
            displayTime
        );


        // Исчезновение.
        damageSequence.Append(
            canvasGroup
                .DOFade(
                    0f,
                    fadeOutDuration
                )
                .SetEase(
                    Ease.InQuad
                )
        );


        damageSequence.OnKill(
            () =>
            {
                damageSequence = null;
            }
        );


        damageSequence.OnComplete(
            () =>
            {
                damageSequence = null;
            }
        );
    }


    // =========================================================
    // CLEANUP
    // =========================================================

    private void KillAnimation()
    {
        if (damageSequence != null &&
            damageSequence.IsActive())
        {
            damageSequence.Kill();
        }

        damageSequence = null;


        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }


        if (indicator != null)
        {
            indicator.localScale =
                Vector3.one;
        }
    }
}