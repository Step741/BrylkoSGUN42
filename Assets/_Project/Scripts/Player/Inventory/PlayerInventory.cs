using System;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    // =========================================================
    // EVENTS
    // =========================================================

    /// <summary>
    /// Вызывается, когда состояние ключ-карты изменилось.
    /// true  - карта получена.
    /// false - карта потрачена / удалена.
    /// </summary>
    public event Action<bool> KeyCardChanged;


    // =========================================================
    // STATE
    // =========================================================

    private bool hasKeyCard;


    // =========================================================
    // PUBLIC API
    // =========================================================

    public bool HasKeyCard =>
        hasKeyCard;


    /// <summary>
    /// Пытается добавить ключ-карту.
    /// Возвращает true, если карта действительно была добавлена.
    /// </summary>
    public bool TryAddKeyCard()
    {
        if (hasKeyCard)
            return false;

        hasKeyCard = true;

        KeyCardChanged?.Invoke(true);

        return true;
    }


    /// <summary>
    /// Пытается потратить ключ-карту.
    /// Возвращает true, если карта действительно была потрачена.
    /// </summary>
    public bool TryRemoveKeyCard()
    {
        if (!hasKeyCard)
            return false;

        hasKeyCard = false;

        KeyCardChanged?.Invoke(false);

        return true;
    }


    /// <summary>
    /// Полностью очищает инвентарь.
    /// Полезно при рестарте / смерти / сбросе уровня.
    /// </summary>
    public void Clear()
    {
        if (!hasKeyCard)
            return;

        hasKeyCard = false;

        KeyCardChanged?.Invoke(false);
    }
}