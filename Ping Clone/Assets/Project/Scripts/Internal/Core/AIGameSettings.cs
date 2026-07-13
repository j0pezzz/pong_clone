using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AI Game Settings", menuName = "Scriptable Objects/Create New AI Game Settings")]
public class AIGameSettings : ScriptableObject
{
    public List<AISetting> aiSettings;

    public AISetting GetAISettings(AIDifficulty difficulty)
    {
        return aiSettings.Find(x => x.difficulty == difficulty);
    }
}

[Serializable]
public struct AISetting
{
    public AIDifficulty difficulty;
    public float maxTrackingOffset;
}
