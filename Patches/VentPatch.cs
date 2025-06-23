using AirlockAPI.Data;
using AirlockClient.AC;
using HarmonyLib;
using Il2CppSG.Airlock.Network;

namespace AirlockClient.Patches
{
    [HarmonyPatch(typeof(NetworkedLocomotionPlayer), nameof(NetworkedLocomotionPlayer.RPC_EnterVent))]
    public class EnterVentPatch
    {
        public static bool Prefix(NetworkedLocomotionPlayer __instance)
        {
            if (CurrentMode.IsHosting && !CurrentMode.Modded)
            {
                if (!AntiCheat.Instance.VerifyVent(__instance.PState, RpcData.Info))
                {
                    return false;
                }
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(NetworkedLocomotionPlayer), nameof(NetworkedLocomotionPlayer.RPC_ExitVent))]
    public class ExitVentPatch
    {
        public static bool Prefix(NetworkedLocomotionPlayer __instance)
        {
            if (CurrentMode.IsHosting && !CurrentMode.Modded)
            {
                if (!AntiCheat.Instance.VerifyVent(__instance.PState, RpcData.Info))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
