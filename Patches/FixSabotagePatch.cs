using AirlockClient.Attributes;
using AirlockAPI.Data;
using AirlockClient.Managers.Gamemode;
using HarmonyLib;
using Il2CppSG.Airlock.Sabotage;

namespace AirlockClient.Patches
{
    [HarmonyPatch(typeof(SabotageManager), nameof(SabotageManager.RPC_EndSabotage))]
    public class FixSabotagePatch
    {
        public static bool Prefix()
        {
            if (CurrentMode.Name == "Hide N Seek")
            {
                return ((HideNSeekManager)AirlockClientGamemode.Current).AllowSabotagesToBeTurnedOff;
            }

            if (CurrentMode.Name == "Containment")
            {
                ((ContainmentManager)AirlockClientGamemode.Current).OnRepairedSabotage();
            }

            return true;
        }
    }
}
