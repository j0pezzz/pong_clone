using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Application = UnityEngine.Application;

[CreateAssetMenu(fileName = "GameData", menuName = "Scriptable Objects/GameData")]
public class GameData : ScriptableObject
{
     public AIGameSettings aiGameSettings;
    [SerializeField] private List<PlatformSettings> platforms;

    public static bool IsDataCached;
    static bool _isCaching;
    private PlatformSettings _currentPlatform;
    
    /// <summary>
    /// Gets the current platform settings.
    /// </summary>
    /// <returns>Either the cached settings, or fetches the current one.</returns>
    public PlatformSettings GetCurrentPlatform()
    {
        // We can safely cache this since the platform cannot change during runtime.
        if (_currentPlatform) return _currentPlatform;
        
#if UNITY_EDITOR_WIN
        _currentPlatform = platforms[0];
#elif UNITY_ANDROID
        _currentPlatform = platforms[1];
        #endif

        return _currentPlatform;
    }
    
    /// <summary>
    /// Cache the GameData from Resources asynchronous to avoid overhead and freeze the main thread the first time we access to the instance
    /// </summary>
    public static IEnumerator AsyncLoadData(Action loaded = null)
    {
        if (!_instance)
        {
            _isCaching = true;
            ResourceRequest rr = Resources.LoadAsync("GameData", typeof(GameData));
            yield return new WaitUntil(() => rr.isDone);

            _instance = rr.asset as GameData;
            _isCaching = false;
        }
        
        loaded?.Invoke();
        IsDataCached = true;
    }
    
    static GameData _instance;
    public static GameData Instance
    {
        get
        {
            if (_instance || _isCaching) return _instance;
            
            if (!IsDataCached && Application.isPlaying)
            {
                IsDataCached = true;
            }
            _instance = Resources.Load("GameData", typeof(GameData)) as GameData;
            return _instance;
        }
    }
}
