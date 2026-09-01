using AirlockAPI.Data;
using HarmonyLib;
using SG.Airlock;

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
