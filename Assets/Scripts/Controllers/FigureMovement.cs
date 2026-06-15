using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public class FigureMovement
    {
        private Board _board;

        public FigureMovement(Board board)
        {
            _board = board;
        }

        public List<Vector2Int> GetTurns(Figure figure, Vector2Int position)
        {
            List<Vector2Int> turns = new List<Vector2Int>();

            switch (figure.Type)
            {
                case FigureType.Checker:
                    turns.AddRange(GetCheckerTurns(figure, position));
                    break;

                case FigureType.King:
                    turns.AddRange(GetKingTurns(figure, position));
                    break;

                case FigureType.None:
                default:
                    break;
            }

            return turns;
        }

        public List<Vector2Int> GetAllTurns(Figure figure, Vector2Int position)
        {
            List<Vector2Int> turns = new List<Vector2Int>();

            switch (figure.Type)
            {
                case FigureType.Checker:
                    turns.AddRange(GetCheckerSimpleTurns(figure, position));
                    turns.AddRange(GetCheckerAttackTurns(figure, position));
                    break;

                case FigureType.King:
                    turns.AddRange(GetKingSimpleTurns(figure, position));
                    turns.AddRange(GetKingAttackTurns(figure, position));
                    break;

                case FigureType.None:
                default:
                    break;
            }

            return turns;
        }

        public List<Vector2Int> GetSimpleTurns(Figure figure, Vector2Int position)
        {
            List<Vector2Int> turns = new List<Vector2Int>();

            switch (figure.Type)
            {
                case FigureType.Checker:
                    return GetCheckerSimpleTurns(figure, position);

                case FigureType.King:
                    return GetKingSimpleTurns(figure, position);

                case FigureType.None:
                default:
                    break;
            }

            return turns;
        }

        public List<Vector2Int> GetAttackTurns(Figure figure, Vector2Int position)
        {
            List<Vector2Int> turns = new List<Vector2Int>();

            switch (figure.Type)
            {
                case FigureType.Checker:
                    return GetCheckerAttackTurns(figure, position);

                case FigureType.King:
                    return GetKingAttackTurns(figure, position);

                case FigureType.None:
                default:
                    break;
            }

            return turns;
        }

        #region OldTurns
        private List<Vector2Int> GetCheckerTurns(Figure figure, Vector2Int position)
        {
            List<Vector2Int> turns = new List<Vector2Int>();
            Vector2Int direction;

            if (figure.Team == Team.Black)
            {
                direction = new Vector2Int(+1, -1);
                turns.AddRange(GetCheckerTurnsByDirection(figure, position, direction, false));

                direction = new Vector2Int(+1, +1);
                turns.AddRange(GetCheckerTurnsByDirection(figure, position, direction, false));

                direction = new Vector2Int(-1, -1);
                turns.AddRange(GetCheckerTurnsByDirection(figure, position, direction, true));

                direction = new Vector2Int(-1, +1);
                turns.AddRange(GetCheckerTurnsByDirection(figure, position, direction, true));
            }
            else if (figure.Team == Team.White)
            {
                direction = new Vector2Int(+1, -1);
                turns.AddRange(GetCheckerTurnsByDirection(figure, position, direction, true));

                direction = new Vector2Int(+1, +1);
                turns.AddRange(GetCheckerTurnsByDirection(figure, position, direction, true));

                direction = new Vector2Int(-1, -1);
                turns.AddRange(GetCheckerTurnsByDirection(figure, position, direction, false));

                direction = new Vector2Int(-1, +1);
                turns.AddRange(GetCheckerTurnsByDirection(figure, position, direction, false));
            }

            return turns;
        }

        private List<Vector2Int> GetKingTurns(Figure figure, Vector2Int position)
        {
            List<Vector2Int> turns = new List<Vector2Int>();
            Vector2Int direction;

            direction = new Vector2Int(+1, -1);
            turns.AddRange(GetKingTurnsByDirection(figure, position, direction));

            direction = new Vector2Int(+1, +1);
            turns.AddRange(GetKingTurnsByDirection(figure, position, direction));

            direction = new Vector2Int(-1, -1);
            turns.AddRange(GetKingTurnsByDirection(figure, position, direction));

            direction = new Vector2Int(-1, +1);
            turns.AddRange(GetKingTurnsByDirection(figure, position, direction));

            return turns;
        }

        private List<Vector2Int> GetCheckerTurnsByDirection(Figure figure, Vector2Int position, Vector2Int direction, bool isOpposite)
        {
            List<Vector2Int> turns = new List<Vector2Int>();
            Vector2Int newPosition = position;

            newPosition += direction;

            if (_board.IsPositionCorrect(newPosition))
            {
                Figure figureOnNewPosition = _board.GetFigureByPosition(newPosition);

                if (isOpposite)
                {
                    if (figureOnNewPosition != null && figureOnNewPosition.Team != figure.Team)
                    {
                        newPosition += direction;

                        if (_board.IsPositionCorrect(newPosition) && _board.GetFigureByPosition(newPosition) == null)
                        {
                            turns.Add(newPosition);
                        }
                    }
                }
                else
                {
                    if (figureOnNewPosition == null)
                    {
                        turns.Add(newPosition);
                    }
                    else if (figureOnNewPosition.Team != figure.Team)
                    {
                        newPosition += direction;

                        if (_board.IsPositionCorrect(newPosition) && _board.GetFigureByPosition(newPosition) == null)
                        {
                            turns.Add(newPosition);
                        }
                    }
                }
            }

            return turns;
        }

        private List<Vector2Int> GetKingTurnsByDirection(Figure figure, Vector2Int position, Vector2Int direction)
        {
            List<Vector2Int> turns = new List<Vector2Int>();
            Vector2Int newPosition = position;
            newPosition += direction;

            while (_board.IsPositionCorrect(newPosition) && _board.GetFigureByPosition(newPosition) == null)
            {
                turns.Add(newPosition);
                newPosition += direction;
            }

            if (_board.IsPositionCorrect(newPosition))
            {
                if (_board.GetFigureByPosition(newPosition).Team != figure.Team)
                {
                    newPosition += direction;

                    if (_board.IsPositionCorrect(newPosition) && _board.GetFigureByPosition(newPosition) == null)
                    {
                        turns.Add(newPosition);
                    }
                }
            }

            return turns;
        }

        #endregion

        #region SimpleTurns
        public List<Vector2Int> GetCheckerSimpleTurns(Figure figure, Vector2Int position)
        {
            List<Vector2Int> turns = new List<Vector2Int>();
            Vector2Int direction;

            if (figure.Team == Team.Black)
            {
                direction = new Vector2Int(+1, -1);
                turns.AddRange(GetCheckerSimpleTurnsByDirection(figure, position, direction, false));

                direction = new Vector2Int(+1, +1);
                turns.AddRange(GetCheckerSimpleTurnsByDirection(figure, position, direction, false));
            }
            else if (figure.Team == Team.White)
            {
                direction = new Vector2Int(-1, -1);
                turns.AddRange(GetCheckerSimpleTurnsByDirection(figure, position, direction, false));

                direction = new Vector2Int(-1, +1);
                turns.AddRange(GetCheckerSimpleTurnsByDirection(figure, position, direction, false));
            }

            return turns;
        }

        public List<Vector2Int> GetKingSimpleTurns(Figure figure, Vector2Int position)
        {
            List<Vector2Int> turns = new List<Vector2Int>();
            Vector2Int direction;

            direction = new Vector2Int(+1, -1);
            turns.AddRange(GetKingSimpleTurnsByDirection(figure, position, direction));

            direction = new Vector2Int(+1, +1);
            turns.AddRange(GetKingSimpleTurnsByDirection(figure, position, direction));

            direction = new Vector2Int(-1, -1);
            turns.AddRange(GetKingSimpleTurnsByDirection(figure, position, direction));

            direction = new Vector2Int(-1, +1);
            turns.AddRange(GetKingSimpleTurnsByDirection(figure, position, direction));

            return turns;
        }

        public List<Vector2Int> GetCheckerSimpleTurnsByDirection(Figure figure, Vector2Int position, Vector2Int direction, bool isOpposite)
        {
            List<Vector2Int> turns = new List<Vector2Int>();
            Vector2Int newPosition = position;

            newPosition += direction;

            if (_board.IsPositionCorrect(newPosition))
            {
                Figure figureOnNewPosition = _board.GetFigureByPosition(newPosition);

                if (figureOnNewPosition == null)
                {
                    turns.Add(newPosition);
                }
            }

            return turns;
        }

        public List<Vector2Int> GetKingSimpleTurnsByDirection(Figure figure, Vector2Int position, Vector2Int direction)
        {
            List<Vector2Int> turns = new List<Vector2Int>();
            Vector2Int newPosition = position;
            newPosition += direction;

            while (_board.IsPositionCorrect(newPosition) && _board.GetFigureByPosition(newPosition) == null)
            {
                turns.Add(newPosition);
                newPosition += direction;
            }

            return turns;
        }

        #endregion

        #region AttackTurns
        public List<Vector2Int> GetCheckerAttackTurns(Figure figure, Vector2Int position)
        {
            List<Vector2Int> turns = new List<Vector2Int>();
            Vector2Int direction;

            if (figure.Team == Team.Black)
            {
                direction = new Vector2Int(+1, -1);
                turns.AddRange(GetCheckerAttackTurnsByDirection(figure, position, direction));

                direction = new Vector2Int(+1, +1);
                turns.AddRange(GetCheckerAttackTurnsByDirection(figure, position, direction));

                direction = new Vector2Int(-1, -1);
                turns.AddRange(GetCheckerAttackTurnsByDirection(figure, position, direction));

                direction = new Vector2Int(-1, +1);
                turns.AddRange(GetCheckerAttackTurnsByDirection(figure, position, direction));
            }
            else if (figure.Team == Team.White)
            {
                direction = new Vector2Int(+1, -1);
                turns.AddRange(GetCheckerAttackTurnsByDirection(figure, position, direction));

                direction = new Vector2Int(+1, +1);
                turns.AddRange(GetCheckerAttackTurnsByDirection(figure, position, direction));

                direction = new Vector2Int(-1, -1);
                turns.AddRange(GetCheckerAttackTurnsByDirection(figure, position, direction));

                direction = new Vector2Int(-1, +1);
                turns.AddRange(GetCheckerAttackTurnsByDirection(figure, position, direction));
            }

            return turns;
        }

        public List<Vector2Int> GetCheckerAttackTurnsByDirection(Figure figure, Vector2Int position, Vector2Int direction)
        {
            List<Vector2Int> turns = new List<Vector2Int>();
            Vector2Int newPosition = position;

            newPosition += direction;

            if (_board.IsPositionCorrect(newPosition))
            {
                Figure figureOnNewPosition = _board.GetFigureByPosition(newPosition);

                if (figureOnNewPosition != null && figureOnNewPosition.Team != figure.Team)
                {
                    newPosition += direction;

                    if (_board.IsPositionCorrect(newPosition) && _board.GetFigureByPosition(newPosition) == null)
                    {
                        turns.Add(newPosition);
                    }
                }
            }

            return turns;
        }

        public List<Vector2Int> GetKingAttackTurns(Figure figure, Vector2Int position)
        {
            List<Vector2Int> turns = new List<Vector2Int>();
            Vector2Int direction;

            direction = new Vector2Int(+1, -1);
            turns.AddRange(GetKingAttackTurnsByDirection(figure, position, direction));

            direction = new Vector2Int(+1, +1);
            turns.AddRange(GetKingAttackTurnsByDirection(figure, position, direction));

            direction = new Vector2Int(-1, -1);
            turns.AddRange(GetKingAttackTurnsByDirection(figure, position, direction));

            direction = new Vector2Int(-1, +1);
            turns.AddRange(GetKingAttackTurnsByDirection(figure, position, direction));

            return turns;
        }

        public List<Vector2Int> GetKingAttackTurnsByDirection(Figure figure, Vector2Int position, Vector2Int direction)
        {
            List<Vector2Int> turns = new List<Vector2Int>();
            Vector2Int newPosition = position;
            newPosition += direction;

            while (_board.IsPositionCorrect(newPosition) && _board.GetFigureByPosition(newPosition) == null)
            {
                newPosition += direction;
            }

            if (_board.IsPositionCorrect(newPosition))
            {
                if (_board.GetFigureByPosition(newPosition).Team != figure.Team)
                {
                    newPosition += direction;

                    if (_board.IsPositionCorrect(newPosition) && _board.GetFigureByPosition(newPosition) == null)
                    {
                        turns.Add(newPosition);
                    }
                }
            }

            return turns;
        }

        #endregion
    }
}
