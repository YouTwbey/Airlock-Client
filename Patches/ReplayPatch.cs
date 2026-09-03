using AirlockAPI.Data;
using AirlockClient.Managers;
using HarmonyLib;
using SG.Airlock.Cutscenes;

namespace AirlockClient.Patches
{
    [HarmonyPatch(typeof(CutsceneManager), nameof(CutsceneManager.RPC_ReplayGame))]
    public class ReplayPatch
    {
        public static void Prefix()
        {
            if (!CurrentMode.Modded) return;
            if (!CurrentMode.IsHosting) return;
            if (!ModdedGameStateManager.Instance) return;
            if (ModdedGameStateManager.Instance.state.InLobbyState())
            {
                ModdedGameStateManager.Instance.state.RPC_ToggleLobbyDoors(false);
            }
        }
    }
}
