using UnityEngine;

public class SpawnPointManager : MonoBehaviour
{
    //TODO: Need to make spawnpoint implementation better.
    public Vector3 SpawnPoint1;
    public Vector3 SpawnPoint2;

    void Awake()
    {
        Instance = this;
    }

    static SpawnPointManager _instance;
    public static SpawnPointManager Instance
    {
        get => _instance;
        private set => _instance = value;
    }
}
