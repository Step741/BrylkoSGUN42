using UnityEngine;
using UnityEngine.UI;

public class FullscreenMapMarkerUI : MonoBehaviour
{
    [SerializeField]
    private Image iconImage;


    public RectTransform RectTransform
    {
        get
        {
            return transform as RectTransform;
        }
    }


    public void Setup(Sprite icon)
    {
        if (iconImage != null)
        {
            iconImage.sprite = icon;
        }
    }


    public void SetVisible(bool value)
    {
        gameObject.SetActive(value);
    }
}