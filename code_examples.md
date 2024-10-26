## Below is the Player Controller, which manages player movement and input handling using Photon Fusion.

```csharp
using Fusion;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [Networked] public NetworkButtons ButtonsPrevious { get; set; }
    [Range(1, 5)] public float Speed = 5f;

    public int PlayerRef = 1;
    Transform m_Transform;
    Vector3 initPosition;

    public override void Spawned()
    {
        Debug.LogWarning("We are spawned");
        bl_EventHandler.Match.DispatchInMatchStatus(true);
        initPosition = transform.position;
        m_Transform = transform;
    }

    public override void FixedUpdateNetwork()
    {
        if (GameTimer.Instance.IsGameDone > 0) return;
        if (GameTimer.Instance.IsGamePaused > 0) return;

        if (GetInput(out NetworkInputData data))
        {
            NetworkButtons pressed = data.Buttons.GetPressed(ButtonsPrevious);
            NetworkButtons released = data.Buttons.GetReleased(ButtonsPrevious);

            ButtonsPrevious = data.Buttons;

            float yDir = data.Buttons.IsSet(Buttons.Up) ? 1 : data.Buttons.IsSet(Buttons.Down) ? -1 : 0;
            float newY = Mathf.Clamp(transform.position.y + (yDir * Speed) * Runner.DeltaTime, GameController.Instance.BottomBound, GameController.Instance.TopBound);
            m_Transform.position = new(m_Transform.position.x, newY, m_Transform.position.z);
        }
    }

    public void SetPlayerToInitPosition()
    {
        m_Transform.position = initPosition;
    }
}
```
## Below is the Game Controller code to handle creating & joining a room, spawning, etc.

```csharp
public class GameController : SimulationBehaviour, INetworkRunnerCallbacks
{
    public GameObject PlayerController;
    public GameObject AIPrefab;
    public NetworkRunner RunnerPrefab;
    [ScenePath]
    public string InitialScenePath;

    public static int GameRequiredPoints { get; private set; } = 5;

    private NetworkRunner _server;
    private Dictionary<PlayerRef, NetworkObject> _spawnedPlayers = new();

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;
    }

    public void StartRunner()
    {
        _server = Instantiate(RunnerPrefab);
        DontDestroyOnLoad(_server);
    }

    public IEnumerator HostRoom(string points)
    {
        int.TryParse(points, out int requiredPoints);
        string sessionName = UnityEngine.Random.Range(0, 99999).ToString();

        _server = Instantiate(RunnerPrefab);
        Task serverTask = InitializeRunner(_server, Fusion.GameMode.Host, NetAddress.Any(), SceneRef.FromIndex(SceneUtility.GetBuildIndexByScenePath(InitialScenePath)), sessionName);

        while (!serverTask.IsCompleted) yield return null;

        if (serverTask.IsFaulted)
        {
            Debug.LogError($"Failed to host room: {serverTask.Exception}");
            yield break;
        }

        GameRequiredPoints = requiredPoints;
    }

    protected async Task InitializeRunner(NetworkRunner runner, Fusion.GameMode gameMode, NetAddress address, SceneRef sceneRef, string sessionName)
    {
        runner.AddCallbacks(this);
        StartGameResult result = await runner.StartGame(new StartGameArgs
        {
            GameMode = gameMode,
            Address = address,
            Scene = sceneRef,
            SessionName = sessionName,
        });

        if (!result.Ok)
        {
            Debug.LogError($"Initialization failed: {result.ShutdownReason}");
            ShutdownAll();
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        NetworkObject spawnedPlayer = SpawnPlayerOnline(player);
        runner.SetPlayerObject(player, spawnedPlayer);
        _spawnedPlayers.Add(player, spawnedPlayer);
    }

    NetworkObject SpawnPlayerOnline(PlayerRef playerRef)
    {
        Vector3 spawnPoint = playerRef.PlayerId == 1 ? SpawnPointManager.Instance.SpawnPoint1 : SpawnPointManager.Instance.SpawnPoint2;
        return _server.Spawn(PlayerController, spawnPoint, Quaternion.identity, playerRef);
    }

    public void ShutdownAll()
    {
        foreach (NetworkRunner runner in NetworkRunner.Instances)
        {
            if (runner.IsRunning)
            {
                runner.Shutdown();
            }
        }
        Destroy(RunnerPrefab.gameObject);
    }
}
