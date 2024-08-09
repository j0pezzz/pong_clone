using Fusion;
using Fusion.Sockets;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    #region Public Members
    public GameObject PlayerController;
    public GameObject AIPrefab;
    public GameObject BallPrefab;
    public NetworkRunner RunnerPrefab;

    public float TopBound = 4f;
    public float BottomBound = -4f;

    public Color WinnerColor = Color.yellow;
    public Color LoserColor = Color.red;

    [NonSerialized]
    NetworkRunner _server;
    #endregion

    #region Public Properties
    public bool IsGameDone { get; set; } = false;
    public bool IsGamePaused { get; set; } = false;

    public int Player1Points { get; private set; } = 0;
    public int Player2Points { get; private set; } = 0;
    public static int GameRequiredPoints { get; private set; } = 5;
    public static AIDifficulty AIDifficulty { get; private set; }
    public bool IsOnline { get; private set; }
    #endregion

    #region Private Members
    public GameMode CurrentGameMode;

    System.Collections.Generic.Dictionary<int, PlayerControlller> playerControllers = new();
    System.Collections.Generic.Dictionary<int, AIController> aiControllers = new();
    [HideInInspector] public Ball cacheBall;

    Vector3 spawnPoint;
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

        SceneManager.sceneLoaded += OnSceneLoaded;
        bl_EventHandler.onPauseCall += OnPause;
    }

    public void StartRunner()
    {
        IsOnline = true;
        RunnerPrefab = Instantiate(RunnerPrefab);
        DontDestroyOnLoad(RunnerPrefab);
        RunnerPrefab.name = "Temp Network Runner";
    }

    public System.Collections.IEnumerator HostRoom()
    {
        string sessionName = Guid.NewGuid().ToString();
        Debug.LogWarning($"MenuHandler (HostRoom): generedted session name={sessionName}");

        _server = Instantiate(RunnerPrefab);
        _server.name = Fusion.GameMode.Host.ToString();

        SceneRef sceneRef = SceneRef.FromIndex(SceneManager.GetSceneByName("Game").buildIndex);

        Task serverTask = InitializeRunner(_server, Fusion.GameMode.Host, NetAddress.Any(), sceneRef, sessionName);

        while (!serverTask.IsCompleted) yield return new WaitForSeconds(1);

        if (serverTask.IsFaulted)
        {
            Debug.LogError($"GameController (HostRoom): {serverTask.Exception}");

            ShutdownAll();
            yield break;
        }

        yield return new WaitForEndOfFrame();
    }

    public System.Collections.IEnumerator JoinRoom(string sessionName)
    {
        yield return AddClient(Fusion.GameMode.Client, SceneRef.FromIndex(SceneManager.GetSceneByName("Game").buildIndex), sessionName);
    }

    Task AddClient(Fusion.GameMode mode, SceneRef scene, string sessionName)
    {
        NetworkRunner client = Instantiate(RunnerPrefab);
        DontDestroyOnLoad(client);

        client.name = $"Client {UnityEngine.Random.Range(1, 9999)}";

        var clientTask = InitializeRunner(client, mode, NetAddress.Any(), scene, sessionName);

        return clientTask;
    }

    protected virtual Task InitializeRunner(NetworkRunner runner, Fusion.GameMode gameMode, NetAddress address, SceneRef sceneRef, string sessionName)
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

        NetworkSceneInfo sceneInfo = new();
        if (sceneRef.IsValid)
        {
            sceneInfo.AddSceneRef(sceneRef, LoadSceneMode.Additive);
        }

        return runner.StartGame(new StartGameArgs
        {
            GameMode = gameMode,
            Address = address,
            Scene = sceneInfo,
            SessionName = sessionName,
            SceneManager = sceneManager,
            ObjectProvider = objectProvider
        });
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

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        bl_EventHandler.onPauseCall -= OnPause;
    }

    void OnPause(bool paused)
    {
        IsGamePaused = paused;
    }

    void OnSceneLoaded(Scene loadedScene, LoadSceneMode sceneLoadMode)
    {
        if (loadedScene.name == "Game")
        {
            SpawnBall();

            if (CurrentGameMode == GameMode.PvP)
            {
                SpawnPlayers();
            }
            else if (CurrentGameMode == GameMode.PvE)
            {
                SpawnPlayers(1);
                SpawnAI();
            }
            else if (CurrentGameMode == GameMode.EvE)
            {
                SpawnAI(2);
            }
        }
    }

    public void StartGame(GameMode mode, string maxPoint, AIDifficulty difficulty)
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

    void SpawnPlayers(int amount = 2)
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
            bl_EventHandler.DispatchGameFinish();
        }
        else if (Player2Points >= GameRequiredPoints)
        {
            GameUI.Instance.GameFinish.ShowFinish("Player 2", "Player 1", Player2Points, Player1Points);
            IsGameDone = true;
            bl_EventHandler.DispatchGameFinish();
        }
    }

    static GameController _instance;
    public static GameController Instance
    {
        get => _instance;
        private set => _instance = value;
    }
}
