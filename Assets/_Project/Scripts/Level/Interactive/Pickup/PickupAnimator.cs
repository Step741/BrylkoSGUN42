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


    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        startPosition =
            transform.localPosition;
    }


    private void Start()
    {
        CreateIdleAnimation();
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
    // IDLE ANIMATION
    // =========================================================

    private void CreateIdleAnimation()
    {
        if (isPickedUp)
            return;


        KillTweens();


        // =========================
        // ПЛАВНОЕ ПОКАЧИВАНИЕ
        // =========================

        floatTween =
            transform
                .DOLocalMoveY(
                    startPosition.y + floatHeight,
                    floatDuration
                )
                .SetEase(
                    Ease.InOutSine
                )
                .SetLoops(
                    -1,
                    LoopType.Yoyo
                )
                .SetLink(
                    gameObject
                )
                .OnKill(
                    () =>
                    {
                        floatTween = null;
                    }
                );


        // =========================
        // ПОСТОЯННОЕ ВРАЩЕНИЕ
        // =========================

        rotationTween =
            transform
                .DOLocalRotate(
                    new Vector3(
                        0f,
                        360f,
                        0f
                    ),
                    rotationDuration,
                    RotateMode.FastBeyond360
                )
                .SetEase(
                    Ease.Linear
                )
                .SetLoops(
                    -1,
                    LoopType.Restart
                )
                .SetLink(
                    gameObject
                )
                .OnKill(
                    () =>
                    {
                        rotationTween = null;
                    }
                );
    }


    // =========================================================
    // PICKUP ANIMATION
    // =========================================================

    public void PlayPickupAnimation(
        Transform target,
        Action onComplete
    )
    {
        if (isPickedUp)
            return;

        if (target == null)
            return;


        isPickedUp = true;


        // Останавливаем idle-анимации.
        KillIdleTweens();


        // =========================
        // ПОДБОР
        // =========================
        //
        // Никакого движения к игроку.
        // Только плавное уменьшение.
        //

        pickupTween =
            transform
                .DOScale(
                    Vector3.zero,
                    scaleDuration
                )
                .SetEase(
                    Ease.InBack
                )
                .SetLink(
                    gameObject
                )
                .OnKill(
                    () =>
                    {
                        pickupTween = null;
                    }
                )
                .OnComplete(
                    () =>
                    {
                        pickupTween = null;

                        if (this == null)
                            return;

                        onComplete?.Invoke();
                    }
                );
    }


    // =========================================================
    // CLEANUP
    // =========================================================

    private void KillTweens()
    {
        KillIdleTweens();
        KillPickupTween();
    }


    private void KillIdleTweens()
    {
        if (
            floatTween != null &&
            floatTween.IsActive()
        )
        {
            floatTween.Kill();
        }

        floatTween = null;


        if (
            rotationTween != null &&
            rotationTween.IsActive()
        )
        {
            rotationTween.Kill();
        }

        rotationTween = null;
    }


    private void KillPickupTween()
    {
        if (
            pickupTween != null &&
            pickupTween.IsActive()
        )
        {
            pickupTween.Kill();
        }

        pickupTween = null;
    }
}