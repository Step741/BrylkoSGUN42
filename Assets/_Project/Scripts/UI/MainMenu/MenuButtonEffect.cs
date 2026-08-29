using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

public class MenuButtonEffect : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [Header("Scale")]
    [SerializeField]
    private float hoverScale = 1.08f;

    [SerializeField]
    private float pressedScale = 0.95f;


    [Header("Animation")]
    [SerializeField]
    private float hoverDuration = 0.2f;

    [SerializeField]
    private float pressDuration = 0.1f;


    [Header("Text")]
    [SerializeField]
    private TextMeshProUGUI buttonText;

    [SerializeField]
    private Color normalColor = Color.white;

    [SerializeField]
    private Color hoverColor = Color.white;


    private Vector3 normalScale;

    private Tween scaleTween;

    private Tween colorTween;


    private void Awake()
    {
        normalScale =
            transform.localScale;


        if (buttonText == null)
        {
            buttonText =
                GetComponentInChildren<TextMeshProUGUI>();
        }


        if (buttonText != null)
        {
            normalColor =
                buttonText.color;
        }
    }

    public void OnPointerEnter(
        PointerEventData eventData)
    {
        AnimateScale(
            normalScale * hoverScale
        );


        if (buttonText != null)
        {
            colorTween?.Kill();


            colorTween =
                buttonText
                    .DOColor(
                        hoverColor,
                        hoverDuration
                    )
                    .SetEase(
                        Ease.OutQuad
                    )
                    .SetUpdate(true);
        }
    }


    public void OnPointerExit(
        PointerEventData eventData)
    {
        AnimateScale(
            normalScale
        );


        if (buttonText != null)
        {
            colorTween?.Kill();


            colorTween =
                buttonText
                    .DOColor(
                        normalColor,
                        hoverDuration
                    )
                    .SetEase(
                        Ease.OutQuad
                    )
                    .SetUpdate(true);
        }
    }

    public void OnPointerDown(
        PointerEventData eventData)
    {
        scaleTween?.Kill();


        scaleTween =
            transform
                .DOScale(
                    normalScale * pressedScale,
                    pressDuration
                )
                .SetEase(
                    Ease.OutQuad
                )
                .SetUpdate(true);
    }


    public void OnPointerUp(
        PointerEventData eventData)
    {
        AnimateScale(
            normalScale * hoverScale
        );
    }

    private void AnimateScale(
        Vector3 targetScale)
    {
        scaleTween?.Kill();


        scaleTween =
            transform
                .DOScale(
                    targetScale,
                    hoverDuration
                )
                .SetEase(
                    Ease.OutBack
                )
                .SetUpdate(true);
    }

    //CLEANUP
    private void OnDestroy()
    {
        scaleTween?.Kill();

        colorTween?.Kill();
    }
}