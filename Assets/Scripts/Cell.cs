using UnityEngine;
using UnityEngine.EventSystems;

public class Cell : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public event System.Action<Vector2Int> OnClick;

    [SerializeField]
    private GameObject _selectedPlane;

    [SerializeField]
    private GameObject _turnPlane;

    [SerializeField]
    private GameObject _activeFigure;


    public Vector2Int Position { get; private set; }

    public void Initialize(Vector2Int position)
    {
        Position = position;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClick?.Invoke(Position);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _selectedPlane.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _selectedPlane.SetActive(false);
    }

    public void ShowTurnPanel()
    {
        _turnPlane.SetActive(true);
    }

    public void HideTurnPanel()
    {
        _turnPlane.SetActive(false);
    }

    public void ShowActiveFigure()
    {
        _activeFigure.SetActive(true);
    }

    public void HideActiveFigure()
    {
        _activeFigure.SetActive(false);
    }
}
