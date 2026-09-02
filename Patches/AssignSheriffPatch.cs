using System;
using AirlockAPI.Data;
using AirlockClient.Data;
using AirlockClient.Data.Roles.MoreRoles.Modifiers;
using AirlockClient.Managers;
using AirlockClient.Managers.Debug;
using HarmonyLib;
using SG.Airlock;
using static UnityEngine.Object;

namespace AirlockClient.Patches
{
    //TODO: Check this i think it causes an error
    [HarmonyPatch(typeof(VoteManager),nameof(VoteManager.ChooseSheriff))]
    public class AssignSheriffPatch
    {
        public static float originalSpeed = 0;
        
        public static void Postfix(VoteManager __instance)
        {
            try
            {
                if (!CurrentMode.IsHosting || !CurrentMode.Modded || CurrentMode.Name != "More Roles") return;

                if (FindObjectOfType<DSpUp>().PlayerWithModifier.PlayerId == __instance.SheriffId)
                {
                    originalSpeed = ModdedGameStateManager.Instance
                        .GetRoleSetting(Enums.RoleFloatSettings.SheriffSpeedMultiplier).GetValue();
                    Logging.Debug_Log(
                        $"Sheriff Original speed: {ModdedGameStateManager.Instance.GetRoleSetting(Enums.RoleFloatSettings.SheriffSpeedMultiplier).GetValue()}");
                    ModdedGameStateManager.Instance.SetRoleSetting(Enums.RoleFloatSettings.SheriffSpeedMultiplier,
                        ModdedGameStateManager.Instance.GetRoleSetting(Enums.RoleFloatSettings.SheriffSpeedMultiplier)
                            .GetValue() + 0.1f);
                    Logging.Debug_Log(
                        $"Sheriff new speed: {ModdedGameStateManager.Instance.GetRoleSetting(Enums.RoleFloatSettings.SheriffSpeedMultiplier).GetValue()}");
                }

                Logging.Debug_Log(
                    $"PlayerID: {FindObjectOfType<DSpUp>().PlayerWithModifier.PlayerId}, DeputyID: {__instance.SheriffId}");
            }
            catch (Exception e)
            {
                Logging.Error(e.ToString());
            }
        }
    }
}
