using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
    public GameFinish GameFinish;
    public PauseMenu PauseMenu;
    public ScoreUI ScoreUI;
    [SerializeField] TextMeshProUGUI MaxScoreText;
    [SerializeField] GameObject WaitingForPlayersUI;
    [SerializeField] TextMeshProUGUI SessionID;
    public GameObject PlayerLeft;

    void Awake()
    {
        Instance = this;

        bl_EventHandler.Match.onWaitingPlayers += WaitingForPlayers;
        bl_EventHandler.Match.onGamePoints += OnGamePoints;
    }

    void OnDisable()
    {
        bl_EventHandler.Match.onWaitingPlayers -= WaitingForPlayers;
        bl_EventHandler.Match.onGamePoints -= OnGamePoints;
    }

    void OnGamePoints(int points)
    {
        MaxScoreText.text = $"Played till either one gets {GameTimer.Instance.RequiredPoints} points";
    }

    void WaitingForPlayers(bool waiting)
    {
        SessionID.text = GameController.Instance.SessionInfo.Name;
        WaitingForPlayersUI.SetActive(waiting);
    }

    /// <summary>
    /// Sends an callback to restart the scene.
    /// </summary>
    public void StartAgain()
    {
        bl_EventHandler.Match.DispatchGameRestart();
    }

    /// <summary>
    /// Leaving session causes all NetworkRunners to shutdown and loading 'MainMenu' scene.
    /// </summary>
    public void LeaveSession()
    {
        GameController.Instance.ShutdownAll();
        SceneManager.LoadScene("MainMenu");
    }

    static GameUI _instance;
    public static GameUI Instance
    {
        get => _instance;
        private set => _instance = value;
    }
}
