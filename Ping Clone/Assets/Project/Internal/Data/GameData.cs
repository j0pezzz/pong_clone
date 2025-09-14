using Fusion;
using UnityEngine;

[CreateAssetMenu(menuName = "Internal/Create Game Data", fileName = "GameData")]
public class GameData : ScriptableObject
{
    public NetworkRunner runnerPrefab;
    
    private static GameData _instance;

    public static GameData Instance
    {
        get
        {
            if (!_instance) _instance = Resources.Load<GameData>("GameData");
            return _instance;
        }
    }
}
