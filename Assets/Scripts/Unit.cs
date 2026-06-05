using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Unit : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
    [SerializeField]
    private float _moveSpeed = 2f;
    public Cell CurrentCell { get; private set; }
    public event Action<Unit> OnMoveEndCallback;


    public void SetCurrentCell(Cell cell)
    {
        CurrentCell = cell;
        transform.position = cell.transform.position;

    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        CurrentCell.OnPointerEnter(eventData);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        CurrentCell.OnPointerExit(eventData);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        CurrentCell.OnPointerClick(eventData);

    }

    public void MoveToCell(Cell cell)
    {
        StopAllCoroutines();
        StartCoroutine(MoveRoutine(cell));
    }
    private IEnumerator MoveRoutine(Cell cell)
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = cell.transform.position;
        float elapse = 0f;
        while (elapse < _moveSpeed)
        {
            elapse += Time.deltaTime;
            float t = elapse / _moveSpeed;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
        transform.position = endPos;
        CurrentCell = cell;

        OnMoveEndCallback?.Invoke(this);
    }

}
