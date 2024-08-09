using TMPro;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    public GameFinish GameFinish;
    public PauseMenu PauseMenu;
    public ScoreUI ScoreUI;
    [SerializeField] TextMeshProUGUI MaxScoreText;

    void Awake()
    {
        Instance = this;
        MaxScoreText.text = $"Played till either one gets {GameController.GameRequiredPoints} points";
    }

    static GameUI _instance;
    public static GameUI Instance
    {
        get => _instance;
        private set => _instance = value;
    }
}
