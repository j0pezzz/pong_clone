using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : SimulationBehaviour, INetworkRunnerCallbacks
{
    #region Public Members
    public GameObject PlayerController;
    public GameObject AIPrefab;
    public GameObject BallPrefab;
    public NetworkRunner RunnerPrefab;
    [ScenePath]
    public string InitialScenePath;

    public float TopBound = 4f;
    public float BottomBound = -4f;

    public Color WinnerColor = Color.yellow;
    public Color LoserColor = Color.red;

    [NonSerialized]
    NetworkRunner _server;

    public SessionInfo SessionInfo { get; private set; }
    #endregion

    #region Public Properties
    public bool IsGameDone { get; set; } = false;

    public int Player1Points { get; private set; } = 0;
    public int Player2Points { get; private set; } = 0;
    public static int GameRequiredPoints { get; private set; } = 5;
    public static AIDifficulty AIDifficulty { get; private set; }
    public bool IsOnline { get; private set; }
    #endregion

    #region Private Members
    public GameMode CurrentGameMode;

    Dictionary<int, PlayerControlller> playerControllers = new();
    Dictionary<int, AIController> aiControllers = new();
    [HideInInspector] public Ball cacheBall;

    Vector3 spawnPoint;

    Dictionary<PlayerRef, NetworkObject> _spawnedPlayers = new();
    #endregion

    void Awake()
    {
        UnityEngine.Object[] gameControllers = FindObjectsByType(typeof(GameController), FindObjectsSortMode.None);
        if (gameControllers.Length > 1)
        {
            Destroy(gameControllers[1]);
        }

        DontDestroyOnLoad(gameObject);
        Instance = this;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void StartRunner()
    {
        IsOnline = true;
        RunnerPrefab = Instantiate(RunnerPrefab);
        DontDestroyOnLoad(RunnerPrefab);
        RunnerPrefab.name = "Temp Network Runner";
        bl_EventHandler.Network.DispatchOnlineStatus();
    }

    public System.Collections.IEnumerator HostRoom()
    {
        string sessionName = UnityEngine.Random.Range(0, 99999).ToString();
        Debug.LogWarning($"GameController (HostRoom): genereted session name={sessionName}");

        _server = Instantiate(RunnerPrefab);
        _server.name = Fusion.GameMode.Host.ToString();

        SceneRef sceneRef = SceneRef.FromIndex(SceneUtility.GetBuildIndexByScenePath(InitialScenePath));

        Task serverTask = InitializeRunner(_server, Fusion.GameMode.Host, NetAddress.Any(), sceneRef, sessionName);

        bl_EventHandler.Menu.DispatchRoomCreate(true);

        while (!serverTask.IsCompleted) yield return null;

        if (serverTask.IsFaulted)
        {
            Debug.LogError($"GameController (HostRoom): {serverTask.Exception}");

            ShutdownAll();
            yield break;
        }

        bl_EventHandler.Menu.DispatchRoomCreate(false);

        Debug.LogWarning($"GameController (HostRoom): NetworkRunner {_server.name} is initialized");

        SessionInfo = _server.SessionInfo;

        yield return new WaitForEndOfFrame();
    }

    public System.Collections.IEnumerator JoinRoom(string sessionName)
    {
        NetworkRunner client = Instantiate(RunnerPrefab);
        DontDestroyOnLoad(client);

        client.name = $"Client {UnityEngine.Random.Range(1, 9999)}";

        SceneRef sceneRef = SceneRef.FromIndex(SceneUtility.GetBuildIndexByScenePath(InitialScenePath));

        Task joinTask = InitializeRunner(client, Fusion.GameMode.Client, NetAddress.Any(), sceneRef, sessionName);

        bl_EventHandler.Menu.DispatchRoomJoin(true);

        while (!joinTask.IsCompleted) yield return null;

        if (joinTask.IsFaulted)
        {
            Debug.LogError($"GameController (JoinRoom): {joinTask.Exception}");

            ShutdownAll();
            yield break;
        }

        if (joinTask.IsCanceled)
        {
            Debug.LogError($"GameController (JoinRoom): {joinTask.Exception}");

            ShutdownAll();
            yield break;
        }

        bl_EventHandler.Menu.DispatchRoomJoin(false);

        yield return new WaitForEndOfFrame();
    }

    protected async Task InitializeRunner(NetworkRunner runner, Fusion.GameMode gameMode, NetAddress address, SceneRef sceneRef, string sessionName)
    {
        runner.TryGetComponent(out INetworkSceneManager sceneManager);
        if (sceneManager == null)
        {
            Debug.LogError($"NetworkRunner does not have any component implementing {nameof(INetworkSceneManager)}");
            sceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
        }

        runner.TryGetComponent(out INetworkObjectProvider objectProvider);
        if (objectProvider == null)
        {
            Debug.LogError($"NetworkRunner does not have any component implementing {nameof(INetworkObjectProvider)}");
            objectProvider = runner.gameObject.AddComponent<NetworkObjectProviderDefault>();
        }

        /// If using this implementation, Server needs to load that scene once Session created ??
        /*
        NetworkSceneInfo sceneInfo = new();
        if (sceneRef.IsValid)
        {
            sceneInfo.AddSceneRef(sceneRef, LoadSceneMode.Additive);
        }*/

        runner.AddCallbacks(this);

        StartGameResult result = await runner.StartGame(new StartGameArgs
        {
            GameMode = gameMode,
            Address = address,
            Scene = sceneRef,
            SessionName = sessionName,
            SceneManager = sceneManager,
            ObjectProvider = objectProvider
        });

        if (!result.Ok)
        {
            Debug.LogError($"GameController (InitializeRunner): {result.ShutdownReason}");
            if (result.ShutdownReason == ShutdownReason.GameNotFound)
            {
                bl_EventHandler.Menu.DispatchNoRoomToJoin(result.ErrorMessage);
            }

            ShutdownAll();
        }
    }

    void ShutdownAll()
    {
        foreach (NetworkRunner runner in NetworkRunner.Instances.ToList())
        {
            if (runner != null && runner.IsRunning)
            {
                runner.Shutdown();
            }
        }

        Destroy(RunnerPrefab.gameObject);
    }

    void OnSceneLoaded(Scene loadedScene, LoadSceneMode sceneLoadMode)
    {
        if (loadedScene.name == "Game")
        {
            SpawnBall();

            if (CurrentGameMode == GameMode.PvP)
            {
                SpawnPlayersOffline();
            }
            else if (CurrentGameMode == GameMode.PvE)
            {
                SpawnPlayersOffline(1);
                SpawnAI();
            }
            else if (CurrentGameMode == GameMode.EvE)
            {
                SpawnAI(2);
            }
        }
    }

    public void StartGameOffline(GameMode mode, string maxPoint, AIDifficulty difficulty)
    {
        CurrentGameMode = mode;
        int.TryParse(maxPoint, out int points);
        GameRequiredPoints = points;
        AIDifficulty = difficulty;
        SceneManager.LoadScene("Game");
    }

    public void ResetGame()
    {
        foreach (PlayerControlller controller in playerControllers.Values)
        {
            controller.SetPlayerToInitPosition();
        }

        foreach (AIController aiController in aiControllers.Values)
        {
            aiController.SetToInit();
        }

        cacheBall.SetBallToInit();
    }

    NetworkObject SpawnPlayerOnline(PlayerRef playerRef)
    {
        /// Get spawnpoint based on PlayerId. Host will be always 1 so he will get SpawnPoint1 and Client SpawnPoint2.
        spawnPoint = playerRef.PlayerId == 1 ? SpawnPointManager.Instance.SpawnPoint1 : SpawnPointManager.Instance.SpawnPoint2;

        string spawnpoint = spawnPoint == SpawnPointManager.Instance.SpawnPoint1 ? "SpawnPoint 1" : "SpawnPoint 2";

        Debug.LogWarning($"GameController (SpawnPlayerOnline): spawning Player {playerRef.PlayerId} to {spawnpoint}");

        /// Might need to use 'SpawnAsync' instead of 'Spawn'.
        NetworkObject nObj = _server.Spawn(PlayerController, spawnPoint, Quaternion.identity, playerRef);

        /// Host renames the GameObject for themselfs.
        nObj.gameObject.name = $"Player {playerRef.PlayerId}";

        return nObj;
    }

    void SpawnPlayersOffline(int amount = 2)
    {
        for (int i = 0; i < amount; i++)
        {
            spawnPoint = i == 0 ? SpawnPointManager.Instance.SpawnPoint1 : SpawnPointManager.Instance.SpawnPoint2;

            GameObject playerObject = Instantiate(PlayerController, spawnPoint, Quaternion.identity);
            playerObject.name = $"Player{i + 1}";
            playerObject.tag = $"Paddle{i + 1}";

            if (playerObject.TryGetComponent(out PlayerControlller controller))
            {
                controller.PlayerRef = i + 1;
                playerControllers.Add(i + 1, controller);
            }
        }
    }

    /// <summary>
    /// Spawns AI.
    /// </summary>
    /// <param name="amount">How many to spawn?</param>
    void SpawnAI(int amount = 1)
    {
        for (int i = 0; i < amount; i++)
        {
            Vector3 aiSpawnPoint;
            if (amount == 1)
            {
                aiSpawnPoint = SpawnPointManager.Instance.SpawnPoint2;
            }
            else
            {
                aiSpawnPoint = i == 0 ? SpawnPointManager.Instance.SpawnPoint1 : SpawnPointManager.Instance.SpawnPoint2;
            }

            GameObject aiObject = Instantiate(AIPrefab, aiSpawnPoint, Quaternion.identity);
            aiObject.name = $"AI {i + 1}";
            aiObject.tag = $"Paddle{i + 1}";

            if (!aiObject.TryGetComponent(out AIController aiController))
            {
                Debug.LogError("No AIController script attached to AI");
            }

            aiControllers.Add(i + 1, aiController);

            if (aiSpawnPoint == SpawnPointManager.Instance.SpawnPoint2)
            {
                aiController.IsLeftSide = false;
                aiObject.transform.Rotate(0, 180, 0);
            }
            else
            {
                aiController.IsLeftSide = true;
            }
        }
    }

    void SpawnBall()
    {
        GameObject ball = Instantiate(BallPrefab, new Vector3(0, 0, -0.25f), Quaternion.identity);

        if (!ball.TryGetComponent(out cacheBall))
        {
            Debug.LogError("No Ball script attached");
        }
    }

    public void AddScore(string player)
    {
        switch (player)
        {
            case "Player 1":
                Player1Points++;
                break;
            case "Player 2":
                Player2Points++;
                break;
        }

        GameUI.Instance.ScoreUI.UpdateScores(Player1Points, Player2Points);

        if (Player1Points >= GameRequiredPoints)
        {
            GameUI.Instance.GameFinish.ShowFinish("Player 1", "Player 2", Player1Points, Player2Points);
            IsGameDone = true;
            bl_EventHandler.Match.DispatchGameFinish();
        }
        else if (Player2Points >= GameRequiredPoints)
        {
            GameUI.Instance.GameFinish.ShowFinish("Player 2", "Player 1", Player2Points, Player1Points);
            IsGameDone = true;
            bl_EventHandler.Match.DispatchGameFinish();
        }
    }

    void CheckPlayerCount(NetworkRunner runner)
    {
        if (runner.ActivePlayers.Count() < 2)
        {
            Debug.LogWarning("GameController (CheckPlayerCount): not enough players, waiting.");

            bl_EventHandler.Match.DispatchPauseEvent(true);
            bl_EventHandler.Match.DispatchWaitingStatus(true);

            //DEBUG:

            //bl_EventHandler.Match.DispatchWaitingStatus(false);
            //bl_EventHandler.Match.DispatchTimerStart();
        }

        if (runner.ActivePlayers.Count() == 2)
        {
            Debug.LogWarning("GameController (CheckPlayerCount): enough players, starting in 10...");

            bl_EventHandler.Match.DispatchWaitingStatus(false);
            bl_EventHandler.Match.DispatchTimerStart();
        }
    }

    #region Fusion Callbacks
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.LogWarning($"GameController (OnPlayerJoined): Player {player.PlayerId} joined!");
        Debug.LogWarning($"GameController (OnPlayerJoined): Are we server? {runner.IsServer}");

        /// If we are the Host, we spawn player characters.
        if (runner.IsServer)
        {
            NetworkObject spawnedPlayer = SpawnPlayerOnline(player);

            runner.SetPlayerObject(player, spawnedPlayer);

            /// Cache the NetworkObject for later use.
            _spawnedPlayers.Add(player, spawnedPlayer);

            CheckPlayerCount(runner);
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        /// Despawn the NetworkObject and remove it from the Dictionary.
        if (_spawnedPlayers.TryGetValue(player, out NetworkObject nObj))
        {
            runner.Despawn(nObj);
            _spawnedPlayers.Remove(player);
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        NetworkInputData data = new();

        if (Input.GetKey(KeyCode.W))
        {
            data.Direction = Vector3.up;
        }
        if (Input.GetKey(KeyCode.S))
        {
            data.Direction = Vector3.down;
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            data.Direction = Vector3.up;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            data.Direction = Vector3.down;
        }

        input.Set(data);
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    #endregion

    static GameController _instance;
    public static GameController Instance
    {
        get => _instance;
        private set => _instance = value;
    }
}
