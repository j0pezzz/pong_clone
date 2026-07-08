using Fusion;
using UnityEngine;

namespace Project.Internal.Utility
{
    public static class FusionTimerUtility
    {
        public static int GetSecondsInt(this TickTimer tickTimer, NetworkRunner runner)
        {
            int? remainingTicks = tickTimer.RemainingTicks(runner);

            if (!remainingTicks.HasValue) return 0;
        
            float remainingSeconds = remainingTicks.Value / (float)runner.TickRate;
        
            int secondsLeft = Mathf.CeilToInt(remainingSeconds);
            return secondsLeft;
        }

        public static float GetSecondsFloat(this TickTimer tickTimer, NetworkRunner runner)
        {
            int? remainingTicks = tickTimer.RemainingTicks(runner);

            if (!remainingTicks.HasValue) return 0;
        
            float remainingSeconds = remainingTicks.Value / (float)runner.TickRate;
        
            return remainingSeconds;
        }
    }
}