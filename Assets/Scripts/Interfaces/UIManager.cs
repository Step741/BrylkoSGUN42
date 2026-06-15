using UnityEngine;
using UnityEngine.Diagnostics;

public class UIManager : MonoBehaviour
{
    [SerializeField] 
    private GameObject _gameEndPanel;

    [SerializeField] 
    private GameObject _whiteTeamTurn;

    [SerializeField] 
    private GameObject _blackTeamTurn;

    private void Awake()
    {
        _gameEndPanel.SetActive(false);
    }

    public void Initialize(GameManager gameManager)
    {
        gameManager.OnTeamChange += GameManager_OnTeamChange;
        gameManager.OnGameEnd += GameManager_OnGameEnd;
    }

    private void GameManager_OnGameEnd(Utils.Team team)
    {
        _gameEndPanel.SetActive(true);
    }

    private void GameManager_OnTeamChange(Utils.Team team)
    {
        switch (team)
        {
            case Utils.Team.White:
                _blackTeamTurn.SetActive(false);
                _whiteTeamTurn.SetActive(true);
                break;

            case Utils.Team.Black:
                _whiteTeamTurn.SetActive(false);
                _blackTeamTurn.SetActive(true);
                break;

            default:
                break;
        }
    }
}
