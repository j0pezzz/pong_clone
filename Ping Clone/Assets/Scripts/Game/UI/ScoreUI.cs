using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI player1Score;
    [SerializeField] TextMeshProUGUI player2Score;

    void Awake()
    {
        Instance = this;
    }

    public void UpdateScores(int p1Score, int p2Score)
    {
        player1Score.text = string.Format(bl_GameTexts.Player1Points, p1Score);
        player2Score.text = string.Format(bl_GameTexts.Player2Points, p2Score);
    }

    static ScoreUI _instance;
    public static ScoreUI Instance
    {
        get => _instance;
        private set => _instance = value;
    }
}
