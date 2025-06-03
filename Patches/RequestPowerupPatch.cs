using AirlockClient.Data;
using HarmonyLib;
using Il2CppSG.Airlock;

namespace AirlockClient.Patches
{
    [HarmonyPatch(typeof(PlayerState), nameof(PlayerState.ApplyRandomPowerUp))]
    public class RequestPowerupPatch
    {
        public static bool Prefix()
        {
            return CurrentMode.Name != "Hide N Seek";
        }
    }
}
