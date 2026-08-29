using System;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public event Action<bool> KeyCardChanged;

    private bool hasKeyCard;

    public bool HasKeyCard =>
        hasKeyCard;

    public bool TryAddKeyCard()
    {
        if (hasKeyCard)
            return false;

        hasKeyCard = true;

        KeyCardChanged?.Invoke(true);

        return true;
    }

    public bool TryRemoveKeyCard()
    {
        if (!hasKeyCard)
            return false;

        hasKeyCard = false;

        KeyCardChanged?.Invoke(false);

        return true;
    }

    public void Clear()
    {
        if (!hasKeyCard)
            return;

        hasKeyCard = false;

        KeyCardChanged?.Invoke(false);
    }
}