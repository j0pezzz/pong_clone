using UnityEngine;

namespace Project.Internal.Structures
{
    public struct SGameSettings
    {
        public GameModes GameMode;
        public int RequiredPoints;
        public AIDifficulty AIDifficulty;
        
        public string ToJson() => JsonUtility.ToJson(this);

        public static SGameSettings FromJson(string json) => JsonUtility.FromJson<SGameSettings>(json);
    }
}