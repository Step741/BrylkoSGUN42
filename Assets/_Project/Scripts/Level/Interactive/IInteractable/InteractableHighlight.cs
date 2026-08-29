using DG.Tweening;
using UnityEngine;

public class InteractableHighlight : MonoBehaviour
{
    [Header("Highlight")]

    [SerializeField]
    private Color highlightColor = Color.yellow;

    [SerializeField]
    [Range(0f, 1f)]
    private float highlightStrength = 0.8f;


    [Header("Animation")]

    [SerializeField]
    private float fadeDuration = 0.2f;

    [SerializeField]
    private float pulseDuration = 0.8f;

    [SerializeField]
    private float pulseStrength = 0.08f;


    private Renderer[] renderers;

    private MaterialPropertyBlock propertyBlock;

    private Tween highlightTween;
    private Tween pulseTween;

    private float currentStrength;
    private bool isHighlighted;


    private static readonly int BaseColorId =
        Shader.PropertyToID("_BaseColor");

    private static readonly int ColorId =
        Shader.PropertyToID("_Color");

    private static readonly int EmissionColorId =
        Shader.PropertyToID("_EmissionColor");


    private void Awake()
    {
        renderers =
            GetComponentsInChildren<Renderer>();

        propertyBlock =
            new MaterialPropertyBlock();

        currentStrength = 0f;

        ApplyHighlight(0f);
    }


    private void OnDisable()
    {
        KillTweens();

        currentStrength = 0f;
        isHighlighted = false;

        ApplyHighlight(0f);
    }


    private void OnDestroy()
    {
        KillTweens();
    }

    public void SetHighlighted(bool highlighted)
    {
        if (isHighlighted == highlighted)
            return;


        isHighlighted = highlighted;


        //Останавливает предыдущие анимации
        KillTweens();


        float targetStrength =
            highlighted
                ? highlightStrength
                : 0f;


        highlightTween =
            DOTween
                .To(
                    () => currentStrength,
                    value =>
                    {
                        currentStrength = value;
                        ApplyHighlight(value);
                    },
                    targetStrength,
                    fadeDuration
                )
                .SetEase(
                    Ease.OutQuad
                )
                .SetLink(
                    gameObject
                )
                .OnKill(
                    () =>
                    {
                        highlightTween = null;
                    }
                )
                .OnComplete(
                    () =>
                    {
                        highlightTween = null;
                    }
                );


        if (highlighted)
        {
            StartPulse();
        }
    }

    private void StartPulse()
    {
        KillPulseTween();


        pulseTween =
            DOTween
                .To(
                    () => currentStrength,
                    value =>
                    {
                        currentStrength = value;
                        ApplyHighlight(value);
                    },
                    highlightStrength + pulseStrength,
                    pulseDuration
                )
                .SetEase(
                    Ease.InOutSine
                )
                .SetLoops(
                    -1,
                    LoopType.Yoyo
                )
                .SetLink(
                    gameObject
                )
                .OnKill(
                    () =>
                    {
                        pulseTween = null;
                    }
                );
    }

    private void ApplyHighlight(
        float strength
    )
    {
        if (renderers == null)
            return;


        Color emission =
            highlightColor * strength;


        foreach (Renderer renderer in renderers)
        {
            if (renderer == null)
                continue;


            renderer.GetPropertyBlock(
                propertyBlock
            );


            propertyBlock.SetColor(
                BaseColorId,
                Color.Lerp(
                    Color.white,
                    highlightColor,
                    strength
                )
            );


            propertyBlock.SetColor(
                ColorId,
                Color.Lerp(
                    Color.white,
                    highlightColor,
                    strength
                )
            );


            propertyBlock.SetColor(
                EmissionColorId,
                emission
            );


            renderer.SetPropertyBlock(
                propertyBlock
            );
        }
    }

    //CLEANUP
    private void KillTweens()
    {
        KillHighlightTween();
        KillPulseTween();
    }


    private void KillHighlightTween()
    {
        if (
            highlightTween != null &&
            highlightTween.IsActive()
        )
        {
            highlightTween.Kill();
        }

        highlightTween = null;
    }


    private void KillPulseTween()
    {
        if (
            pulseTween != null &&
            pulseTween.IsActive()
        )
        {
            pulseTween.Kill();
        }

        pulseTween = null;
    }
}