using HarmonyLib;
using SG.LightUI;
using UnityEngine;

namespace AirlockClient.Patches
{
    [HarmonyPatch(typeof(LUIElement), nameof(LUIElement.SetHeight))]
    public class SetHeightPatch
    {
        public static void Prefix(LUIElement __instance, ref float height)
        {
            if (!__instance.name.Contains("Gamemode")) return;
            __instance._totalHeight = Mathf.Approximately(__instance._totalHeight, 30) ? 31 : 30;
            height = 125;
        }
    }
}
