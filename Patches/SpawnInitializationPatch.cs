using AirlockClient.Attributes;
using AirlockClient.Data;
using AirlockClient.Managers;
using AirlockClient.Managers.Dev;
using AirlockClient.Managers.Gamemode;
using HarmonyLib;
using Il2CppSG.Airlock.Network;

namespace AirlockClient.Patches
{
    [HarmonyPatch(typeof(NetworkedLocomotionPlayer), nameof(NetworkedLocomotionPlayer.RPC_SpawnInitialization))]
    public class SpawnInitializationPatch
    {
        public static void Postfix(NetworkedLocomotionPlayer __instance)
        {
            if (CurrentMode.Modded)
            {
                if (CurrentMode.IsHosting)
                {
                    Listener.Send("JoinedModdedLobby", __instance.PState.PlayerId);

                    if (CurrentMode.Name == "Sandbox")
                    {
                        ((SandboxManager)ModdedGamemode.Current).playerDidSpawn = true;
                        __instance.PState.IsSpectating = false;
                        __instance.PState.IsAlive = true;
                    }
                    else
                    {
                        if (ModdedGameStateManager.Instance)
                        {
                            if (ModdedGameStateManager.Instance.state.InLobbyState())
                            {
                                ModdedGameStateManager.Instance.state.RPC_ToggleLobbyDoors(false);
                            }
                        }
                    }

                    CommandManager.Instance.CheckAuthorityForNameTag(__instance.PState);
                    CommandManager.Instance.requiresUpdate = true;
                }

                PetManager.Instance.AssignDebugPet(__instance);
            }
        }
    }
}
