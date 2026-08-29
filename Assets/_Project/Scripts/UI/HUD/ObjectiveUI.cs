using TMPro;
using UnityEngine;
using DG.Tweening;

public class ObjectiveUI : MonoBehaviour
{
    [Header("References")]

    [SerializeField]
    private KeyCardPickup keyCardPickup;

    [SerializeField]
    private ComputerInteractable computerInteractable;


    [Header("UI")]

    [SerializeField]
    private TMP_Text objectiveText;


    [Header("Texts")]

    [TextArea]
    [SerializeField]
    private string findKeyCardText =
        "Mission\n\n- Find the key card";

    [TextArea]
    [SerializeField]
    private string activateComputerText =
        "Mission\n\nKey card received\n\n- Activate the PC";

    [TextArea]
    [SerializeField]
    private string completedText =
        "Mission\n\nKey card received\n\nThe system is activated";


    [Header("DOTween Animation")]

    [SerializeField]
    private float fadeOutDuration = 0.15f;

    [SerializeField]
    private float fadeInDuration = 0.25f;

    [SerializeField]
    private float slideOffset = 15f;


    //Исходная позиция текста
    private Vector3 originalLocalPosition;

    //Текущая последовательность анимации текста
    private Sequence textSequence;

    private void Awake()
    {
        if (objectiveText != null)
        {
            originalLocalPosition =
                objectiveText.transform.localPosition;

            SetTextInstant(
                findKeyCardText
            );
        }
    }


    private void OnEnable()
    {
        if (keyCardPickup != null)
        {
            keyCardPickup.PickedUp +=
                OnKeyCardPickedUp;
        }


        if (computerInteractable != null)
        {
            computerInteractable.Activated +=
                OnComputerActivated;
        }
    }


    private void OnDisable()
    {
        if (keyCardPickup != null)
        {
            keyCardPickup.PickedUp -=
                OnKeyCardPickedUp;
        }


        if (computerInteractable != null)
        {
            computerInteractable.Activated -=
                OnComputerActivated;
        }


        KillTextTween();

        ResetTextState();
    }


    private void OnDestroy()
    {
        KillTextTween();
    }

    private void OnKeyCardPickedUp()
    {
        ShowActivateComputer();
    }


    private void OnComputerActivated()
    {
        ShowCompleted();
    }

    private void ShowFindKeyCard()
    {
        SetText(
            findKeyCardText
        );
    }


    private void ShowActivateComputer()
    {
        SetText(
            activateComputerText
        );
    }


    private void ShowCompleted()
    {
        SetText(
            completedText
        );
    }

    private void SetText(
        string newText
    )
    {
        if (objectiveText == null)
        {
            return;
        }

        KillTextTween();


        textSequence =
            DOTween.Sequence()
                .SetLink(
                    gameObject
                );

        textSequence.Append(
            objectiveText
                .DOFade(
                    0f,
                    fadeOutDuration
                )
                .SetEase(
                    Ease.OutQuad
                )
        );

        textSequence.AppendCallback(
            () =>
            {
                if (objectiveText == null)
                    return;


                objectiveText.text =
                    newText;

                objectiveText.transform.localPosition =
                    originalLocalPosition -
                    new Vector3(
                        0f,
                        slideOffset,
                        0f
                    );
            }
        );

        textSequence.Append(
            objectiveText
                .DOFade(
                    1f,
                    fadeInDuration
                )
                .SetEase(
                    Ease.OutQuad
                )
        );


        textSequence.Join(
            objectiveText.transform
                .DOLocalMove(
                    originalLocalPosition,
                    fadeInDuration
                )
                .SetEase(
                    Ease.OutQuad
                )
        );


        textSequence.OnKill(
            () =>
            {
                textSequence = null;
            }
        );


        textSequence.OnComplete(
            () =>
            {
                textSequence = null;
            }
        );
    }

    private void SetTextInstant(
        string text
    )
    {
        if (objectiveText == null)
            return;


        KillTextTween();


        objectiveText.text =
            text;


        objectiveText.alpha =
            1f;


        objectiveText.transform.localPosition =
            originalLocalPosition;
    }

    private void ResetTextState()
    {
        if (objectiveText == null)
            return;


        objectiveText.alpha =
            1f;


        objectiveText.transform.localPosition =
            originalLocalPosition;
    }

    //CLEANUP
    private void KillTextTween()
    {
        if (
            textSequence != null &&
            textSequence.IsActive()
        )
        {
            textSequence.Kill();
        }

        textSequence = null;


        if (objectiveText == null)
            return;


        objectiveText.DOKill();

        objectiveText.transform.DOKill();
    }
}