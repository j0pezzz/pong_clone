public static class StringUtility
{
    public static string GetTimeFormat(int seconds) => string.Format("{0:00}", seconds / 60);
}
