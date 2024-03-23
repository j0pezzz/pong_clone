using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public float TopBound = 4f;
    public float BottomBound = -4f;
    public bool IsGameDone { get; set; } = false;
    public bool IsGamePaused { get; set; } = false;

    public int Player1Points { get; private set; } = 0;
    public int Player2Points { get; private set; } = 0;
    public int MaxPoint { get; private set; } = 5;

    public GameObject PlayerController;
    public GameObject BallPrefab;

    protected GameMode CurrentGameMode;

    PlayerControlller cacheP1;
    PlayerControlller cacheP2;
    Ball cacheBall;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;

        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        bl_EventHandler.onPauseCall += OnGamePaused;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
        bl_EventHandler.onPauseCall -= OnGamePaused;
    }

    void OnGamePaused(bool paused)
    {
        IsGamePaused = paused;
    }

    void SceneManager_sceneLoaded(Scene loadedScene, LoadSceneMode sceneLoadMode)
    {
        if (loadedScene.name == "Game")
        {
            SpawnPlayer();
            SpawnBall();
        }
    }

    public void StartGame(GameMode mode)
    {
        CurrentGameMode = mode;
        SceneManager.LoadScene("Game");
    }

    public void ResetGame()
    {
        cacheP1.SetPlayerToInitPosition();
        cacheP2.SetPlayerToInitPosition();
        cacheBall.SetBallToInit();
    }

    void SpawnPlayer()
    {
        GameObject playerObject = Instantiate(PlayerController, SpawnPointManager.Instance.SpawnPoint1, Quaternion.identity);
        playerObject.name = "Player 1";
        playerObject.tag = "Paddle1";
        if (playerObject.TryGetComponent(out cacheP1))
        {
            cacheP1.PlayerRef = 1;
        }

        GameObject playerObject2 = Instantiate(PlayerController, SpawnPointManager.Instance.SpawnPoint2, Quaternion.identity);
        playerObject2.name = "Player 2";
        playerObject2.tag = "Paddle2";
        if (playerObject2.TryGetComponent(out cacheP2))
        {
            cacheP2.PlayerRef = 2;
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

        ScoreUI.Instance.UpdateScores(Player1Points, Player2Points);
    }

    static GameController _instance;
    public static GameController Instance
    {
        get => _instance;
        private set => _instance = value;
    }
}
