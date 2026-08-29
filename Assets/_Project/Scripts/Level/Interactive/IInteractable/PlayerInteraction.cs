using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class PlayerInteraction : MonoBehaviour
{
    [Header("References")]

    [SerializeField]
    private Camera playerCamera;


    [Header("Raycast")]

    [SerializeField]
    private float interactionDistance = 3f;

    [SerializeField]
    private LayerMask interactionMask;


    [Header("UI")]

    [SerializeField]
    private InteractionPromptAnimator interactionPromptAnimator;


    private IInputService inputService;

    private IInteractable currentInteractable;
    private InteractableHighlight currentHighlight;

    private string currentPromptText;

    private bool isSubscribed;


    [Inject]
    private void Construct(
        IInputService inputService
    )
    {
        this.inputService =
            inputService;
    }

    private void OnEnable()
    {
        SubscribeInput();
    }


    private void Update()
    {
        CheckInteraction();
    }


    private void OnDisable()
    {
        UnsubscribeInput();

        ClearInteraction();
    }


    private void OnDestroy()
    {
        UnsubscribeInput();
    }

    private void SubscribeInput()
    {
        if (
            inputService == null ||
            isSubscribed
        )
        {
            return;
        }


        inputService.Interact.performed +=
            OnInteract;

        isSubscribed = true;
    }


    private void UnsubscribeInput()
    {
        if (
            inputService == null ||
            !isSubscribed
        )
        {
            return;
        }


        inputService.Interact.performed -=
            OnInteract;

        isSubscribed = false;
    }

    private void CheckInteraction()
    {
        IInteractable detectedInteractable =
            null;

        InteractableHighlight detectedHighlight =
            null;


        if (playerCamera == null)
        {
            ClearInteraction();
            return;
        }


        Ray ray =
            new Ray(
                playerCamera.transform.position,
                playerCamera.transform.forward
            );


        if (
            Physics.Raycast(
                ray,
                out RaycastHit hit,
                interactionDistance,
                interactionMask,
                QueryTriggerInteraction.Ignore
            )
        )
        {
            detectedInteractable =
                hit.collider.GetComponent<IInteractable>();


            if (detectedInteractable == null)
            {
                detectedInteractable =
                    hit.collider.GetComponentInParent<IInteractable>();
            }


            detectedHighlight =
                hit.collider.GetComponent<InteractableHighlight>();


            if (detectedHighlight == null)
            {
                detectedHighlight =
                    hit.collider.GetComponentInParent<InteractableHighlight>();
            }


            //Подсвечиваем только действительно интерактивные объекты
            if (detectedInteractable == null)
            {
                detectedHighlight = null;
            }
        }

        if (
            detectedInteractable !=
            currentInteractable
        )
        {
            ClearInteraction();


            currentInteractable =
                detectedInteractable;

            currentHighlight =
                detectedHighlight;


            if (currentHighlight != null)
            {
                currentHighlight.SetHighlighted(
                    true
                );
            }

            currentPromptText =
                null;
        }

        else if (
            currentInteractable != null &&
            currentHighlight !=
            detectedHighlight
        )
        {
            if (currentHighlight != null)
            {
                currentHighlight.SetHighlighted(
                    false
                );
            }


            currentHighlight =
                detectedHighlight;


            if (currentHighlight != null)
            {
                currentHighlight.SetHighlighted(
                    true
                );
            }
        }

        if (currentInteractable != null)
        {
            string newPromptText =
                currentInteractable
                    .GetInteractionText();

            if (
                currentPromptText !=
                newPromptText
            )
            {
                currentPromptText =
                    newPromptText;

                ShowPrompt(
                    newPromptText
                );
            }
        }
        else
        {
            if (currentPromptText != null)
            {
                currentPromptText =
                    null;

                HidePrompt();
            }
        }
    }

    private void OnInteract(
        InputAction.CallbackContext context
    )
    {
        if (currentInteractable == null)
            return;


        currentInteractable.Interact();
    }

    //CLEAR
    private void ClearInteraction()
    {
        if (currentHighlight != null)
        {
            currentHighlight.SetHighlighted(
                false
            );
        }


        currentHighlight =
            null;

        currentInteractable =
            null;


        if (currentPromptText != null)
        {
            currentPromptText =
                null;

            HidePrompt();
        }
    }

    private void ShowPrompt(
        string text
    )
    {
        if (interactionPromptAnimator == null)
            return;


        interactionPromptAnimator.Show(
            text
        );
    }


    private void HidePrompt()
    {
        if (interactionPromptAnimator == null)
            return;


        interactionPromptAnimator.Hide();
    }
}