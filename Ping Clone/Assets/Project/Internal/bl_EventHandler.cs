using System;

public static class bl_EventHandler
{
    public static class Network
    {
        public static Action Online;
        public static void DispatchOnlineStatus() => Online?.Invoke();
    }

    public static class Match
    {
        public static Action<bool> InMatch;
        public static void DispatchInMatchStatus(bool inMatch) => InMatch?.Invoke(inMatch);

        public static Action<bool> WaitingPlayers;
        public static void DispatchWaitingStatus(bool waiting) => WaitingPlayers?.Invoke(waiting);

        public static Action TimerStart;
        public static void DispatchTimerStart() => TimerStart?.Invoke();

        public static Action<bool> onPauseCall;
        public static void DispatchPauseEvent(bool paused) => onPauseCall?.Invoke(paused);

        public static Action onGameFinish;
        public static void DispatchGameFinish() => onGameFinish?.Invoke();
    }

    public static class Menu
    {
        public static Action<bool> CreatingRoom;
        public static void DispatchRoomCreate(bool creating) => CreatingRoom?.Invoke(creating);

        public static Action<bool> JoinRoom;
        public static void DispatchRoomJoin(bool joining) => JoinRoom?.Invoke(joining);

        public static Action<string> NoRoom;
        public static void DispatchNoRoomToJoin(string message) => NoRoom?.Invoke(message);
    }
}
