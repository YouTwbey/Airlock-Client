using AirlockAPI.Data;
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
            var modded = new SessionProperty
            {
                _value = true
            };
            args.SessionProperties.Add("modded", modded);
        }
    }
}
