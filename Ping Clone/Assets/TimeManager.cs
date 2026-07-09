using Fusion;
using Project.Internal.Utility;
using Project.Scripts.Game;
using UnityEngine;

/// <summary>
/// Handles everything time related.
/// </summary>
public class TimeManager : NetworkBehaviour
{
    [Networked] public TickTimer StartingTimer { get; set; }

    public static int InitialTick;
    bool _roundStart;
    bool _startTimerExpired;
    private bool _isHost;

    public override void Spawned()
    {
        bl_EventHandler.Match.OnNewRound += OnNewRound;
        bl_EventHandler.Match.OnTimerStart += OnTimerStart;

        //bl_EventHandler.Match.DispatchGamePoints(RequiredPoints);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        bl_EventHandler.Match.OnNewRound -= OnNewRound;
        bl_EventHandler.Match.OnTimerStart -= OnTimerStart;
    }

    void OnTimerStart(bool isStarting)
    {
        StartingTimer = TickTimer.CreateFromSeconds(Runner, 10);
    }

    public void OnNewRound()
    {
        InitialTick = Runner.Tick;
        if (!_roundStart) _roundStart = true;
    }

    /// <summary>
    /// Since we want both players do show the time on their own, we are using Render.
    /// </summary>
    public override void Render()
    {
        base.Render();

        if (StartingTimer.Expired(Runner) && !_startTimerExpired)
        {
            //Debug.LogWarning("GameTimer (Render): Start Timer expired!");
            _startTimerExpired = true;
            
            bl_EventHandler.Match.DispatchGameStart();
            bl_EventHandler.Match.DispatchTimerStart(false);
            bl_EventHandler.Match.DispatchGlobalGamePause(false);
            OnNewRound();
        }

        if (StartingTimer.IsRunning && !_startTimerExpired)
        {
            float remainingSeconds = StartingTimer.GetSecondsFloat(Runner);
            
            bl_EventHandler.GameplayUI.DispatchStartingTimerChange(remainingSeconds);
        }

        if (_roundStart && !GameManager.Instance.IsGameDone)
        {
            int elapsedTicks = Runner.Tick - InitialTick;

            float elapsedTime = elapsedTicks / (float)Runner.TickRate;

            //Debug.LogWarning($"Round has been played for {elapsedTime}");
            
            bl_EventHandler.GameplayUI.DispatchRoundTimerChange(elapsedTime, true);
        }

        if (GameManager.Instance.IsGameDone)
        {
            bl_EventHandler.GameplayUI.DispatchRoundTimerChange(-1, false);
        }
    }
    
    static TimeManager _instance;
    public static TimeManager Instance
    {
        get
        {
            if (!_instance) _instance = FindAnyObjectByType<TimeManager>();
            return _instance;
        }
    }
}
