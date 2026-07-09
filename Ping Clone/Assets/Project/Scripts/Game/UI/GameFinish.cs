using Project.Scripts.Game;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFinish : MonoBehaviour
{
    [SerializeField] GameObject Content;
    [SerializeField] TextMeshProUGUI WinnerText;

    public void ShowFinish(Team winningTeam)
    {
        Content.SetActive(true);

        string winner = winningTeam == Team.Team1 ? "Player 1" : winningTeam == Team.Team2 ? "Player 2" : "None";
        string loser = winningTeam == Team.Team1 ? "Player 2" : winningTeam == Team.Team2 ? "Player 1" : "None";
        
        int winningPoints = winningTeam == Team.Team1 ? GameManager.Instance.Player1Points : winningTeam == Team.Team2 ? GameManager.Instance.Player2Points : 0;
        int losingPoints = winningTeam == Team.Team1 ? GameManager.Instance.Player2Points : winningTeam == Team.Team2 ? GameManager.Instance.Player1Points : 0;

        Color winColor = NetworkHandler.Instance.WinnerColor;
        Color loseColor = NetworkHandler.Instance.LoserColor;

        WinnerText.SetText($"<color=#{ColorUtility.ToHtmlStringRGBA(winColor)}>{winner}</color> won with {winningPoints} points \n over <color=#{ColorUtility.ToHtmlStringRGBA(loseColor)}>{loser}'s</color> {losingPoints} points");
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
