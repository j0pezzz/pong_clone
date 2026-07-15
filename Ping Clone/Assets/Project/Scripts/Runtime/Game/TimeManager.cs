using Fusion;
using Project.Internal.Utility;
using Project.Scripts.Game;

/// <summary>
/// Handles everything time related.
/// </summary>
/// HasStateAuthority needs to be used instead of HasInputAuthority because this is a scene NetworkObject.
public class TimeManager : NetworkBehaviour
{
    [Networked] public TickTimer StartingTimer { get; set; }
    [Networked] private Tick InitialTick { get; set; }
    [Networked] private NetworkBool RoundStart { get; set; }

    public override void Spawned()
    {
        bl_EventHandler.Match.OnNewRound += OnNewRound;
        bl_EventHandler.Match.OnTimerStart += OnTimerStart;
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

    public void OnNewRound(int requiredPoints)
    {
        InitialTick = Runner.Tick;
        RoundStart = true;
    }

    /// <summary>
    /// Since we want both players do show the time on their own, we are using Render.
    /// </summary>
    public override void Render()
    {
        if (StartingTimer.Expired(Runner))
        {
            //Debug.LogWarning("GameTimer (Render): Start Timer expired!")
            
            bl_EventHandler.Match.DispatchGameStart();
            bl_EventHandler.Match.DispatchTimerStart(false);
            bl_EventHandler.Match.DispatchGlobalGamePause(false);
            bl_EventHandler.Match.DispatchNewRound(Runner.SessionInfo.GetGameSettings().RequiredPoints);
            StartingTimer = TickTimer.None;
        }

        if (StartingTimer.IsRunning)
        {
            float remainingSeconds = StartingTimer.GetSecondsFloat(Runner);
            
            bl_EventHandler.GameplayUI.DispatchStartingTimerChange(remainingSeconds);
        }

        if (RoundStart && !GameManager.Instance.IsGameDone)
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
