using Fusion;
using Project.Internal.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameTimer : NetworkBehaviour
{
    [Networked] public TickTimer StartingTimer { get; set; }
    [Networked] public NetworkBool IsGamePaused { get; set; }
    [Networked] public NetworkBool IsGameDone { get; set; }
    [Networked] public int RequiredPoints { get; set; }
    [Networked, OnChangedRender(nameof(Team1PointsChanged))] public int Player1Points { get; private set; }
    [Networked, OnChangedRender(nameof(Team2PointsChanged))] public int Player2Points { get; private set; }
    [SerializeField] GameObject Content;
    [SerializeField] TextMeshProUGUI StartingText;
    [SerializeField] TextMeshProUGUI RoundTimer;

    public static int InitialTick;
    bool _roundStart;
    bool _startTimerExpired;
    private bool _isHost;

    public override void Spawned()
    {
        bl_EventHandler.Match.onPauseCall += OnPause;
        bl_EventHandler.Match.onTimerStart += OnTimerStart;
        bl_EventHandler.Match.onScoreCheck += CheckTeamScore;
        bl_EventHandler.Match.onGameRestart += OnMatchRestart;

        if (GameController.Instance.IsHost)
        {
            IsGamePaused = true;
            IsGameDone = false;
            RequiredPoints = GameController.GameRequiredPoints;
        }

        bl_EventHandler.Match.DispatchGamePoints(RequiredPoints);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        bl_EventHandler.Match.onPauseCall -= OnPause;
        bl_EventHandler.Match.onTimerStart -= OnTimerStart;
        bl_EventHandler.Match.onScoreCheck -= CheckTeamScore;
        bl_EventHandler.Match.onGameRestart-= OnMatchRestart;
    }

    void OnMatchRestart()
    {
        //TODO: Need to test if loading the same scene again like this works.
        Runner.LoadScene(SceneRef.FromIndex(SceneUtility.GetBuildIndexByScenePath(GameController.Instance.InitialScenePath)));
    }

    public static void AddScore(Team team)
    {
        switch (team)
        {
            case Team.Team1:
                Instance.Player1Points++;
                break;
            case Team.Team2:
                Instance.Player2Points++;
                break;
        }
    }

    void Team1PointsChanged() => GameUI.Instance.ScoreUI.ScoreChange(Team.Team1, Player1Points);

    void Team2PointsChanged() => GameUI.Instance.ScoreUI.ScoreChange(Team.Team2, Player2Points);

    public void CheckTeamScore()
    {
        if (IsGamePaused) return;

        if (Player1Points >= RequiredPoints || Player2Points >= RequiredPoints)
        {
            IsGameDone = true;

            // Get the winner and send an RPC to both players that the game is done.
            Team winningTeam = DetermineWinner();
            RPC_GameFinish(winningTeam);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_GameFinish(Team winnerTeam)
    {
        GameUI.Instance.GameFinish.ShowFinish(winnerTeam);
        bl_EventHandler.Match.DispatchGameFinish();
    }

    Team DetermineWinner()
    {
        if (Player1Points >= RequiredPoints)
        {
            return Team.Team1;
        }

        if (Player2Points >= RequiredPoints)
        {
            return Team.Team2;
        }

        return Team.None;
    }

    void OnPause(bool pause)
    {
        IsGamePaused = pause;
    }

    void OnTimerStart()
    {
        StartingText.gameObject.SetActive(true);
        StartingTimer = TickTimer.CreateFromSeconds(Runner, 10);
    }

    public void RoundStart()
    {
        InitialTick = Runner.Tick;
        if (!_roundStart) _roundStart = true;
        RoundTimer.gameObject.SetActive(true);
    }

    public override void FixedUpdateNetwork()
    {
        //TODO: we want the host to handle 
    }

    //TODO: this is fucking stupid, probably did not know how to do this properly before.
    
    /// <summary>
    /// Since we want both players do show the time on their own, we are using Render.
    /// </summary>
    public override void Render()
    {
        base.Render();

        if (StartingTimer.Expired(Runner) && !_startTimerExpired)
        {
            //Debug.LogWarning("GameTimer (Render): Start Timer expired!");

            if (GameController.Instance.IsHost)
            {
                GameController.Instance.SpawnBall();
                
                // If we are playing against AI, spawn AI.
                if (GameController.Instance.currentGameModes == GameModes.PvE)
                {
                    GameController.Instance.SpawnAI();
                }
                
                IsGamePaused = false;
            }
            
            _startTimerExpired = true;
            StartingText.gameObject.SetActive(false);
            bl_EventHandler.Match.DispatchPauseEvent(false);
            RoundStart();
        }

        if (StartingTimer.IsRunning && !_startTimerExpired)
        {
            if (!Content.activeInHierarchy) Content.SetActive(true);

            float remainingSeconds = StartingTimer.GetSecondsFloat(Runner);

            //Debug.LogWarning("GameTimer (Render): Start Timer running.");
            string time = StringUtility.GetTimeFormat(Mathf.FloorToInt(remainingSeconds / 60), Mathf.FloorToInt(remainingSeconds % 60));
            StartingText.SetText($"STARTING IN {time}");
        }

        if (_roundStart && !IsGameDone)
        {
            int elapsedTicks = Runner.Tick - InitialTick;

            float elapsedTime = elapsedTicks / (float)Runner.TickRate;

            string formattedTime = FormatTime(elapsedTime);

            //Debug.LogWarning($"Round has been played for {formattedTime}");

            RoundTimer.text = formattedTime;
        }

        if (IsGameDone)
        {
            RoundTimer.gameObject.SetActive(false);
        }
    }

    string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        return $"{minutes:00}:{seconds:00}";
    }
    
    static GameTimer _instance;
    public static GameTimer Instance
    {
        get
        {
            if (!_instance) _instance = FindAnyObjectByType<GameTimer>();
            return _instance;
        }
    }
}
