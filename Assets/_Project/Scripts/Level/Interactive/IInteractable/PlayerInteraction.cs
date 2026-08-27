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

    // Последний текст, который был передан в prompt.
    // Нужен, чтобы не вызывать Show() каждый кадр.
    private string currentPromptText;

    // Отслеживаем, подписались ли мы на InputAction.
    private bool isSubscribed;


    // =========================================================
    // ZENJECT
    // =========================================================

    [Inject]
    private void Construct(
        IInputService inputService
    )
    {
        this.inputService =
            inputService;
    }


    // =========================================================
    // UNITY
    // =========================================================

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


    // =========================================================
    // INPUT
    // =========================================================

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


    // =========================================================
    // INTERACTION CHECK
    // =========================================================

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


            // Подсвечиваем только действительно
            // интерактивные объекты.
            if (detectedInteractable == null)
            {
                detectedHighlight = null;
            }
        }


        // =====================================================
        // ПЕРЕШЛИ НА ДРУГОЙ ОБЪЕКТ
        // =====================================================

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


            // Новый объект — сбрасываем сохранённый текст,
            // чтобы новый prompt гарантированно показался.
            currentPromptText =
                null;
        }


        // =====================================================
        // ТОТ ЖЕ ОБЪЕКТ, НО ДРУГОЙ COLLIDER
        // =====================================================

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


        // =====================================================
        // PROMPT
        // =====================================================

        if (currentInteractable != null)
        {
            string newPromptText =
                currentInteractable
                    .GetInteractionText();


            // Вызываем Show только если текст изменился.
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
            // Если интерактивного объекта нет,
            // скрываем prompt только один раз.
            if (currentPromptText != null)
            {
                currentPromptText =
                    null;

                HidePrompt();
            }
        }
    }


    // =========================================================
    // INPUT CALLBACK
    // =========================================================

    private void OnInteract(
        InputAction.CallbackContext context
    )
    {
        if (currentInteractable == null)
            return;


        currentInteractable.Interact();
    }


    // =========================================================
    // CLEAR
    // =========================================================

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


    // =========================================================
    // PROMPT
    // =========================================================

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