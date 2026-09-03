using AirlockClient.Data.Roles.MoreRoles.Neutral;
using HarmonyLib;
using Il2CppSG.Airlock;

namespace AirlockClient.Patches
{
    [HarmonyPatch(typeof(PlayerState), nameof(PlayerState.MournSoulmate))]
    public class MournSoulmatePatch
    {
        public static bool Prefix(PlayerState __instance)
        {
            return __instance.GetComponent<Lover>() || __instance.GetComponent<OtherLover>();
        }
    }
}
