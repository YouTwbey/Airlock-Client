using AirlockClient.Attributes;
using AirlockClient.Data.Roles.MoreRoles.Modifiers;
using AirlockClient.Managers.Debug;
using AirlockClient.Utils;
using HarmonyLib;
using SG.Airlock;
using UnityEngine;

namespace AirlockClient.Patches;

[HarmonyPatch(typeof(PlayerState),nameof(PlayerState.MournSoulmate))]
public class MournSoulMate
{
    public static bool Prefix(PlayerState __instance, bool isAlive)
    {
        foreach (var modifier in Modifier.All)
        {
            if (modifier == (Lover.Instance || OtherLover.Instance) && modifier.PlayerWithModifier == __instance)
            {
                Logging.Log($"{__instance.NetworkName.Value} was lover and mourned soulmate");
                return true;
            }
            
            Logging.Log($"{__instance.NetworkName.Value} wasnt lover and didnt mourn soulmate");
        }
        
        return false;
    }
}