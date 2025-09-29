using AirlockAPI.Data;
using AirlockClient.AC;
using AirlockClient.Attributes;
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
        public static void Postfix(NetworkedLocomotionPlayer __instance, int color, int hat, int hands, int skin, string name, string moderationID, string moderationUsername, string accountID, bool is3D)
        {
            if (AntiCheat.Instance != null)
            {
                AntiCheat.Instance.VerifyJoin(__instance, color, hat, hands, skin, name, moderationID, moderationUsername, accountID, is3D);
            }

            if (CurrentMode.Modded)
            {
                if (CurrentMode.IsHosting)
                {
                    if (__instance.PState.PlayerId != 9) ModdedGameStateManager.RPC_JoinedModdedGame(__instance.PState.PlayerId); 

                    if (CurrentMode.Name == "Sandbox")
                    {
                        ((SandboxManager)AirlockClientGamemode.Current).playerDidSpawn = true;
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

                    if (CommandManager.Instance)
                    {
                        CommandManager.Instance.CheckAuthorityForNameTag(__instance.PState);
                        CommandManager.Instance.requiresUpdate = true;
                    }
                }

                //if (PetManager.Instance) PetManager.Instance.AssignDebugPet(__instance);
            }
        }
    }
}
