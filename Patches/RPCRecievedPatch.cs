using AirlockClient.AC;
using HarmonyLib;
using Il2CppFusion;

namespace AirlockClient.Patches
{
    [HarmonyPatch(typeof(NetworkRunner), nameof(NetworkRunner.OnMessageUser))]
    public class RPCRecievedPatch
    {
        public static unsafe void Prefix(NetworkRunner __instance, SimulationMessage* message)
        {
            RpcData.FromSimulationMessage(__instance, message);
        }
    }
}
