using System;
using UnityEngine;

public class ComputerInteractable : MonoBehaviour, IInteractable
{
    [Header("Interaction")]
    [SerializeField]
    private string interactionText = "PRESS - E";

    [SerializeField]
    private string noKeyCardText = "NEED KEY CARD";

    [SerializeField]
    private string activatedText = "ACCESS GRANTED";

    [Header("Player")]
    [SerializeField]
    private PlayerInventory playerInventory;

    private bool isActivated;

    public event Action Activated;


    // =========================================================
    // INTERACTION
    // =========================================================

    public void Interact()
    {
        if (isActivated)
            return;

        if (playerInventory == null)
        {
            Debug.LogWarning(
                $"[{name}] PlayerInventory is not assigned.",
                this
            );

            return;
        }

        // Нет ключ-карты.
        if (!playerInventory.HasKeyCard)
        {
            Debug.Log(
                $"[{name}] Access denied. Key card required."
            );

            return;
        }

        // Пытаемся забрать карту из инвентаря.
        if (!playerInventory.TryRemoveKeyCard())
            return;

        Activate();
    }


    // =========================================================
    // ACTIVATION
    // =========================================================

    private void Activate()
    {
        if (isActivated)
            return;

        isActivated = true;

        Debug.Log(
            $"[{name}] Computer activated."
        );

        Activated?.Invoke();
    }


    // =========================================================
    // INTERACTION TEXT
    // =========================================================

    public string GetInteractionText()
    {
        if (isActivated)
            return activatedText;

        if (playerInventory == null)
            return noKeyCardText;

        if (!playerInventory.HasKeyCard)
            return noKeyCardText;

        return interactionText;
    }


    // =========================================================
    // STATE
    // =========================================================

    public bool IsActivated =>
        isActivated;
}