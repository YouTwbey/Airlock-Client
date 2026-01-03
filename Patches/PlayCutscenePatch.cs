using AirlockAPI.Data;
using AirlockClient.Managers.Gamemode;
using HarmonyLib;
using Il2CppSG.Airlock.Cutscenes;
using Il2CppTMPro;
using Il2CppSG.Airlock.Localization;

namespace AirlockClient.Patches
{
    [HarmonyPatch(typeof(CutsceneSetup), nameof(CutsceneSetup.Play))]
    public class PlayCutscenePatch
    {
        public static void Prefix(CutsceneSetup __instance)
        {
            if (__instance.name.Contains("_Role"))
            {
                TextMeshPro yourRole = __instance._hideUntilPlay.transform.Find("TMP_YourRole").GetComponent<TextMeshPro>();
                TextMeshPro role = __instance._hideUntilPlay.transform.Find("TMP_Role").GetComponent<TextMeshPro>();
                TextMeshPro desc = __instance._hideUntilPlay.transform.Find("TMP_Desc").GetComponent<TextMeshPro>();

                if (MoreRolesManager.MyRole != null)
                {
                    yourRole.GetComponent<UserStringComponent_TMP>().enabled = false;
                    role.GetComponent<UserStringComponent_TMP>().enabled = false;
                    desc.GetComponent<UserStringComponent_TMP>().enabled = false;

                    if (CurrentMode.Name == "More Roles")
                    {
                        MoreRolesManager.OnRoleReveal(yourRole, role, desc);
                    }
                }
                else
                {
                    yourRole.GetComponent<UserStringComponent_TMP>().enabled = true;
                    role.GetComponent<UserStringComponent_TMP>().enabled = true;
                    desc.GetComponent<UserStringComponent_TMP>().enabled = true;
                }
            }
        }
    }
}
