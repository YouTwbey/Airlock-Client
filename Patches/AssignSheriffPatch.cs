using AirlockClient.Data;
using AirlockClient.Data.Roles.MoreRoles.Modifiers;
using AirlockClient.Managers;
using AirlockClient.Managers.Debug;
using HarmonyLib;
using Il2CppSG.Airlock;
using static UnityEngine.Object;

namespace AirlockClient.Patches
{
    [HarmonyPatch(typeof(VoteManager),nameof(VoteManager.ChooseSheriff))]
    public class AssignSheriffPatch
    {
        public static float originalSpeed = 0;
        public static void Postfix(VoteManager __instance) 
        {
            if (FindObjectOfType<DSpUp>().PlayerWithModifier.PlayerId == __instance.SheriffId)
            {
                originalSpeed = ModdedGameStateManager.Instance.GetRoleSetting(Enums.RoleFloatSettings.SheriffSpeedMultiplier).GetValue();
                Logging.Debug_Log($"Sheriff Original speed: {ModdedGameStateManager.Instance.GetRoleSetting(Enums.RoleFloatSettings.SheriffSpeedMultiplier).GetValue()}");
                ModdedGameStateManager.Instance.SetRoleSetting(Enums.RoleFloatSettings.SheriffSpeedMultiplier, ModdedGameStateManager.Instance.GetRoleSetting(Enums.RoleFloatSettings.SheriffSpeedMultiplier).GetValue() + 0.1f);
                Logging.Debug_Log($"Sheriff new speed: {ModdedGameStateManager.Instance.GetRoleSetting(Enums.RoleFloatSettings.SheriffSpeedMultiplier).GetValue()}");
            }
            Logging.Debug_Log($"PlayerID: {FindObjectOfType<DSpUp>().PlayerWithModifier.PlayerId}, DeputyID: {__instance.SheriffId}");
        }
    }
}
