using UnityEngine;
using DG.Tweening;

public class DynamicCrosshair : MonoBehaviour
{
    [Header("Crosshair Parts")]

    [SerializeField]
    private RectTransform top;

    [SerializeField]
    private RectTransform bottom;

    [SerializeField]
    private RectTransform left;

    [SerializeField]
    private RectTransform right;


    [Header("Player")]

    [SerializeField]
    private PlayerController playerController;

    [SerializeField]
    private ADSController adsController;


    [Header("Movement Spread")]

    [SerializeField]
    private float idleSpread = 15f;

    [SerializeField]
    private float moveSpread = 20f;

    [SerializeField]
    private float sprintSpread = 25f;

    [SerializeField]
    private float crouchSpread = 10f;

    [SerializeField]
    private float airSpread = 35f;

    [SerializeField]
    private float adsSpread = 6f;


    [Header("Fire Spread")]

    [SerializeField]
    private float fireSpreadPerShot = 4f;

    [SerializeField]
    private float maxFireSpread = 20f;

    [SerializeField]
    private float fireSpreadRecoverySpeed = 15f;


    [Header("Spread Animation")]

    [SerializeField]
    private float expandSpeed = 80f;

    [SerializeField]
    private float contractSpeed = 50f;


    [Header("DOTween - Fire Punch")]

    [SerializeField]
    private float firePunchScale = 0.08f;

    [SerializeField]
    private float firePunchDuration = 0.12f;


    [Header("DOTween - ADS Transition")]

    [SerializeField]
    private float adsVisualScale = 0.9f;

    [SerializeField]
    private float adsTransitionDuration = 0.12f;


    private float currentBaseSpread;
    private float fireSpread;

    private bool wasAiming;

    private Vector2 topBasePosition;
    private Vector2 bottomBasePosition;
    private Vector2 leftBasePosition;
    private Vector2 rightBasePosition;

    private void Awake()
    {
        SaveInitialPositions();

        currentBaseSpread = idleSpread;

        if (adsController != null)
        {
            wasAiming = adsController.IsAiming;
        }

        SetPartsScale(
            wasAiming
                ? adsVisualScale
                : 1f
        );

        UpdateCrosshair(
            currentBaseSpread
        );
    }


    private void Update()
    {
        if (playerController == null)
            return;


        CheckADSState();

        UpdateBaseSpread();
        UpdateFireSpread();


        float finalSpread =
            currentBaseSpread +
            fireSpread;


        UpdateCrosshair(
            finalSpread
        );
    }


    private void OnDisable()
    {
        KillCrosshairTweens();

        SetPartsScale(1f);
    }


    private void OnDestroy()
    {
        KillCrosshairTweens();
    }

    private void SaveInitialPositions()
    {
        if (top != null)
        {
            topBasePosition =
                top.anchoredPosition;
        }


        if (bottom != null)
        {
            bottomBasePosition =
                bottom.anchoredPosition;
        }


        if (left != null)
        {
            leftBasePosition =
                left.anchoredPosition;
        }


        if (right != null)
        {
            rightBasePosition =
                right.anchoredPosition;
        }
    }

    private void CheckADSState()
    {
        if (adsController == null)
            return;


        bool isAiming =
            adsController.IsAiming;


        if (isAiming == wasAiming)
            return;


        wasAiming = isAiming;

        PlayADSTransition(
            isAiming
        );
    }


    private void PlayADSTransition(
        bool isAiming
    )
    {
        float targetScale =
            isAiming
                ? adsVisualScale
                : 1f;


        AnimatePartScale(
            top,
            targetScale
        );

        AnimatePartScale(
            bottom,
            targetScale
        );

        AnimatePartScale(
            left,
            targetScale
        );

        AnimatePartScale(
            right,
            targetScale
        );
    }


    private void AnimatePartScale(
        RectTransform part,
        float targetScale
    )
    {
        if (part == null)
            return;


        part.DOKill();


        part
            .DOScale(
                Vector3.one * targetScale,
                adsTransitionDuration
            )
            .SetEase(
                Ease.OutQuad
            )
            .SetLink(
                part.gameObject
            );
    }

    private void UpdateBaseSpread()
    {
        float targetBaseSpread =
            GetBaseSpread();


        float speed;


        if (targetBaseSpread > currentBaseSpread)
        {
            speed = expandSpeed;
        }
        else
        {
            speed = contractSpeed;
        }


        currentBaseSpread =
            Mathf.MoveTowards(
                currentBaseSpread,
                targetBaseSpread,
                speed * Time.deltaTime
            );
    }


    private float GetBaseSpread()
    {
        if (!playerController.IsGrounded)
        {
            return airSpread;
        }

        if (
            adsController != null &&
            adsController.IsAiming
        )
        {
            return adsSpread;
        }

        if (playerController.IsCrouching)
        {
            return crouchSpread;
        }

        if (playerController.IsSprinting)
        {
            return sprintSpread;
        }

        if (playerController.IsMoving)
        {
            return moveSpread;
        }

        return idleSpread;
    }

    private void UpdateFireSpread()
    {
        fireSpread =
            Mathf.MoveTowards(
                fireSpread,
                0f,
                fireSpreadRecoverySpeed *
                Time.deltaTime
            );
    }


    public void AddFireSpread()
    {
        fireSpread +=
            fireSpreadPerShot;


        fireSpread =
            Mathf.Clamp(
                fireSpread,
                0f,
                maxFireSpread
            );


        PlayFirePunch();
    }


    private void PlayFirePunch()
    {
        PunchPart(
            top
        );

        PunchPart(
            bottom
        );

        PunchPart(
            left
        );

        PunchPart(
            right
        );
    }


    private void PunchPart(
        RectTransform part
    )
    {
        if (part == null)
            return;


        part.DOKill();


        part
            .DOPunchScale(
                Vector3.one *
                firePunchScale,
                firePunchDuration,
                1,
                0.5f
            )
            .SetLink(
                part.gameObject
            );
    }

    private void UpdateCrosshair(
        float spread
    )
    {
        float spreadOffset =
            spread -
            idleSpread;


        if (top != null)
        {
            top.anchoredPosition =
                topBasePosition +
                Vector2.up *
                spreadOffset;
        }


        if (bottom != null)
        {
            bottom.anchoredPosition =
                bottomBasePosition +
                Vector2.down *
                spreadOffset;
        }


        if (left != null)
        {
            left.anchoredPosition =
                leftBasePosition +
                Vector2.left *
                spreadOffset;
        }


        if (right != null)
        {
            right.anchoredPosition =
                rightBasePosition +
                Vector2.right *
                spreadOffset;
        }
    }

    private void SetPartsScale(
        float scale
    )
    {
        Vector3 targetScale =
            Vector3.one *
            scale;


        if (top != null)
            top.localScale = targetScale;

        if (bottom != null)
            bottom.localScale = targetScale;

        if (left != null)
            left.localScale = targetScale;

        if (right != null)
            right.localScale = targetScale;
    }


    private void KillCrosshairTweens()
    {
        if (top != null)
            top.DOKill();

        if (bottom != null)
            bottom.DOKill();

        if (left != null)
            left.DOKill();

        if (right != null)
            right.DOKill();
    }
}