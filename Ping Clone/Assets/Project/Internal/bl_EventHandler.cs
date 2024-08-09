using System;

public static class bl_EventHandler
{
    public static Action<bool> onPauseCall;
    public static void DispatchPauseEvent(bool paused) => onPauseCall?.Invoke(paused);

    public static Action onGameFinish;
    public static void DispatchGameFinish() => onGameFinish?.Invoke();
}
