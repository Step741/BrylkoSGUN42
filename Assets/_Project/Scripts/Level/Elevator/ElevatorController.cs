using System.Collections;
using DG.Tweening;
using UnityEngine;

public class ElevatorController : MonoBehaviour
{
    [Header("Platform")]

    [SerializeField]
    private Transform platform;


    [Header("Movement")]

    [SerializeField]
    private float height = 8f;

    [SerializeField]
    private float moveDuration = 3f;


    [Header("Return")]

    [SerializeField]
    private float returnDelay = 5f;


    [Header("Player")]

    [SerializeField]
    private Transform player;


    [Header("Sound")]

    [SerializeField]
    private ElevatorSoundController elevatorSoundController;


    private Vector3 bottomPosition;
    private Vector3 topPosition;

    private bool isMoving;
    private bool isAtTop;

    private Coroutine returnCoroutine;

    // Текущий tween движения лифта.
    private Tween movementTween;


    // ==========================================
    // UNITY
    // ==========================================

    private void Awake()
    {
        if (platform == null)
        {
            Debug.LogError(
                "[Elevator] Platform is not assigned.",
                this
            );

            return;
        }


        bottomPosition =
            platform.position;

        topPosition =
            bottomPosition +
            Vector3.up * height;
    }


    private void OnDisable()
    {
        CancelReturn();
        KillMovementTween();
    }


    private void OnDestroy()
    {
        CancelReturn();
        KillMovementTween();
    }


    // ==========================================
    // PLAYER
    // ==========================================

    public void SetPlayer(
        Transform target)
    {
        player = target;

        // Если игрок снова зашёл на платформу,
        // отменяем уже запланированный возврат.
        CancelReturn();
    }


    public void ClearPlayer(
        Transform target)
    {
        if (player == target)
        {
            player = null;
        }
    }


    // ==========================================
    // ACTIVATE
    // ==========================================

    public void Activate()
    {
        if (isMoving)
            return;

        if (isAtTop)
            return;

        MoveUp();
    }


    // ==========================================
    // MOVE UP
    // ==========================================

    private void MoveUp()
    {
        if (platform == null)
            return;


        isMoving = true;

        CancelReturn();
        KillMovementTween();


        // 🔊 SOUND UP

        if (elevatorSoundController != null)
        {
            elevatorSoundController.PlayMoveUp();
        }


        if (player != null)
        {
            player.SetParent(
                platform
            );
        }


        movementTween =
            platform
                .DOMove(
                    topPosition,
                    moveDuration
                )
                .SetEase(
                    Ease.InOutQuad
                )
                .SetLink(
                    gameObject
                )
                .OnKill(
                    () =>
                    {
                        movementTween = null;
                    }
                )
                .OnComplete(
                    () =>
                    {
                        movementTween = null;

                        if (this == null)
                            return;

                        isMoving = false;
                        isAtTop = true;

                        StartReturnTimer();
                    }
                );
    }


    // ==========================================
    // MOVE DOWN
    // ==========================================

    public void ReturnDown()
    {
        if (isMoving)
            return;

        if (!isAtTop)
            return;

        if (platform == null)
            return;


        isMoving = true;

        CancelReturn();
        KillMovementTween();


        // 🔊 SOUND DOWN

        if (elevatorSoundController != null)
        {
            elevatorSoundController.PlayMoveDown();
        }


        // Если игрок всё ещё на платформе,
        // оставляем его дочерним объектом.
        // Он поедет вниз вместе с лифтом.

        movementTween =
            platform
                .DOMove(
                    bottomPosition,
                    moveDuration
                )
                .SetEase(
                    Ease.InOutQuad
                )
                .SetLink(
                    gameObject
                )
                .OnKill(
                    () =>
                    {
                        movementTween = null;
                    }
                )
                .OnComplete(
                    () =>
                    {
                        movementTween = null;

                        if (this == null)
                            return;

                        isMoving = false;
                        isAtTop = false;

                        if (player != null)
                        {
                            player.SetParent(
                                null
                            );

                            player = null;
                        }
                    }
                );
    }


    // ==========================================
    // RETURN TIMER
    // ==========================================

    private void StartReturnTimer()
    {
        CancelReturn();

        returnCoroutine =
            StartCoroutine(
                ReturnAfterDelay()
            );
    }


    private IEnumerator ReturnAfterDelay()
    {
        yield return new WaitForSeconds(
            returnDelay
        );

        returnCoroutine = null;

        ReturnDown();
    }


    private void CancelReturn()
    {
        if (returnCoroutine == null)
            return;

        StopCoroutine(
            returnCoroutine
        );

        returnCoroutine = null;
    }


    // ==========================================
    // TWEEN CLEANUP
    // ==========================================

    private void KillMovementTween()
    {
        if (
            movementTween != null &&
            movementTween.IsActive()
        )
        {
            movementTween.Kill();
        }

        movementTween = null;


        if (platform != null)
        {
            platform.DOKill();
        }
    }


    // ==========================================
    // STATE
    // ==========================================

    public bool IsMoving()
    {
        return isMoving;
    }


    public bool IsAtTop()
    {
        return isAtTop;
    }
}