using AirlockAPI.Data;
using AirlockAPI.Handlers;
using AirlockClient.Managers.Gamemode;
using HarmonyLib;
using SG.Airlock.Sabotage;

namespace AirlockClient.Patches
{
    [HarmonyPatch(typeof(SabotageManager), nameof(SabotageManager.RPC_EndSabotage))]
    public class FixSabotagePatch
    {
        public static bool Prefix()
        {
            if (CurrentMode.Name == "Hide N Seek")
            {
                return ((HideNSeekManager)CustomGameHandler.Current).AllowSabotagesToBeTurnedOff;
            }

            if (CurrentMode.Name == "Containment")
            {
                ((ContainmentManager)CustomGameHandler.Current).OnRepairedSabotage();
            }

            return true;
        }
    }
}
