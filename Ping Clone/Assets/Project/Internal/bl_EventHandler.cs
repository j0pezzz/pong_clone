using System;

public static class bl_EventHandler
{
    public static class Network
    {
        public static Action<bool> Online;
        public static void DispatchOnlineStatus(bool online) => Online?.Invoke(online);
    }

    //TODO: this needs refactoring, most are super useless and just confusing.
    public static class Match
    {
        public static Action<bool> onMatch;
        public static void DispatchInMatchStatus(bool inMatch) => onMatch?.Invoke(inMatch);

        public static Action<bool> OnWaitingPlayers;
        public static void DispatchWaitingPlayers(bool waiting) => OnWaitingPlayers?.Invoke(waiting);

        public static Action<bool> OnTimerStart;
        public static void DispatchTimerStart(bool isStarting) => OnTimerStart?.Invoke(isStarting);

        public static Action OnNewRound;
        public static void DispatchNewRound() => OnNewRound?.Invoke();

        public static Action<bool> OnGlobalGamePause;
        public static void DispatchGlobalGamePause(bool paused) => OnGlobalGamePause?.Invoke(paused);

        public static Action OnGameStart;
        public static void DispatchGameStart() => OnGameStart?.Invoke();
        
        public static Action onGameFinish;
        public static void DispatchGameFinish() => onGameFinish?.Invoke();

        public static Action OnGameRestart;
        public static void DispatchGameRestart() => OnGameRestart?.Invoke();

        public static Action<Team> OnTeamPointAdd;
        public static void DispatchPointAddition(Team team) => OnTeamPointAdd?.Invoke(team);
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

    public static class GameplayUI
    {
        public static Action<int, int> OnPointsChange;

        public static void DispatchPointsChange(int player1Points, int player2Points) =>
            OnPointsChange?.Invoke(player1Points, player2Points);

        public static Action<float> OnStartingTimerChange;
        public static void DispatchStartingTimerChange(float seconds) => OnStartingTimerChange?.Invoke(seconds);

        public static Action<float, bool> OnRoundTimerChange;

        public static void DispatchRoundTimerChange(float elapsedTime, bool show) =>
            OnRoundTimerChange?.Invoke(elapsedTime, show);
    }
}
