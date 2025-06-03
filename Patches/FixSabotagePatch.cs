using AirlockClient.Attributes;
using AirlockClient.Data;
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
                return ((HideNSeekManager)ModdedGamemode.Current).AllowSabotagesToBeTurnedOff;
            }

            if (CurrentMode.Name == "Cintainment")
            {
                ((ContainmentManager)ModdedGamemode.Current).OnRepairedSabotage();
            }

            return true;
        }
    }
}
