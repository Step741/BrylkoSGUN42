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

    [Inject]
    private void Construct(IInputService inputService)
    {
        this.inputService = inputService;
    }

    private void Start()
    {
        if (inputService != null)
        {
            inputService.Interact.performed += OnInteract;
        }
    }

    private void Update()
    {
        CheckInteraction();
    }

    private void OnDestroy()
    {
        if (inputService != null)
        {
            inputService.Interact.performed -= OnInteract;
        }
    }

    private void CheckInteraction()
    {
        IInteractable detectedInteractable = null;
        InteractableHighlight detectedHighlight = null;

        if (playerCamera == null)
        {
            ClearInteraction();
            return;
        }

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            interactionDistance,
            interactionMask,
            QueryTriggerInteraction.Ignore))
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

            // Подсвечиваем только действительно интерактивные объекты.
            if (detectedInteractable == null)
            {
                detectedHighlight = null;
            }
        }

        // Перешли на другой объект или полностью ушли с объекта.
        if (detectedInteractable != currentInteractable)
        {
            ClearInteraction();

            currentInteractable = detectedInteractable;
            currentHighlight = detectedHighlight;

            if (currentHighlight != null)
            {
                currentHighlight.SetHighlighted(true);
            }
        }
        // Тот же интерактивный объект, но другой Collider.
        else if (currentInteractable != null &&
                 currentHighlight != detectedHighlight)
        {
            if (currentHighlight != null)
            {
                currentHighlight.SetHighlighted(false);
            }

            currentHighlight = detectedHighlight;

            if (currentHighlight != null)
            {
                currentHighlight.SetHighlighted(true);
            }
        }

        if (currentInteractable != null)
        {
            ShowPrompt(
                currentInteractable.GetInteractionText()
            );
        }
        else
        {
            HidePrompt();
        }
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (currentInteractable == null)
            return;

        currentInteractable.Interact();
    }

    private void ClearInteraction()
    {
        if (currentHighlight != null)
        {
            currentHighlight.SetHighlighted(false);
        }

        currentHighlight = null;
        currentInteractable = null;

        HidePrompt();
    }

    private void ShowPrompt(string text)
    {
        if (interactionPromptAnimator == null)
            return;

        interactionPromptAnimator.Show(text);
    }

    private void HidePrompt()
    {
        if (interactionPromptAnimator == null)
            return;

        interactionPromptAnimator.Hide();
    }
}