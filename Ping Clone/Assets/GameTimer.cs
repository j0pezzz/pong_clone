using Fusion;
using TMPro;
using UnityEngine;

public class GameTimer : NetworkBehaviour
{
    [Networked] public TickTimer Timer { get; set; }
    [Networked] public int IsGamePaused { get; set; }
    [SerializeField] GameObject Content;
    [SerializeField] TextMeshProUGUI TimeText;

    bool _isExpired = false;
    bool _isGamePaused
    {
        get => IsGamePaused > 0;
        set => IsGamePaused = IsGamePaused >= 0 ?
            value ? IsGamePaused + 1 : -(IsGamePaused + 1) :
            value ? -(IsGamePaused - 1) : IsGamePaused - 1;
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
        bl_EventHandler.Match.TimerStart += OnTimerStart;
        Debug.LogWarning("GameTimer spawned!");
    }

    void OnPause(bool pause)
    {
        _isGamePaused = pause;
    }

    void OnTimerStart()
    {
        Timer = TickTimer.CreateFromSeconds(Runner, 10);
    }

    /// <summary>
    /// Since we want both players do show the time on their own, we are using Render.
    /// </summary>
    public override void Render()
    {
        base.Render();

        if (Timer.Expired(Runner) && !_isExpired)
        {
            Debug.LogWarning("Timer expired!");
            _isExpired = true;
            Content.SetActive(false);
            bl_EventHandler.Match.DispatchPauseEvent(false);
        }

        if (Timer.IsRunning && !_isExpired)
        {
            if (!Content.activeInHierarchy) Content.SetActive(true);

            Debug.LogWarning("Running");
            string time = StringUtility.GetTimeFormat((int)Timer.RemainingTicks(Runner));
            TimeText.text = $"STARTING IN {time}";
        }
    }
}
