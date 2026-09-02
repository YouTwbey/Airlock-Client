using AirlockAPI.Data;
using AirlockClient.AC;
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
            
            return AntiCheat.Instance.VerifyVent(__instance.PState);
        }
    }

    [HarmonyPatch(typeof(NetworkedLocomotionPlayer), nameof(NetworkedLocomotionPlayer.RPC_ExitVent))]
    public class ExitVentPatch
    {
        public static bool Prefix(NetworkedLocomotionPlayer __instance)
        {
            if (!CurrentMode.IsHosting || CurrentMode.Modded) return true;
            
            return AntiCheat.Instance.VerifyVent(__instance.PState);
        }
    }
}
