using Il2CppSG.Airlock.Roles;
using HarmonyLib;
using AirlockClient.Attributes;

namespace AirlockClient.Patches
{
    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.AssignRoles))]
    public class AssignRolesPatch
    {
        public static void Postfix(RoleManager __instance)
        {
            if (ModdedGamemode.Current)
            {
                ModdedGamemode.Current.OnAssignRoles();
            }
        }
    }
}
