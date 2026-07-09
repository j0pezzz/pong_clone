using Fusion;
using UnityEngine.SceneManagement;

namespace Project.Scripts.Game
{
    /// <summary>
    /// Handles everything game state related.
    /// </summary>
    public class GameManager : NetworkBehaviour
    {
        [Networked] public int Player1Points { get; private set; }
        [Networked] public int Player2Points { get; private set; }
        
        [Networked] public NetworkBool IsGamePaused { get; set; }
        [Networked] public NetworkBool IsGameDone { get; set; }
        [Networked] public int RequiredPoints { get; set; }
        
        private ChangeDetector _changeDetector;

        public override void Spawned()
        {
            bl_EventHandler.Match.OnGameStart += OnGameStart;
            bl_EventHandler.Match.OnGameRestart += OnGameRestart;
            bl_EventHandler.Match.OnGlobalGamePause += OnGamePaused;
            bl_EventHandler.Match.OnTeamPointAdd += AddPoint;
            _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

            if (HasInputAuthority)
            {
                IsGamePaused = true;
                IsGameDone = false;
                RequiredPoints = NetworkHandler.GameRequiredPoints;
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            bl_EventHandler.Match.OnGameStart -= OnGameStart;
            bl_EventHandler.Match.OnGameRestart -= OnGameRestart;
            bl_EventHandler.Match.OnGlobalGamePause -= OnGamePaused;
            bl_EventHandler.Match.OnTeamPointAdd -= AddPoint;
        }

        public override void Render()
        {
            foreach (string change in _changeDetector.DetectChanges(this))
            {
                switch (change)
                {
                    case nameof(Player1Points):
                    case nameof(Player2Points):
                        bl_EventHandler.GameplayUI.DispatchPointsChange(Player1Points, Player2Points);
                        ValidatePointRequirement(Player1Points, Player2Points);
                        //TODO: we could here check if either team reached the required amount of points.
                        break;
                }
            }
        }

        void ValidatePointRequirement(int player1Points, int player2Points)
        {
            if (IsGamePaused) return;

            if (player1Points >= RequiredPoints || player2Points >= RequiredPoints)
            {
                IsGameDone = true;

                // Get the winner and send an RPC to both players that the game is done.
                Team winningTeam = DetermineWinner();
                RPC_GameFinish(winningTeam);
            }
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
        
        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_GameFinish(Team winnerTeam)
        {
            GameUI.Instance.GameFinish.ShowFinish(winnerTeam);
            bl_EventHandler.Match.DispatchGameFinish();
        }

        void AddPoint(Team team)
        {
            if (!HasInputAuthority) return;

            if (team == Team.None)
            {
                Log.Error("Team.None is not a valid team, not adding any points");
            }
            
            Log.Info($"Point added for {team}");
            switch (team)
            {
                case Team.Team1:
                    Player1Points++;
                    break;
                case Team.Team2:
                    Player2Points++;
                    break;
            }
        }

        /// <summary>
        /// Invoked when game needs to start, which then spawns the ball
        /// and potentially AI if set so.
        /// </summary>
        void OnGameStart()
        {
            if (!HasInputAuthority) return;
            
            NetworkHandler.Instance.SpawnBall();
                
            // If we are playing against AI, spawn AI.
            if (NetworkHandler.Instance.currentGameModes == GameModes.PvE)
            {
                NetworkHandler.Instance.SpawnAI();
            }
        }

        
        //TODO: this is very WIP, this is probably the easiest way to handle restarting the match.
        // not yet tested.
        
        /// <summary>
        /// Invoked when the game has to be restarted.
        /// </summary>
        void OnGameRestart()
        {
            Runner.LoadScene(SceneRef.FromIndex(SceneUtility.GetBuildIndexByScenePath(NetworkHandler.Instance.InitialScenePath)));
        }

        /// <summary>
        /// Invoked when the game needs to be paused completely.
        /// </summary>
        /// <param name="isPaused"></param>
        void OnGamePaused(bool isPaused)
        {
            if (!HasInputAuthority) return;
            
            IsGamePaused = isPaused;
        }

        private static GameManager _instance;
        public static GameManager Instance
        {
            get
            {
                if (!_instance) _instance = FindAnyObjectByType<GameManager>();
                return _instance;
            }
        }
    }
}