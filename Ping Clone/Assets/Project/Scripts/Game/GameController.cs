using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
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
    NetworkRunner _serverNetworkRunner;

    public SessionInfo SessionInfo { get; private set; }
    #endregion

    #region Public Properties
    public static int GameRequiredPoints { get; private set; } = 5;
    public static AIDifficulty AIDifficulty { get; private set; }
    public bool IsOnline { get; private set; }
    public GameModes currentGameModes;
    public bool isMobile;
    public bool IsHost => _serverNetworkRunner.IsServer || _serverNetworkRunner.IsSharedModeMasterClient;
    #endregion

    #region Private Members
    Dictionary<int, PlayerControlller> playerControllers = new();
    Dictionary<int, AIController> aiControllers = new();
    [HideInInspector] public Ball cacheBall;
    Vector3 _spawnPoint;
    readonly Dictionary<PlayerRef, NetworkObject> _spawnedPlayers = new();
    #endregion

    void Awake()
    {
        isMobile = true;
        //isMobile = Application.isMobilePlatform;
        
        UnityEngine.Object[] gameControllers = FindObjectsByType(typeof(GameController), FindObjectsSortMode.None);
        if (gameControllers.Length > 1)
        {
            Destroy(gameControllers[1]);
        }

        DontDestroyOnLoad(gameObject);
        Instance = this;
        
        if (RunnerPrefab == null)
        {
            RunnerPrefab = FindAnyObjectByType<NetworkRunner>();
        }

        RunnerPrefab = Instantiate(RunnerPrefab);
        DontDestroyOnLoad(RunnerPrefab);
        RunnerPrefab.name = "Temp Network Runner";
    }

    public void StartRunner(bool online)
    {
        IsOnline = online;
        if (online)
        {
            bl_EventHandler.Network.DispatchOnlineStatus(true);
        }
    }

    public void StopRunner()
    {
        IsOnline = false;
        Destroy(RunnerPrefab.gameObject);
        bl_EventHandler.Network.DispatchOnlineStatus(false);
    }

    public System.Collections.IEnumerator HostRoom(string points)
    {
        int.TryParse(points, out int requiredPoints);
        string sessionName = UnityEngine.Random.Range(0, 99999).ToString();
        Debug.Log($"[GameController]: Generated session name = {sessionName}");

        GameMode mode = isMobile ? GameMode.Shared : GameMode.Host;
        
        _serverNetworkRunner = Instantiate(RunnerPrefab);
        _serverNetworkRunner.name = $"{mode} NetworkRunner";

        SceneRef sceneRef = SceneRef.FromIndex(SceneUtility.GetBuildIndexByScenePath(InitialScenePath));

        Task serverTask = InitializeRunner(_serverNetworkRunner, mode, NetAddress.Any(), sceneRef, sessionName);

        bl_EventHandler.Menu.DispatchRoomCreate(true);

        yield return new WaitUntil(() => serverTask.IsCompleted);

        if (!serverTask.IsCompletedSuccessfully)
        {
            Debug.LogError($"[GameController]: Failed to create a session. Exception: {serverTask.Exception}");

            ShutdownAll();
            yield break;
        }

        bl_EventHandler.Menu.DispatchRoomCreate(false);

        Debug.Log($"[GameController]: {_serverNetworkRunner.name} NetworkRunner is initialized");

        SessionInfo = _serverNetworkRunner.SessionInfo;

        GameRequiredPoints = requiredPoints;

        yield return new WaitForEndOfFrame();
    }

    public System.Collections.IEnumerator JoinRoom(string sessionName)
    {
        NetworkRunner client = Instantiate(RunnerPrefab);
        DontDestroyOnLoad(client);

        GameMode mode = isMobile ? GameMode.Shared : GameMode.Client;
        client.name = $"{mode} {UnityEngine.Random.Range(1, 9999)}";

        SceneRef sceneRef = SceneRef.FromIndex(SceneUtility.GetBuildIndexByScenePath(InitialScenePath));

        Task joinTask = InitializeRunner(client, mode, NetAddress.Any(), sceneRef, sessionName);

        bl_EventHandler.Menu.DispatchRoomJoin(true);

        yield return new WaitUntil(() => joinTask.IsCompleted);

        if (!joinTask.IsCompletedSuccessfully)
        {
            Debug.LogError($"GameController (JoinRoom): {joinTask.Exception}");

            ShutdownAll();
            yield break;
        }

        bl_EventHandler.Menu.DispatchRoomJoin(false);

        yield return new WaitForEndOfFrame();
    }

    //TODO: Need to re-work single player.
    public System.Collections.IEnumerator CreateRoomLocally(GameModes gameModes, string points, AIDifficulty difficulty)
    {
        int.TryParse(points, out int requiredPoints);
        string sessionName = UnityEngine.Random.Range(0, 99999).ToString();
        Debug.Log($"[GameController]: Generated session name = {sessionName}");

        _serverNetworkRunner = Instantiate(RunnerPrefab);
        _serverNetworkRunner.name = $"{GameMode.Single} NetworkRunner";

        SceneRef sceneRef = SceneRef.FromIndex(SceneUtility.GetBuildIndexByScenePath(InitialScenePath));

        Task serverTask = InitializeRunner(_serverNetworkRunner, GameMode.Single, NetAddress.Any(), sceneRef, sessionName);

        bl_EventHandler.Menu.DispatchRoomCreate(true);

        yield return new WaitUntil(() => serverTask.IsCompleted);

        if (!serverTask.IsCompletedSuccessfully)
        {
            Debug.LogError($"[GameController]: Failed to create local session. Exception: {serverTask.Exception}");

            ShutdownAll();
            yield break;
        }

        bl_EventHandler.Menu.DispatchRoomCreate(false);

        Debug.Log($"[GameController]: {_serverNetworkRunner.name} is initialized");

        SessionInfo = _serverNetworkRunner.SessionInfo;

        GameRequiredPoints = requiredPoints;
        currentGameModes = gameModes;
        AIDifficulty = difficulty;

        yield return new WaitForEndOfFrame();
    }

    protected async Task InitializeRunner(NetworkRunner runner, GameMode gameMode, NetAddress address, SceneRef sceneRef, string sessionName)
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

        runner.ProvideInput = gameMode != GameMode.Shared;

        // If using this implementation, Server needs to load that scene once Session created ??
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
            ObjectProvider = objectProvider,
            PlayerCount = 2,
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

    public void ShutdownAll()
    {
        foreach (NetworkRunner runner in NetworkRunner.Instances.ToList())
        {
            if (runner != null && runner.IsRunning)
            {
                runner.Shutdown();
            }
        }

        if (RunnerPrefab != null)
        {
            Destroy(RunnerPrefab.gameObject);
        }
    }

    public void ResetGame()
    {
        foreach (NetworkObject nObj in _spawnedPlayers.Values)
        {
            if (nObj.HasInputAuthority && nObj.TryGetComponent(out PlayerControlller controller))
            {
                controller.SetPlayerToInitPosition();
            }
        }

        foreach (PlayerControlller controller in playerControllers.Values)
        {
            controller.SetPlayerToInitPosition();
        }

        foreach (AIController aiController in aiControllers.Values)
        {
            aiController.SetToInit();
        }

        GameTimer.Instance.RoundStart();

        cacheBall.SetBallToInit();
    }

    NetworkObject SpawnPlayerOnline(PlayerRef playerRef)
    {
        // Get spawn point based on PlayerId. Host will be always 1 so he will get SpawnPoint1 and Client SpawnPoint2.
        _spawnPoint = playerRef.PlayerId == 1 ? SpawnPointManager.Instance.SpawnPoint1 : SpawnPointManager.Instance.SpawnPoint2;

        string spawnName = _spawnPoint == SpawnPointManager.Instance.SpawnPoint1 ? "SpawnPoint 1" : "SpawnPoint 2";

        Debug.Log($"[GameController]: Spawning {playerRef} to {spawnName}");

        // Might need to use 'SpawnAsync' instead of 'Spawn'.
        NetworkObject nObj = _serverNetworkRunner.Spawn(PlayerController, _spawnPoint, Quaternion.identity, playerRef);

        // Host renames the GameObject for them self.
        nObj.gameObject.name = $"Player {playerRef.PlayerId}";
        nObj.tag = $"Paddle{playerRef.PlayerId}";

        return nObj;
    }

    void SpawnPlayersOffline(int amount = 2)
    {
        for (int i = 0; i < amount; i++)
        {
            _spawnPoint = i == 0 ? SpawnPointManager.Instance.SpawnPoint1 : SpawnPointManager.Instance.SpawnPoint2;

            GameObject playerObject = Instantiate(PlayerController, _spawnPoint, Quaternion.identity);
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
    public void SpawnAI(int amount = 1)
    {
        if (!IsHost) return;
        
        for (int i = 0; i < amount; i++)
        {
            Vector3 aiSpawnPoint = amount == 1 ? SpawnPointManager.Instance.SpawnPoint2 : i == 0 ? SpawnPointManager.Instance.SpawnPoint1 : SpawnPointManager.Instance.SpawnPoint2;

            GameObject aiObject = Instantiate(AIPrefab, aiSpawnPoint, Quaternion.identity);
            aiObject.name = $"AI {i + 1}";
            aiObject.tag = $"Paddle2";

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

    /// <summary>
    /// Spawns the Ball inside the Network.
    /// </summary>
    public void SpawnBall()
    {
        NetworkObject ball = _serverNetworkRunner.Spawn(BallPrefab, new Vector3(0, 0, -0.25f), Quaternion.identity, _serverNetworkRunner.LocalPlayer);
        //GameObject ball = Instantiate(BallPrefab, new Vector3(0, 0, -0.25f), Quaternion.identity);

        if (!ball.TryGetComponent(out cacheBall))
        {
            Debug.LogError("GameController (SpawnBall): no Ball script attached");
        }
    }

    void CheckPlayerCount(NetworkRunner runner)
    {
        if (runner.ActivePlayers.Count() < 2)
        {
            Debug.Log("[GameController]: Not enough players, waiting for more players.");

            bl_EventHandler.Match.DispatchPauseEvent(true);
            bl_EventHandler.Match.DispatchWaitingStatus(true);

            //DEBUG:

            //bl_EventHandler.Match.DispatchWaitingStatus(false);
            //bl_EventHandler.Match.DispatchTimerStart();
        }
        else
        {
            Debug.LogWarning("[GameController]: Enough players, starting in 10 seconds.");

            bl_EventHandler.Match.DispatchWaitingStatus(false);
            bl_EventHandler.Match.DispatchTimerStart();
        }
    }

    #region Fusion Callbacks
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"[GameController]: Player {player.PlayerId} joined! We are host/master client: {runner.IsServer || runner.IsSharedModeMasterClient}");
        
        // If we are the Host, we spawn player characters.
        if (!IsHost) return;
        
        NetworkObject spawnedPlayer = SpawnPlayerOnline(player);

        runner.SetPlayerObject(player, spawnedPlayer);

        // Cache the NetworkObject for later use.
        _spawnedPlayers.Add(player, spawnedPlayer);

        if (IsOnline)
        {
            CheckPlayerCount(runner);
        }
        else
        {
            bl_EventHandler.Match.DispatchWaitingStatus(false);
            bl_EventHandler.Match.DispatchTimerStart();
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (!IsHost) return;
        
        Debug.Log($"[GameController]: {player} left");
        
        // Despawn the NetworkObject and remove it from the Dictionary.
        if (_spawnedPlayers.TryGetValue(player, out NetworkObject nObj))
        {
            runner.Despawn(nObj);
            _spawnedPlayers.Remove(player);
        }
            
        // Since a player left, it means we the Host are alone.
        // We should give the Host an option to either leave the session or start the match again.
        bl_EventHandler.Match.DispatchPauseEvent(true);
        GameUI.Instance.PlayerLeft.SetActive(true);

        if (cacheBall != null)
        {
            runner.Despawn(cacheBall.Object);
        }

        GameTimer.Instance.StartingTimer = TickTimer.None;
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        if (runner.GameMode == GameMode.Shared) return;
        
        NetworkInputData data = new();

        // W & S controls.
        data.Buttons.Set(Buttons.Up, Keyboard.current.wKey.isPressed);
        data.Buttons.Set(Buttons.Down, Keyboard.current.sKey.isPressed);

        // If calling Arrow controls after W & S controls, only Arrow controls will work.
        // Up & Down Arrow controls.
        //data.Buttons.Set(Buttons.Up, Input.GetKey(KeyCode.UpArrow));
        //data.Buttons.Set(Buttons.Down, Input.GetKey(KeyCode.DownArrow));

        input.Set(data);
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        // Since the NetworkRunner has been Shutdown, we just load the MainMenu.
        ShutdownAll();
        SceneManager.LoadScene("MainMenu");
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ReadOnlySpan<byte> data) { }

    #endregion

    static GameController _instance;
    public static GameController Instance
    {
        get => _instance;
        private set => _instance = value;
    }
}
