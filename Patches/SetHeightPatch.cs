using HarmonyLib;
using Il2CppSG.LightUI;

namespace AirlockClient.Patches
{
    [HarmonyPatch(typeof(LUIElement), nameof(LUIElement.SetHeight))]
    public class SetHeightPatch
    {
        public static void Prefix(LUIElement __instance, ref float height)
        {
            if (__instance.name.Contains("Gamemode"))
            {
                if (__instance._totalHeight == 30)
                {
                    __instance._totalHeight = 31;
                }
                else
                {
                    __instance._totalHeight = 30;
                }
                height = 125;
            }
        }
    }
}
