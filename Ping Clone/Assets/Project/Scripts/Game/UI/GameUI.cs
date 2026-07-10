using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles everything game UI related.
/// </summary>
public class GameUI : MonoBehaviour
{
    public GameFinish GameFinish;
    public PauseMenu PauseMenu;
    public ScoreUI ScoreUI;
    [SerializeField] TextMeshProUGUI MaxScoreText;
    [SerializeField] GameObject WaitingForPlayersUI;
    [SerializeField] TextMeshProUGUI SessionID;
    public GameObject PlayerLeft;
    
    [Header("Time References")]
    [SerializeField] GameObject content;
    [SerializeField] TextMeshProUGUI startingText;
    [SerializeField] TextMeshProUGUI roundTimer;

    void Awake()
    {
        bl_EventHandler.Match.OnTimerStart += OnTimerStart;
        bl_EventHandler.GameplayUI.OnStartingTimerChange += OnStartingTimerChanged;
        bl_EventHandler.GameplayUI.OnRoundTimerChange += OnRoundTimerChange;
        bl_EventHandler.Match.OnNewRound += OnNewRound;
        bl_EventHandler.Match.OnWaitingPlayers += WaitingForPlayers;
        
        MaxScoreText.SetText($"Played till either one gets {NetworkHandler.GameRequiredPoints} points");
    }

    void OnDisable()
    {
        bl_EventHandler.Match.OnTimerStart -= OnTimerStart;
        bl_EventHandler.GameplayUI.OnStartingTimerChange -= OnStartingTimerChanged;
        bl_EventHandler.GameplayUI.OnRoundTimerChange -= OnRoundTimerChange;
        bl_EventHandler.Match.OnNewRound -= OnNewRound;
        bl_EventHandler.Match.OnWaitingPlayers -= WaitingForPlayers;
    }

    void WaitingForPlayers(bool waiting)
    {
        SessionID.text = NetworkHandler.Instance.SessionInfo.Name;
        WaitingForPlayersUI.SetActive(waiting);
    }

    /// <summary>
    /// Sends a callback to restart the scene.
    /// </summary>
    public void StartAgain()
    {
        bl_EventHandler.Match.DispatchGameRestart();
    }

    void OnTimerStart(bool isStarting)
    {
        //content.SetActive(isStarting);
        startingText.gameObject.SetActive(isStarting);
    }

    void OnStartingTimerChanged(float seconds)
    {
        string time = StringUtility.GetTimeFormat(Mathf.FloorToInt(seconds / 60), Mathf.FloorToInt(seconds % 60));
        startingText.SetText($"STARTING IN {time}");
    }

    void OnRoundTimerChange(float elapsedTime, bool show)
    {
        //content.SetActive(show);

        if (elapsedTime.Equals(-1)) return;
        
        string formattedTime = StringUtility.GetTimeFormat(elapsedTime);
        
        roundTimer.SetText(formattedTime);
    }

    void OnNewRound()
    {
        roundTimer.gameObject.SetActive(true);
    }

    /// <summary>
    /// Leaving session causes all NetworkRunners to shutdown and loading 'MainMenu' scene.
    /// </summary>
    public void LeaveSession()
    {
        NetworkHandler.Instance.ShutdownAll();
        SceneManager.LoadScene("MainMenu");
    }

    static GameUI _instance;
    public static GameUI Instance
    {
        get
        {
            if (!_instance) _instance = FindAnyObjectByType<GameUI>();
            return _instance;
        }
    }
}
