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

    public void Show(string text)
    {
        if (promptText != null)
        {
            promptText.text = text;
        }

        fadeTween?.Kill();
        scaleTween?.Kill();

        gameObject.SetActive(true);

        if (canvasGroup != null)
        {
            fadeTween =
                canvasGroup
                    .DOFade(
                        1f,
                        showDuration
                    )
                    .SetEase(Ease.OutQuad);
        }

        transform.localScale =
            originalScale * startScale;

        scaleTween =
            transform
                .DOScale(
                    originalScale,
                    showDuration
                )
                .SetEase(Ease.OutBack);
    }

    public void Hide()
    {
        fadeTween?.Kill();
        scaleTween?.Kill();

        if (canvasGroup == null)
        {
            gameObject.SetActive(false);
            return;
        }

        fadeTween =
            canvasGroup
                .DOFade(
                    0f,
                    hideDuration
                )
                .SetEase(Ease.InQuad)
                .OnComplete(() =>
                {
                    gameObject.SetActive(false);
                });

        scaleTween =
            transform
                .DOScale(
                    originalScale * startScale,
                    hideDuration
                )
                .SetEase(Ease.InQuad);
    }

    private void OnDisable()
    {
        fadeTween?.Kill();
        scaleTween?.Kill();
    }

    private void OnDestroy()
    {
        fadeTween?.Kill();
        scaleTween?.Kill();
    }
}