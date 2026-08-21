using UnityEngine;
using UnityEngine.UI;

public class MinimapMarkerUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Image iconImage;

    [SerializeField]
    private Image edgeArrowImage;


    private void Awake()
    {
        if (iconImage == null)
        {
            iconImage = GetComponent<Image>();
        }
    }


    public void Setup(Sprite icon)
    {
        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
        }

        if (edgeArrowImage != null)
        {
            edgeArrowImage.gameObject.SetActive(false);
        }
    }


    public void ShowInside()
    {
        if (iconImage != null)
        {
            iconImage.gameObject.SetActive(true);
        }

        if (edgeArrowImage != null)
        {
            edgeArrowImage.gameObject.SetActive(false);
        }
    }


    public void ShowEdgeArrow(Vector2 direction)
    {
        if (iconImage != null)
        {
            iconImage.gameObject.SetActive(false);
        }

        if (edgeArrowImage == null)
            return;


        edgeArrowImage.gameObject.SetActive(true);


        float angle =
            Mathf.Atan2(
                direction.y,
                direction.x
            ) * Mathf.Rad2Deg;


        // Стандартная UI-стрелка должна смотреть вверх.
        edgeArrowImage.rectTransform.localRotation =
            Quaternion.Euler(
                0f,
                0f,
                angle - 90f
            );
    }


    public void ResetMarker()
    {
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.gameObject.SetActive(true);
        }

        if (edgeArrowImage != null)
        {
            edgeArrowImage.gameObject.SetActive(false);

            edgeArrowImage.rectTransform.localRotation =
                Quaternion.identity;
        }
    }
}