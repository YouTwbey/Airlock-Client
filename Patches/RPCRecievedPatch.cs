using AirlockClient.AC;
using HarmonyLib;
using Il2CppFusion;

namespace AirlockClient.Patches
{
    [HarmonyPatch(typeof(NetworkRunner), nameof(NetworkRunner.OnMessageUser))]
    public class RpcRecievedPatch
    {
        public static unsafe void Prefix(NetworkRunner __instance, SimulationMessage* message)
        {
            RpcData.FromMessage(__instance, message);
        }
    }
}
