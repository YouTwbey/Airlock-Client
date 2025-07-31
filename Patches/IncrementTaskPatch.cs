using AirlockAPI.Data;
using AirlockClient.Attributes;
using HarmonyLib;
using Il2CppSG.Airlock;

namespace AirlockClient.Patches
{
    [HarmonyPatch(typeof(MinigamePlayer), nameof(MinigamePlayer.RPC_IncrementCompletedTasks))]
    public class IncrementTaskPatch
    {
        public static void Prefix()
        {
            if (CurrentMode.Name == "Hide N Seek")
            {
                AirlockClientGamemode.Current.State._gamemodeTimerCurrent -= 5;
            }
        }
    }
}
