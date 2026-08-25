using System;
using DG.Tweening;
using UnityEngine;

public class PickupAnimator : MonoBehaviour
{
    [Header("Idle Animation")]
    [SerializeField]
    private float rotationDuration = 3f;

    [SerializeField]
    private float floatHeight = 0.15f;

    [SerializeField]
    private float floatDuration = 1.2f;

    [Header("Pickup Animation")]
    [SerializeField]
    private float scaleDuration = 0.2f;

    private Tween floatTween;
    private Tween rotationTween;
    private Tween pickupTween;

    private Vector3 startPosition;
    private bool isPickedUp;


    private void Awake()
    {
        startPosition =
            transform.localPosition;
    }


    private void Start()
    {
        CreateIdleAnimation();
    }


    private void CreateIdleAnimation()
    {
        // На всякий случай убиваем старые твины
        // этого Transform.
        DOTween.Kill(transform);

        // =========================
        // ПЛАВНОЕ ПОКАЧИВАНИЕ
        // =========================

        floatTween =
            transform.DOLocalMoveY(
                startPosition.y + floatHeight,
                floatDuration
            )
            .SetEase(Ease.InOutSine)
            .SetLoops(
                -1,
                LoopType.Yoyo
            )
            .SetLink(gameObject);


        // =========================
        // ПОСТОЯННОЕ ВРАЩЕНИЕ
        // =========================

        rotationTween =
            transform.DOLocalRotate(
                new Vector3(
                    0f,
                    360f,
                    0f
                ),
                rotationDuration,
                RotateMode.FastBeyond360
            )
            .SetEase(Ease.Linear)
            .SetLoops(
                -1,
                LoopType.Restart
            )
            .SetLink(gameObject);
    }


    public void PlayPickupAnimation(
        Transform target,
        Action onComplete)
    {
        if (isPickedUp)
            return;

        if (target == null)
            return;

        isPickedUp = true;


        // Останавливаем idle-анимацию.
        floatTween?.Kill();
        rotationTween?.Kill();

        DOTween.Kill(transform);


        // =========================
        // ПОДБОР
        // =========================
        //
        // Никакого движения к игроку.
        // Только плавное уменьшение.
        //

        pickupTween =
            transform.DOScale(
                Vector3.zero,
                scaleDuration
            )
            .SetEase(Ease.InBack)
            .SetLink(gameObject);


        pickupTween.OnComplete(() =>
        {
            onComplete?.Invoke();
        });
    }


    private void OnDestroy()
    {
        floatTween?.Kill();
        rotationTween?.Kill();
        pickupTween?.Kill();
    }
}