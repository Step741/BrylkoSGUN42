using System.Collections;
using DG.Tweening;
using UnityEngine;

public class ElevatorController : MonoBehaviour
{
    [Header("Platform")]
    [SerializeField] private Transform platform;

    [Header("Movement")]
    [SerializeField] private float height = 8f;
    [SerializeField] private float moveDuration = 3f;

    [Header("Return")]
    [SerializeField] private float returnDelay = 5f;

    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip moveSound;

    private Vector3 bottomPosition;
    private Vector3 topPosition;

    private bool isMoving;
    private bool isAtTop;

    private Coroutine returnCoroutine;

    private void Awake()
    {
        if (platform == null)
        {
            Debug.LogError("[Elevator] Platform is not assigned.", this);
            return;
        }

        bottomPosition = platform.position;
        topPosition = bottomPosition + Vector3.up * height;
    }

    public void SetPlayer(Transform target)
    {
        player = target;

        // Если игрок снова зашёл на платформу,
        // отменяем уже запланированный возврат.
        CancelReturn();
    }

    public void ClearPlayer(Transform target)
    {
        if (player == target)
            player = null;
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
        isMoving = true;

        CancelReturn();

        platform.DOKill();

        if (audioSource != null && moveSound != null)
        {
            audioSource.PlayOneShot(moveSound);
        }

        if (player != null)
        {
            player.SetParent(platform);
        }

        platform
            .DOMove(topPosition, moveDuration)
            .SetEase(Ease.InOutQuad)
            .SetLink(gameObject)
            .OnComplete(() =>
            {
                isMoving = false;
                isAtTop = true;

                StartReturnTimer();
            });
    }

    public void ReturnDown()
    {
        if (isMoving)
            return;

        if (!isAtTop)
            return;

        isMoving = true;

        CancelReturn();

        platform.DOKill();

        // Если игрок всё ещё на платформе,
        // оставляем его дочерним объектом.
        // Он поедет вниз вместе с лифтом.

        platform
            .DOMove(bottomPosition, moveDuration)
            .SetEase(Ease.InOutQuad)
            .SetLink(gameObject)
            .OnComplete(() =>
            {
                isMoving = false;
                isAtTop = false;

                if (player != null)
                {
                    player.SetParent(null);
                    player = null;
                }
            });
    }

    private void StartReturnTimer()
    {
        CancelReturn();

        returnCoroutine = StartCoroutine(ReturnAfterDelay());
    }

    private IEnumerator ReturnAfterDelay()
    {
        yield return new WaitForSeconds(returnDelay);

        returnCoroutine = null;

        ReturnDown();
    }

    private void CancelReturn()
    {
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
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