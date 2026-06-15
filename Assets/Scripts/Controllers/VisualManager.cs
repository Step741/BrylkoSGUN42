using UnityEngine;
using Utils;

public class VisualManager : MonoBehaviour
{
    [SerializeField] 
    private Transform _startPosition;

    [SerializeField] 
    private Vector2 _spacing;

    [SerializeField] 
    private Transform _figuresContainer;

    [SerializeField] 
    private FiguresData _figuresData;

    private Transform[,] _figures;

    public void Initialize(Board board)
    {
        board.OnMoveFigure += Board_OnMoveFigure;
        board.OnDeleteFigure += Board_OnDeleteFigure;
        board.OnChangeToKing += Board_OnChangeToKing;

        _figures = new Transform[board.Size.x, board.Size.y];

        for (int x = 0; x < board.Size.x; x++)
        {
            for (int y = 0; y < board.Size.y; y++)
            {
                Vector2Int position = new Vector2Int(x, y);

                CreateFigure(position, board.GetFigureByPosition(position));
            }
        }
    }

    private void CreateFigure(Vector2Int position, Figure figure)
    {
        if (figure != null)
        {
            Transform fig = _figuresData.GetFigure(figure);
            _figures[position.x, position.y] = Instantiate(fig, GetWorldPosition(position), Quaternion.Euler(0f, 0f, 0f), _figuresContainer);
        }
    }

    private Vector3 GetWorldPosition(Vector2Int position)
    {
        return _startPosition.position + new Vector3(_spacing.y * position.y, 0f, -1 * _spacing.x * position.x);
    }

    private void Board_OnChangeToKing(Vector2Int position, Figure figure)
    {
        Board_OnDeleteFigure(position);
        CreateFigure(position, figure);
    }

    private void Board_OnDeleteFigure(Vector2Int position)
    {
        if (_figures[position.x, position.y] != null)
        {
            Destroy(_figures[position.x, position.y].gameObject);
            _figures[position.x, position.y] = null;
        }
    }

    private void Board_OnMoveFigure(Vector2Int oldPosition, Vector2Int newPosition)
    {
        _figures[oldPosition.x, oldPosition.y].position = GetWorldPosition(newPosition);
        _figures[newPosition.x, newPosition.y] = _figures[oldPosition.x, oldPosition.y];
        _figures[oldPosition.x, oldPosition.y] = null;
    }
}