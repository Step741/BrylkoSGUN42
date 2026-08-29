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


    private void OnDisable()
    {
        KillTweens();
    }


    private void OnDestroy()
    {
        KillTweens();
    }

    public void Show(string text)
    {
        if (promptText != null)
        {
            promptText.text = text;
        }


        //Останавливает предыдущие анимации
        KillTweens();

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

    public void Hide()
    {
        KillTweens();


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

                        if (this != null)
                        {
                            gameObject.SetActive(false);
                        }
                    }
                );

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

    //CLEANUP
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