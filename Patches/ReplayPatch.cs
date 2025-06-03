using AirlockClient.Data;
using AirlockClient.Managers;
using HarmonyLib;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Cutscenes;

namespace AirlockClient.Patches
{
    [HarmonyPatch(typeof(CutsceneManager), nameof(CutsceneManager.RPC_ReplayGame))]
    public class ReplayPatch
    {
        public static void Prefix()
        {
            if (CurrentMode.Modded)
            {
                if (CurrentMode.IsHosting)
                {
                    if (ModdedGameStateManager.Instance)
                    {
                        if (ModdedGameStateManager.Instance.state.InLobbyState())
                        {
                            ModdedGameStateManager.Instance.state.RPC_ToggleLobbyDoors(false);
                        }
                    }
                }
            }
        }
    }
}
