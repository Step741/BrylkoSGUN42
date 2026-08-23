using DG.Tweening;
using UnityEngine;

public class KeyCardHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private PlayerInventory playerInventory;

    [SerializeField]
    private GameObject keyCardIcon;

    [SerializeField]
    private CanvasGroup canvasGroup;

    [Header("Animation")]
    [SerializeField]
    private float showDuration = 0.25f;

    [SerializeField]
    private float hideDuration = 0.2f;

    [SerializeField]
    private float startScale = 0.8f;

    private Vector3 originalScale;

    private Tween fadeTween;
    private Tween scaleTween;


    private void Awake()
    {
        originalScale = keyCardIcon.transform.localScale;

        keyCardIcon.SetActive(false);
    }


    private void OnEnable()
    {
        if (playerInventory != null)
        {
            playerInventory.KeyCardChanged += OnKeyCardChanged;
        }
    }


    private void Start()
    {
        if (playerInventory != null)
        {
            OnKeyCardChanged(
                playerInventory.HasKeyCard
            );
        }
    }


    private void OnDisable()
    {
        if (playerInventory != null)
        {
            playerInventory.KeyCardChanged -= OnKeyCardChanged;
        }

        KillTweens();
    }


    private void OnKeyCardChanged(bool hasKeyCard)
    {
        if (hasKeyCard)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }


    private void Show()
    {
        fadeTween?.Kill();
        scaleTween?.Kill();

        keyCardIcon.SetActive(true);

        canvasGroup.alpha = 0f;

        keyCardIcon.transform.localScale =
            originalScale * startScale;

        fadeTween =
            canvasGroup
                .DOFade(1f, showDuration)
                .SetEase(Ease.OutQuad);

        scaleTween =
            keyCardIcon.transform
                .DOScale(
                    originalScale,
                    showDuration
                )
                .SetEase(Ease.OutBack);
    }


    private void Hide()
    {
        fadeTween?.Kill();
        scaleTween?.Kill();

        fadeTween =
            canvasGroup
                .DOFade(0f, hideDuration)
                .SetEase(Ease.InQuad);

        scaleTween =
            keyCardIcon.transform
                .DOScale(
                    originalScale * startScale,
                    hideDuration
                )
                .SetEase(Ease.InQuad)
                .OnComplete(() =>
                {
                    keyCardIcon.SetActive(false);
                });
    }


    private void KillTweens()
    {
        fadeTween?.Kill();
        scaleTween?.Kill();

        fadeTween = null;
        scaleTween = null;
    }
}