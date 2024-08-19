using System;

public static class bl_EventHandler
{
    public static class Network
    {
        public static Action<bool> Online;
        public static void DispatchOnlineStatus(bool online) => Online?.Invoke(online);
    }

    public static class Match
    {
        public static Action<bool> onMatch;
        public static void DispatchInMatchStatus(bool inMatch) => onMatch?.Invoke(inMatch);

        public static Action<bool> onWaitingPlayers;
        public static void DispatchWaitingStatus(bool waiting) => onWaitingPlayers?.Invoke(waiting);

        public static Action onTimerStart;
        public static void DispatchTimerStart() => onTimerStart?.Invoke();

        public static Action<bool> onPauseCall;
        public static void DispatchPauseEvent(bool paused) => onPauseCall?.Invoke(paused);

        public static Action onGameFinish;
        public static void DispatchGameFinish() => onGameFinish?.Invoke();

        public static Action onScoreCheck;
        public static void DispatchScoreCheck() => onScoreCheck?.Invoke();

        public static Action<int> onGamePoints;
        public static void DispatchGamePoints(int points) => onGamePoints?.Invoke(points);

        public static Action onGameRestart;
        public static void DispatchGameRestart() => onGameRestart?.Invoke();
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
