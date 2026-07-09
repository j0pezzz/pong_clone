using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI player1Score;
    [SerializeField] TextMeshProUGUI player2Score;

    private void Start()
    {
        bl_EventHandler.GameplayUI.OnPointsChange += UpdateScores;
    }

    private void OnDisable()
    {
        bl_EventHandler.GameplayUI.OnPointsChange -= UpdateScores;
    }

    public void UpdateScores(int p1Score, int p2Score)
    {
        player1Score.SetText(string.Format(bl_GameTexts.Player1Points, p1Score));
        player2Score.SetText(string.Format(bl_GameTexts.Player2Points, p2Score));
    }
}
