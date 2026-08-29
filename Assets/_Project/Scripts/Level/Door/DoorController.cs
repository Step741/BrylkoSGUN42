using DG.Tweening;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Door Panels")]

    [SerializeField]
    private Transform leftPanel;

    [SerializeField]
    private Transform rightPanel;


    [Header("Movement")]

    [SerializeField]
    private float openDistance = 2.5f;

    [SerializeField]
    private float openDuration = 0.8f;


    [Header("Sound")]

    [SerializeField]
    private DoorSoundController doorSoundController;


    private Vector3 leftClosedPosition;
    private Vector3 rightClosedPosition;

    private Vector3 leftOpenPosition;
    private Vector3 rightOpenPosition;


    private bool isOpen;
    private int playersInside;


    //Анимация открытия или закрытия двери
    private Sequence doorSequence;

    private void Awake()
    {
        if (leftPanel != null)
        {
            leftClosedPosition =
                leftPanel.localPosition;

            leftOpenPosition =
                leftClosedPosition +
                Vector3.left * openDistance;
        }


        if (rightPanel != null)
        {
            rightClosedPosition =
                rightPanel.localPosition;

            rightOpenPosition =
                rightClosedPosition +
                Vector3.right * openDistance;
        }
    }


    private void OnDisable()
    {
        KillTweens();
    }


    private void OnDestroy()
    {
        KillTweens();
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


        doorSequence =
            DOTween.Sequence()
                .SetLink(
                    gameObject
                );


        if (leftPanel != null)
        {
            doorSequence.Join(
                leftPanel.DOLocalMove(
                    leftOpenPosition,
                    openDuration
                )
            );
        }


        if (rightPanel != null)
        {
            doorSequence.Join(
                rightPanel.DOLocalMove(
                    rightOpenPosition,
                    openDuration
                )
            );
        }


        doorSequence
            .SetEase(
                Ease.InOutQuad
            )
            .OnKill(
                () =>
                {
                    doorSequence = null;
                }
            )
            .OnComplete(
                () =>
                {
                    doorSequence = null;
                }
            );


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


        doorSequence =
            DOTween.Sequence()
                .SetLink(
                    gameObject
                );


        if (leftPanel != null)
        {
            doorSequence.Join(
                leftPanel.DOLocalMove(
                    leftClosedPosition,
                    openDuration
                )
            );
        }


        if (rightPanel != null)
        {
            doorSequence.Join(
                rightPanel.DOLocalMove(
                    rightClosedPosition,
                    openDuration
                )
            );
        }


        doorSequence
            .SetEase(
                Ease.InOutQuad
            )
            .OnKill(
                () =>
                {
                    doorSequence = null;
                }
            )
            .OnComplete(
                () =>
                {
                    doorSequence = null;
                }
            );


        if (doorSoundController != null)
        {
            doorSoundController.PlayClose();
        }
    }

    //CLEANUP
    private void KillTweens()
    {
        if (
            doorSequence != null &&
            doorSequence.IsActive()
        )
        {
            doorSequence.Kill();
        }

        doorSequence = null;


        if (leftPanel != null)
        {
            leftPanel.DOKill();
        }


        if (rightPanel != null)
        {
            rightPanel.DOKill();
        }
    }
}