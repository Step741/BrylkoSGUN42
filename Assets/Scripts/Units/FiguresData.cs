using UnityEngine;
using Utils;

[CreateAssetMenu(fileName = "FiguresData", menuName = "SO/FiguresData")]
public class FiguresData : ScriptableObject
{
    [Header("White Figures")]
    [SerializeField] 
    private Transform _whiteCheckerPrefab;

    [SerializeField] 
    private Transform _whiteKingPrefab;

    [Header("Black Figures")]
    [SerializeField] 
    private Transform _blackCheckerPrefab;

    [SerializeField] 
    private Transform _blackKingPrefab;

    public Transform GetFigure(Figure figure)
    {
        if (figure.Type == FigureType.Checker)
        {
            switch (figure.Team)
            {
                case Team.White:
                    return _whiteCheckerPrefab;

                case Team.Black:
                    return _blackCheckerPrefab;

                default:
                    break;
            }
        }
        else if (figure.Type == FigureType.King)
        {
            switch (figure.Team)
            {
                case Team.White:
                    return _whiteKingPrefab;

                case Team.Black:
                    return _blackKingPrefab;

                default:
                    break;
            }
        }
        return null;
    }
}
