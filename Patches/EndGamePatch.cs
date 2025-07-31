using AirlockClient.Attributes;
using HarmonyLib;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Roles;

namespace AirlockClient.Patches
{
    [HarmonyPatch(typeof(GameStateManager), nameof(GameStateManager.EndGame))]
    public class EndGamePatch
    {
        public static void Prefix(GameStateManager __instance, GameTeam winningTeam)
        {
            foreach (SubRole role in SubRole.All)
            {
                role.OnGameEnd(winningTeam);
            }
        }
    }
}
