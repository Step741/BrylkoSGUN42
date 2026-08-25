using DG.Tweening;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Door Panels")]
    [SerializeField] private Transform leftPanel;
    [SerializeField] private Transform rightPanel;

    [Header("Movement")]
    [SerializeField] private float openDistance = 2.5f;
    [SerializeField] private float openDuration = 0.8f;

    [Header("Sound")]
    [SerializeField] private DoorSoundController doorSoundController;

    private Vector3 leftClosedPosition;
    private Vector3 rightClosedPosition;

    private Vector3 leftOpenPosition;
    private Vector3 rightOpenPosition;

    private bool isOpen;
    private int playersInside;

    private void Awake()
    {
        leftClosedPosition = leftPanel.localPosition;
        rightClosedPosition = rightPanel.localPosition;

        leftOpenPosition =
            leftClosedPosition +
            Vector3.left * openDistance;

        rightOpenPosition =
            rightClosedPosition +
            Vector3.right * openDistance;
    }

    public void PlayerEntered()
    {
        playersInside++;

        Open();
    }

    public void PlayerExited()
    {
        playersInside =
            Mathf.Max(
                0,
                playersInside - 1
            );

        if (playersInside == 0)
        {
            Close();
        }
    }

    private void Open()
    {
        if (isOpen)
            return;

        isOpen = true;

        KillTweens();

        Sequence sequence =
            DOTween.Sequence();

        sequence.Join(
            leftPanel.DOLocalMove(
                leftOpenPosition,
                openDuration
            )
        );

        sequence.Join(
            rightPanel.DOLocalMove(
                rightOpenPosition,
                openDuration
            )
        );

        sequence
            .SetEase(Ease.InOutQuad)
            .SetLink(gameObject);

        if (doorSoundController != null)
        {
            doorSoundController.PlayOpen();
        }
    }

    private void Close()
    {
        if (!isOpen)
            return;

        isOpen = false;

        KillTweens();

        Sequence sequence =
            DOTween.Sequence();

        sequence.Join(
            leftPanel.DOLocalMove(
                leftClosedPosition,
                openDuration
            )
        );

        sequence.Join(
            rightPanel.DOLocalMove(
                rightClosedPosition,
                openDuration
            )
        );

        sequence
            .SetEase(Ease.InOutQuad)
            .SetLink(gameObject);

        if (doorSoundController != null)
        {
            doorSoundController.PlayClose();
        }
    }

    private void KillTweens()
    {
        if (leftPanel != null)
        {
            leftPanel.DOKill();
        }

        if (rightPanel != null)
        {
            rightPanel.DOKill();
        }
    }

    private void OnDestroy()
    {
        KillTweens();
    }
}