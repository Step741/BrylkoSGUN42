using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

[RequireComponent(typeof(Selectable))]
public class UIButtonSound :
    MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler
{
    [Header("Volume")]

    [SerializeField]
    [Range(0f, 1f)]
    private float hoverVolume = 1f;

    [SerializeField]
    [Range(0f, 1f)]
    private float clickVolume = 1f;


    private ISoundService soundService;
    private UISoundLibrary uiSoundLibrary;
    private Selectable selectable;

    private bool isPointerInside;


    [Inject]
    public void Construct(
        ISoundService soundService,
        UISoundLibrary uiSoundLibrary)
    {
        this.soundService = soundService;
        this.uiSoundLibrary = uiSoundLibrary;
    }


    private void Awake()
    {
        selectable = GetComponent<Selectable>();
    }


    private void OnDisable()
    {
        isPointerInside = false;
    }

    public void OnPointerEnter(
        PointerEventData eventData)
    {
        if (!IsInteractable())
            return;

        if (isPointerInside)
            return;

        isPointerInside = true;

        if (uiSoundLibrary == null ||
            uiSoundLibrary.HoverSound == null)
        {
            return;
        }

        PlayUI(
            uiSoundLibrary.HoverSound,
            hoverVolume
        );
    }


    public void OnPointerExit(
        PointerEventData eventData)
    {
        isPointerInside = false;
    }

    public void OnPointerDown(
        PointerEventData eventData)
    {
        if (!IsInteractable())
            return;

        if (eventData.button !=
            PointerEventData.InputButton.Left)
        {
            return;
        }

        if (uiSoundLibrary == null ||
            uiSoundLibrary.ClickSound == null)
        {
            return;
        }

        PlayUI(
            uiSoundLibrary.ClickSound,
            clickVolume
        );
    }

    private void PlayUI(
        AudioClip clip,
        float volume)
    {
        if (soundService == null)
        {
            Debug.LogWarning(
                "UIButtonSound: ISoundService is not available."
            );

            return;
        }

        soundService.Play2D(
            clip,
            SoundType.UI,
            volume,
            1f
        );
    }

    private bool IsInteractable()
    {
        return selectable != null &&
               selectable.IsInteractable();
    }
}