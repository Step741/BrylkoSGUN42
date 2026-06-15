using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using Zenject;

public class GameManager : MonoBehaviour
{
    public event Action<Team> OnGameEnd;
    public event Action<Team> OnTeamChange;

    [SerializeField] 
    private Vector2Int _boardSize;

    [SerializeField] 
    private Transform _cellsContainer;

    [SerializeField] 
    private VisualManager _visualManager;

    [SerializeField] 
    private UIManager _uiManager;

    [SerializeField] 
    private Board _board;

    private FigureMovement _figureMovement;
    private bool _isFigureChose = false;
    private Vector2Int _choseFigurePosition;

    private List<Vector2Int> _availableTurns = new List<Vector2Int>();
    private List<Vector2Int> _availableFigures = new List<Vector2Int>();

    private Team _currenTeamTurn = Team.White;
    private bool _isGameEnd;
    private TurnState _currentTurnState = TurnState.Simple;

    [Inject]
    private Controls _controls;

    private void Awake()
    {
        Vector2Int size = new Vector2Int(_boardSize.y, _boardSize.x);

        _board = new Board(size);
        _board.OnDeleteFigure += Board_OnDeleteFigure;

        _figureMovement = new FigureMovement(_board);

        _controls.Game.Cancel.Enable();

        _controls.Game.Cancel.performed += Cancel_performed;
    }

    private void Cancel_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        CancelChoice();
    }

    private void Start()
    {
        for (int i = 0; i < _cellsContainer.childCount; i++)
        {
            if (_cellsContainer.GetChild(i).TryGetComponent(out Cell cell))
            {
                Vector2Int position = new Vector2Int(i / _board.Size.x, i % _board.Size.y);
                cell.Initialize(position);
                cell.OnClick += Cell_OnClick;
            }
        }

        _visualManager.Initialize(_board);
        _uiManager.Initialize(this);

        OnTeamChange?.Invoke(_currenTeamTurn);

        _availableFigures = GetAvailableFigures();
        ShowAvailableFigures();
    }

    private void Cell_OnClick(Vector2Int position)
    {
        if (_isGameEnd)
        {
            return;
        }

        HideAvailableTurns();

        if (_isFigureChose)
        {
            if (TryTurn(position))
            {
                _availableFigures.Clear();

                if (_currentTurnState == TurnState.Attack)
                {
                    if (_figureMovement.GetAttackTurns(_board.GetFigureByPosition(position), position).Count == 0)
                    {
                        ChangeTeam();
                    }
                }
                else
                {
                    ChangeTeam();
                }

                _availableFigures = GetAvailableFigures();
            }

            CancelChoice();
        }
        else
        {
            Figure figure = _board.GetFigureByPosition(position);

            if (_availableFigures.Contains(position) && figure != null && figure.Team == _currenTeamTurn)
            {
                _availableTurns = _figureMovement.GetAttackTurns(figure, position);
                _currentTurnState = TurnState.Attack;

                if (_availableTurns.Count == 0)
                {
                    _availableTurns = _figureMovement.GetSimpleTurns(figure, position);
                    _currentTurnState = TurnState.Simple;
                }

                HideAvailableFigures();
                ShowAvailableTurns();

                if (_availableTurns.Count > 0)
                {
                    _choseFigurePosition = position;
                    _isFigureChose = true;
                }
            }
        }
    }

    private List<Vector2Int> GetAvailableFigures()
    {
        List<Vector2Int> simpleFigures = new List<Vector2Int>();
        List<Vector2Int> attackFigures = new List<Vector2Int>();

        for (int x = 0; x < _board.Size.x; x++)
        {
            for (int y = 0; y < _board.Size.y; y++)
            {
                Vector2Int position = new Vector2Int(x, y);
                Figure fig = _board.GetFigureByPosition(position);

                if (fig != null && fig.Team == _currenTeamTurn)
                {
                    if (_figureMovement.GetSimpleTurns(fig, position).Count > 0)
                    {
                        simpleFigures.Add(position);
                    }

                    if (_figureMovement.GetAttackTurns(fig, position).Count > 0)
                    {
                        attackFigures.Add(position);
                    }
                }
            }
        }

        if (attackFigures.Count > 0)
        {
            return attackFigures;
        }

        return simpleFigures;
    }

    private void Board_OnDeleteFigure(Vector2Int position)
    {
        CheckEndGame();
    }

    private void CheckEndGame()
    {
        if (GetAvailableFigures().Count == 0)
        {
            _currenTeamTurn = (Team)(((int)_currenTeamTurn + 1) % Enum.GetValues(typeof(Team)).Length);
            OnGameEnd?.Invoke(_currenTeamTurn);
            _isGameEnd = true;
        }
        else if (_board.WhiteCheckersCount == 0)
        {
            OnGameEnd?.Invoke(Team.White);
            _isGameEnd = true;
        }
        else if (_board.BlackCheckersCount == 0)
        {
            OnGameEnd?.Invoke(Team.Black);
            _isGameEnd = true;
        }
    }

    private bool TryTurn(Vector2Int position)
    {
        if (_availableTurns.Contains(position))
        {
            _board.MoveFigure(_choseFigurePosition, position);
            return true;
        }

        return false;
    }

    private void ChangeTeam()
    {
        _currenTeamTurn = (Team)(((int)_currenTeamTurn + 1) % Enum.GetValues(typeof(Team)).Length);
        CheckEndGame();
        OnTeamChange?.Invoke(_currenTeamTurn);
    }

    private void ShowAvailableTurns()
    {
        foreach (Vector2Int pos in _availableTurns)
        {
            if (_cellsContainer.GetChild(pos.x * _board.Size.y + pos.y).TryGetComponent(out Cell cell))
            {
                cell.ShowTurnPanel();
            }
        }
    }

    private void HideAvailableTurns()
    {
        foreach (Vector2Int pos in _availableTurns)
        {
            if (_cellsContainer.GetChild(pos.x * _board.Size.y + pos.y).TryGetComponent(out Cell cell))
            {
                cell.HideTurnPanel();
            }
        }
    }

    private void ShowAvailableFigures()
    {
        foreach (Vector2Int pos in _availableFigures)
        {
            if (_cellsContainer.GetChild(pos.x * _board.Size.y + pos.y).TryGetComponent(out Cell cell))
            {
                cell.ShowActiveFigure();
            }
        }
    }

    private void HideAvailableFigures()
    {
        foreach (Vector2Int pos in _availableFigures)
        {
            if (_cellsContainer.GetChild(pos.x * _board.Size.y + pos.y).TryGetComponent(out Cell cell))
            {
                cell.HideActiveFigure();
            }
        }
    }

    private void CancelChoice()
    {
        HideAvailableTurns();

        if (_isFigureChose)
        {
            _isFigureChose = false;
            _availableTurns.Clear();
            ShowAvailableFigures();
        }
    }

    private void UnsubscribeFromCells()
    {
        for (int i = 0; i < _cellsContainer.childCount; i++)
        {
            if (_cellsContainer.GetChild(i).TryGetComponent(out Cell cell))
            {
                cell.OnClick -= Cell_OnClick;
            }
        }
    }
}

public enum TurnState
{
    Simple,
    Attack
}
