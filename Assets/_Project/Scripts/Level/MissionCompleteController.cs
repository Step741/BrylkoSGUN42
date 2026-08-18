using DG.Tweening;
using TMPro;
using UnityEngine;

public class MissionCompleteController : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private ComputerInteractable computerInteractable;

    [SerializeField]
    private GameObject missionCompletePanel;

    [SerializeField]
    private CanvasGroup canvasGroup;

    [SerializeField]
    private TMP_Text statusText;

    [Header("Completion")]
    [SerializeField]
    private float loadingDuration = 5f;

    [Header("Animation")]
    [SerializeField]
    private float fadeDuration = 0.25f;

    [SerializeField]
    private float scaleDuration = 0.3f;

    [SerializeField]
    private float startScale = 0.9f;

    private Tween fadeTween;
    private Tween scaleTween;
    private Tween completionTween;

    private Vector3 originalScale;

    private bool isCompleting;
    private bool isCompleted;


    private void Awake()
    {
        if (missionCompletePanel == null)
        {
            Debug.LogError(
                "[MissionComplete] Mission Complete Panel is not assigned.",
                this
            );

            return;
        }

        originalScale =
            missionCompletePanel.transform.localScale;

        missionCompletePanel.SetActive(false);
    }


    private void Start()
    {
        if (computerInteractable == null)
        {
            Debug.LogError(
                "[MissionComplete] Computer Interactable is not assigned.",
                this
            );

            return;
        }

        computerInteractable.Activated += StartMissionComplete;

        Debug.Log(
            "[MissionComplete] Successfully subscribed to Computer."
        );
    }


    private void OnDestroy()
    {
        if (computerInteractable != null)
        {
            computerInteractable.Activated -= StartMissionComplete;
        }

        KillTweens();
    }


    // =========================================================
    // MISSION
    // =========================================================

    private void StartMissionComplete()
    {
        Debug.Log(
            "[MissionComplete] Computer activated. Starting sequence."
        );

        if (isCompleting || isCompleted)
            return;

        isCompleting = true;

        ShowPanel();

        completionTween =
            DOVirtual.DelayedCall(
                loadingDuration,
                CompleteMission
            )
            .SetLink(gameObject);
    }


    // =========================================================
    // UI
    // =========================================================

    private void ShowPanel()
    {
        missionCompletePanel.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;

            fadeTween?.Kill();

            fadeTween =
                canvasGroup
                    .DOFade(1f, fadeDuration)
                    .SetEase(Ease.OutQuad);
        }

        missionCompletePanel.transform.localScale =
            originalScale * startScale;

        scaleTween?.Kill();

        scaleTween =
            missionCompletePanel
                .transform
                .DOScale(
                    originalScale,
                    scaleDuration
                )
                .SetEase(Ease.OutBack);

        if (statusText != null)
        {
            statusText.text =
                "SYSTEM INITIALIZING...";
        }
    }


    // =========================================================
    // COMPLETE
    // =========================================================

    private void CompleteMission()
    {
        if (isCompleted)
            return;

        isCompleted = true;
        isCompleting = false;

        if (statusText != null)
        {
            statusText.text =
                "MISSION COMPLETE";
        }

        Debug.Log(
            "[MissionComplete] MISSION COMPLETE!"
        );
    }


    // =========================================================
    // CLEANUP
    // =========================================================

    private void KillTweens()
    {
        fadeTween?.Kill();
        scaleTween?.Kill();
        completionTween?.Kill();

        fadeTween = null;
        scaleTween = null;
        completionTween = null;
    }
}