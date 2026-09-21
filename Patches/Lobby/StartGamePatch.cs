using AirlockAPI.Data;
using AirlockClient.Managers;
using Fusion;
using HarmonyLib;

namespace AirlockClient.Patches
{
    [HarmonyPatch(typeof(NetworkRunner), nameof(NetworkRunner.StartGame))]
    public class StartGamePatch
    {
        public static void Prefix(NetworkRunner __instance, ref StartGameArgs args)
        {
            if (!CurrentMode.IsHosting || !CurrentMode.Modded) return;

            AirlockClientManager.SendGameState(CurrentMode.IsHosting && CurrentMode.Modded, CurrentMode.Name);
        }
    }
}
