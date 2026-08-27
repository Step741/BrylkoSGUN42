using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

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


    public void Setup(
        Sprite icon)
    {
        if (iconImage != null)
        {
            iconImage.sprite =
                icon;
        }
    }


    public void SetVisible(
        bool value)
    {
        if (gameObject == null)
            return;


        gameObject.SetActive(
            value
        );
    }


    private void OnDestroy()
    {
        RectTransform rectTransform =
            RectTransform;


        if (rectTransform != null)
        {
            rectTransform.DOKill();
        }
    }
}