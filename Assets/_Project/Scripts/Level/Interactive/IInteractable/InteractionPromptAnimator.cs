using DG.Tweening;
using TMPro;
using UnityEngine;

public class InteractionPromptAnimator : MonoBehaviour
{
    [Header("References")]

    [SerializeField]
    private CanvasGroup canvasGroup;

    [SerializeField]
    private TMP_Text promptText;


    [Header("Show Animation")]

    [SerializeField]
    private float showDuration = 0.18f;

    [SerializeField]
    private float startScale = 0.85f;


    [Header("Hide Animation")]

    [SerializeField]
    private float hideDuration = 0.12f;


    private Vector3 originalScale;

    private Tween fadeTween;
    private Tween scaleTween;


    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        originalScale =
            transform.localScale;


        if (canvasGroup == null)
        {
            canvasGroup =
                GetComponent<CanvasGroup>();
        }


        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }


        transform.localScale =
            originalScale * startScale;
    }


    private void OnDisable()
    {
        KillTweens();
    }


    private void OnDestroy()
    {
        KillTweens();
    }


    // =========================================================
    // SHOW
    // =========================================================

    public void Show(string text)
    {
        if (promptText != null)
        {
            promptText.text = text;
        }


        // Останавливаем предыдущие анимации.
        KillTweens();


        // Если объект был скрыт после Hide(),
        // снова включаем его перед запуском анимации.
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }


        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }


        transform.localScale =
            originalScale * startScale;


        // =====================================================
        // FADE IN
        // =====================================================

        if (canvasGroup != null)
        {
            fadeTween =
                canvasGroup
                    .DOFade(
                        1f,
                        showDuration
                    )
                    .SetEase(
                        Ease.OutQuad
                    )
                    .SetLink(
                        gameObject
                    )
                    .OnKill(
                        () =>
                        {
                            fadeTween = null;
                        }
                    )
                    .OnComplete(
                        () =>
                        {
                            fadeTween = null;
                        }
                    );
        }


        // =====================================================
        // SCALE IN
        // =====================================================

        scaleTween =
            transform
                .DOScale(
                    originalScale,
                    showDuration
                )
                .SetEase(
                    Ease.OutBack
                )
                .SetLink(
                    gameObject
                )
                .OnKill(
                    () =>
                    {
                        scaleTween = null;
                    }
                )
                .OnComplete(
                    () =>
                    {
                        scaleTween = null;
                    }
                );
    }


    // =========================================================
    // HIDE
    // =========================================================

    public void Hide()
    {
        KillTweens();


        if (canvasGroup == null)
        {
            gameObject.SetActive(false);
            return;
        }


        // =====================================================
        // FADE OUT
        // =====================================================

        fadeTween =
            canvasGroup
                .DOFade(
                    0f,
                    hideDuration
                )
                .SetEase(
                    Ease.InQuad
                )
                .SetLink(
                    gameObject
                )
                .OnKill(
                    () =>
                    {
                        fadeTween = null;
                    }
                )
                .OnComplete(
                    () =>
                    {
                        fadeTween = null;


                        // Если объект всё ещё существует,
                        // отключаем его после анимации.
                        if (this != null)
                        {
                            gameObject.SetActive(false);
                        }
                    }
                );


        // =====================================================
        // SCALE OUT
        // =====================================================

        scaleTween =
            transform
                .DOScale(
                    originalScale * startScale,
                    hideDuration
                )
                .SetEase(
                    Ease.InQuad
                )
                .SetLink(
                    gameObject
                )
                .OnKill(
                    () =>
                    {
                        scaleTween = null;
                    }
                )
                .OnComplete(
                    () =>
                    {
                        scaleTween = null;
                    }
                );
    }


    // =========================================================
    // CLEANUP
    // =========================================================

    private void KillTweens()
    {
        if (
            fadeTween != null &&
            fadeTween.IsActive()
        )
        {
            fadeTween.Kill();
        }

        fadeTween = null;


        if (
            scaleTween != null &&
            scaleTween.IsActive()
        )
        {
            scaleTween.Kill();
        }

        scaleTween = null;


        if (canvasGroup != null)
        {
            canvasGroup.DOKill();
        }


        transform.DOKill();
    }
}