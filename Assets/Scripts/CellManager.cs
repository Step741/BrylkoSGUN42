using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Primitives;

public class CellManager : MonoBehaviour
{
    public event Action<Cell> OnCellClicked;
    private Cell[] _cells;
    private Unit[] _units;
    private Dictionary<Cell, Dictionary<NeighbourType, Cell>> _neighbours;


    private void Awake()
    {
        _cells = FindObjectsOfType<Cell>();
        _units = FindObjectsOfType<Unit>();

        _neighbours = new Dictionary<Cell, Dictionary<NeighbourType, Cell>>(_cells.Length * 8);
        var position = Array.ConvertAll(_cells, cell => cell.transform.position);
        var distance = 0f;

        for (int i = 0; i < _cells.Length; i++)
        {
            _cells[i].OnPointerClickEvent += OnCellClicked;
            var localNeighbours = new Dictionary<NeighbourType, Cell>();

            for (int j = 0; j < _cells.Length; j++)
            {
                if (i == j) continue;
                var source = position[i];
                var destination = position[j];

                var forward = destination.z.CompareTo(source.z);
                var right = destination.x.CompareTo(source.x);
                var type = (forward, right) switch
                {

                    (0, 1) => NeighbourType.Right,
                    (0, -1) => NeighbourType.Left,
                    (1, 0) => NeighbourType.Up,
                    (-1, 0) => NeighbourType.Down,
                    (1, 1) => NeighbourType.UpRight,
                    (1, -1) => NeighbourType.UpLeft,
                    (-1, 1) => NeighbourType.DownRight,
                    (-1, -1) => NeighbourType.DownLeft,
                    _ => default
                };
                localNeighbours[type] = _cells[j];
            }
            _neighbours[_cells[i]] = localNeighbours;
        }
        foreach (var unit in _units)
        {

            Cell nearest = FindClosestCell(unit.transform.position);
            unit.SetCurrentCell(nearest);
            nearest.Unit = unit;
        }
    }
    private void HandleCellClicked(Cell cell)
    {
        OnCellClicked?.Invoke(cell);
    }

    private Cell FindClosestCell(Vector3 position)
    {
        if (_cells.Length == 0) return null;

        Cell closestCell = _cells[0];
        float minDistance = Vector3.Distance(closestCell.transform.position, position);

        for (int i = 1; i < _cells.Length; i++)
        {
            float distance = Vector3.Distance(_cells[i].transform.position, position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestCell = _cells[i];
            }
        }

        return closestCell;
    }
}
