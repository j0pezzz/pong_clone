using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    protected GameMode CurrentGameMode;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;
    }

    public void StartGame(GameMode mode)
    {
        CurrentGameMode = mode;
        SceneManager.LoadScene("Game");
    }

    static GameController _instance;
    public static GameController Instance
    {
        get => _instance;
        private set => _instance = value;
    }
}
