using AirlockClient.Attributes;
using HarmonyLib;
using Il2CppSG.Airlock;

namespace AirlockClient.Patches
{
    [HarmonyPatch(typeof(GameStateManager), nameof(GameStateManager.StartGame))]
    public class StartGamePatch
    {
        public static void Prefix(GameStateManager __instance)
        {
            if (ModdedGamemode.Current)
            {
                ModdedGamemode.Current.OnGameStart();
            }
        }
    }
}
