using Fusion;
using Project.Internal.Structures;

namespace Project.Internal.Utility
{
    public static class FusionNetworkRunnerHelper
    {
        public static SGameSettings GetGameSettings(this SessionInfo sessionInfo)
        {
            string settingsJson = sessionInfo.Properties["Settings"];

            if (string.IsNullOrEmpty(settingsJson))
            {
                Log.Error("SettingsJson is empty, is SGameSettings actually put in to SessionProperties?");
                return default;
            }
            
            SGameSettings gameSettings = SGameSettings.FromJson(settingsJson);
            return gameSettings;
        }
    }
}