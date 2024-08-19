using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI player1Score;
    [SerializeField] TextMeshProUGUI player2Score;

    public void UpdateScores(int p1Score, int p2Score)
    {
        player1Score.text = string.Format(bl_GameTexts.Player1Points, p1Score);
        player2Score.text = string.Format(bl_GameTexts.Player2Points, p2Score);
    }

    public void ScoreChange(Team team, int score)
    {
        switch (team)
        {
            case Team.Team1:
                player1Score.text = string.Format(bl_GameTexts.Player1Points, score);
                break;
            case Team.Team2:
                player2Score.text = string.Format(bl_GameTexts.Player2Points, score);
                break;
        }

        bl_EventHandler.Match.DispatchScoreCheck();
    }
}
