using System;
using UnityEngine;

public class ComputerInteractable : MonoBehaviour, IInteractable
{
    [Header("Interaction")]
    [SerializeField]
    private string interactionText = "PRESS - E";

    [SerializeField]
    private string noKeyCardText = "NEED KEY CARD";


    [Header("Player")]
    [SerializeField]
    private PlayerInventory playerInventory;


    private bool isActivated;


    public event Action Activated;

    public void Interact()
    {
        if (isActivated)
            return;


        if (playerInventory == null)
        {
            return;
        }


        //Нет ключ-карты
        if (!playerInventory.HasKeyCard)
        {
            return;
        }


        //Забрать карту из инвентаря
        if (!playerInventory.TryRemoveKeyCard())
            return;


        Activate();
    }

    private void Activate()
    {
        if (isActivated)
            return;


        isActivated = true;

        Activated?.Invoke();
    }

    public string GetInteractionText()
    {
        if (isActivated)
            return string.Empty;


        if (playerInventory == null)
            return noKeyCardText;


        if (!playerInventory.HasKeyCard)
            return noKeyCardText;


        return interactionText;
    }

    public bool IsActivated =>
        isActivated;
}