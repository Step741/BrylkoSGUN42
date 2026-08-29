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

    //Текущий tween движения лифта
    private Tween movementTween;

    private void Awake()
    {
        if (platform == null)
        {
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

    public void SetPlayer(
        Transform target)
    {
        player = target;

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

    public void Activate()
    {
        if (isMoving)
            return;

        if (isAtTop)
            return;

        MoveUp();
    }

    private void MoveUp()
    {
        if (platform == null)
            return;


        isMoving = true;

        CancelReturn();
        KillMovementTween();

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

        if (elevatorSoundController != null)
        {
            elevatorSoundController.PlayMoveDown();
        }

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

    //CLEANUP
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

    public bool IsMoving()
    {
        return isMoving;
    }


    public bool IsAtTop()
    {
        return isAtTop;
    }
}