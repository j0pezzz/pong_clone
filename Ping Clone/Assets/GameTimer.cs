using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameTimer : NetworkBehaviour
{
    [Networked] public TickTimer StartTimer { get; set; }
    [Networked] public int IsGamePaused { get; set; }
    [Networked] public int IsGameDone { get; set; }
    [Networked] public int RequiredPoints { get; set; }
    [Networked, OnChangedRender(nameof(Team1PointsChanged))] public int Player1Points { get; private set; }
    [Networked, OnChangedRender(nameof(Team2PointsChanged))] public int Player2Points { get; private set; }
    [SerializeField] GameObject Content;
    [SerializeField] TextMeshProUGUI StartingText;
    [SerializeField] TextMeshProUGUI RoundTimer;

    public static int _initialTick;
    bool _roundStart = false;

    bool _startTimerExpired = false;
    bool isGamePaused
    {
        get => IsGamePaused > 0;
        set => IsGamePaused = IsGamePaused >= 0 ?
            value ? IsGamePaused + 1 : -(IsGamePaused + 1) :
            value ? -(IsGamePaused - 1) : IsGamePaused - 1;
    }

    bool _isGameDone
    {
        get => IsGameDone > 0;
        set => IsGameDone = IsGameDone >= 0 ?
            value ? IsGameDone + 1 : -(IsGameDone + 1) :
            value ? -(IsGameDone - 1) : IsGameDone - 1;
    }

    static GameTimer _instance;
    public static GameTimer Instance
    {
        get => _instance;
        private set => _instance = value;
    }

    public override void Spawned()
    {
        Instance = this;
        bl_EventHandler.Match.onPauseCall += OnPause;
        bl_EventHandler.Match.onTimerStart += OnTimerStart;
        bl_EventHandler.Match.onScoreCheck += CheckTeamScore;
        bl_EventHandler.Match.onGameRestart += OnMatchRestart;

        if (Runner.IsServer)
        {
            _isGameDone = false;
            RequiredPoints = GameController.GameRequiredPoints;
        }

        bl_EventHandler.Match.DispatchGamePoints(RequiredPoints);
        Debug.LogWarning("GameTimer spawned!");
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
        if (isGamePaused) return;

        if (Player1Points >= RequiredPoints || Player2Points >= RequiredPoints)
        {
            _isGameDone = true;

            /// Get the winner and send an RPC to both players that the game is done.
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
        else if (Player2Points >= RequiredPoints)
        {
            return Team.Team2;
        }

        return Team.None;
    }

    void OnPause(bool pause)
    {
        isGamePaused = pause;
    }

    void OnTimerStart()
    {
        StartingText.gameObject.SetActive(true);
        StartTimer = TickTimer.CreateFromSeconds(Runner, 10);
    }

    public void RoundStart()
    {
        _initialTick = Runner.Tick;
        if (!_roundStart) _roundStart = true;
        RoundTimer.gameObject.SetActive(true);
    }

    /// <summary>
    /// Since we want both players do show the time on their own, we are using Render.
    /// </summary>
    public override void Render()
    {
        base.Render();

        if (StartTimer.Expired(Runner) && !_startTimerExpired)
        {
            //Debug.LogWarning("GameTimer (Render): Start Timer expired!");

            if (Runner.IsServer)
            {
                GameController.Instance.SpawnBall();

                /// If we are playing against AI, spawn AI.
                if (GameController.Instance.CurrentGameMode == GameMode.PvE)
                {
                    GameController.Instance.SpawnAI();
                }
            }

            _startTimerExpired = true;
            StartingText.gameObject.SetActive(false);
            bl_EventHandler.Match.DispatchPauseEvent(false);
            RoundStart();
        }

        if (StartTimer.IsRunning && !_startTimerExpired)
        {
            if (!Content.activeInHierarchy) Content.SetActive(true);

            //Debug.LogWarning("GameTimer (Render): Start Timer running.");
            string time = StringUtility.GetTimeFormat((int)StartTimer.RemainingTicks(Runner));
            StartingText.text = $"STARTING IN {time}";
        }

        if (_roundStart && !_isGameDone)
        {
            int elapsedTicks = Runner.Tick - _initialTick;

            float elapsedTime = elapsedTicks / (float)Runner.TickRate;

            string formattedTime = FormatTime(elapsedTime);

            //Debug.LogWarning($"Round has been played for {formattedTime}");

            RoundTimer.text = formattedTime;
        }

        if (_isGameDone)
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
}
