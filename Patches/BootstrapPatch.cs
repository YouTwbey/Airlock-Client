using AirlockClient.Core;
using HarmonyLib;
using SG.Airlock;

namespace AirlockClient.Patches
{
    [HarmonyPatch(typeof(AirlockBootstrap), nameof(AirlockBootstrap.Main))]
    public class BootstrapPatch
    {
        public static void Prefix(AirlockBootstrap __instance)
        {
            Base.OnInit();
        }
    }
}
