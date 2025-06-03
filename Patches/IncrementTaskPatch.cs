using AirlockClient.Attributes;
using AirlockClient.Data;
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
                ModdedGamemode.Current.State._gamemodeTimerCurrent -= 5;
            }
        }
    }
}
