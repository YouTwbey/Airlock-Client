using AirlockAPI.Data;
using AirlockClient.AC;
using AirlockClient.Utils;
using HarmonyLib;
using SG.Airlock.Network;

namespace AirlockClient.Patches
{
    [HarmonyPatch(typeof(NetworkedLocomotionPlayer), nameof(NetworkedLocomotionPlayer.RPC_EnterVent))]
    public class EnterVentPatch
    {
        public static bool Prefix(NetworkedLocomotionPlayer __instance)
        {
            if (!CurrentMode.IsHosting || CurrentMode.Modded) return true;
            
            return __instance.PState.VerifyVent();
        }
    }

    [HarmonyPatch(typeof(NetworkedLocomotionPlayer), nameof(NetworkedLocomotionPlayer.RPC_ExitVent))]
    public class ExitVentPatch
    {
        public static bool Prefix(NetworkedLocomotionPlayer __instance)
        {
            if (!CurrentMode.IsHosting || CurrentMode.Modded) return true;
            
            return __instance.PState.VerifyVent();
        }
    }
}
