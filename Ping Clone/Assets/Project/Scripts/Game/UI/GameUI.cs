using TMPro;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    public GameFinish GameFinish;
    public PauseMenu PauseMenu;
    public ScoreUI ScoreUI;
    [SerializeField] TextMeshProUGUI MaxScoreText;
    [SerializeField] GameObject WaitingForPlayersUI;
    [SerializeField] TextMeshProUGUI SessionID;

    void Awake()
    {
        Instance = this;
        MaxScoreText.text = $"Played till either one gets {GameController.GameRequiredPoints} points";

        bl_EventHandler.Match.WaitingPlayers += WaitingForPlayers;
    }

    void OnDisable()
    {
        bl_EventHandler.Match.WaitingPlayers -= WaitingForPlayers;
    }

    void WaitingForPlayers(bool waiting)
    {
        SessionID.text = GameController.Instance.SessionInfo.Name;
        WaitingForPlayersUI.SetActive(waiting);
    }

    static GameUI _instance;
    public static GameUI Instance
    {
        get => _instance;
        private set => _instance = value;
    }
}
