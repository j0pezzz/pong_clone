using UnityEngine;

public static class StringUtility
{
    public static string GetTimeFormat(float m, float s) => $"{m:00}:{s:00}";

    public static string GetTimeFormat(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        return $"{minutes:00}:{seconds:00}";
    }
}
