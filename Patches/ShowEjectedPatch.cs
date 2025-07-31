using Il2CppFusion;
using Il2CppSG.Airlock.Cutscenes;
using Il2CppSG.Airlock.Roles;
using HarmonyLib;
using AirlockClient.Attributes;
using UnityEngine;
using Il2CppSG.Airlock;
using AirlockClient.Data.Roles.MoreRoles.Imposter;
using AirlockClient.Managers.Debug;

namespace AirlockClient.Patches
{
    [HarmonyPatch(typeof(CutsceneManager), nameof(CutsceneManager.ShowEjected))]
    public class ShowEjectedPatch1
    {
        public static void Prefix(CutsceneManager __instance, PlayerRef playerEjected, GameRole playerEjectedRole, bool onlyShowImpostors, int aliveImposters, int aliveCrewmates)
        {
            PlayerState ejected = GameObject.Find("PlayerState (" + playerEjected.PlayerId + ")").GetComponent<PlayerState>();
            Logging.Debug_Log("Player Ejected: " + ejected.NetworkName.Value);

            foreach (SubRole role in SubRole.All)
            {
                role.OnPlayerEjected(ejected, playerEjectedRole);
            }
        }
    }

    [HarmonyPatch(typeof(CutsceneManager), nameof(CutsceneManager.ShowAnonymousEjected))]
    public class ShowEjectedPatch2
    {
        public static void Prefix(CutsceneManager __instance, PlayerRef playerEjected, int aliveImposters, int aliveCrewmates)
        {
            PlayerState ejected = GameObject.Find("PlayerState (" + playerEjected.PlayerId + ")").GetComponent<PlayerState>();
            Logging.Debug_Log("Player Ejected: " + ejected.NetworkName.Value);

            foreach (SubRole role in SubRole.All)
            {
                role.OnPlayerEjected(ejected, GameRole.NotSet);
            }
        }
    }

    [HarmonyPatch(typeof(CutsceneManager), nameof(CutsceneManager.ShowNoEjected))]
    public class ShowEjectedPatch3
    {
        public static void Prefix(CutsceneManager __instance, int aliveImposters)
        {
            foreach (SubRole role in SubRole.All)
            {
                role.OnPlayerEjected(null, GameRole.NotSet);
            }
        }
    }

    [HarmonyPatch(typeof(CutsceneManager), nameof(CutsceneManager.ShowDebugEjected))]
    public class ShowEjectedPatch4
    {
        public static void Prefix(CutsceneManager __instance, PlayerRef playerEjected, bool wasImposter, int aliveImposters, int cutsceneIndex)
        {
            PlayerState ejected = GameObject.Find("PlayerState (" + playerEjected.PlayerId + ")").GetComponent<PlayerState>();
            Logging.Debug_Log("Player Ejected: " + ejected.NetworkName.Value);

            foreach (SubRole role in SubRole.All)
            {
                role.OnPlayerEjected(ejected, GameRole.NotSet);
            }
        }
    }

    [HarmonyPatch(typeof(VoteManager), nameof(VoteManager.OnEndVoteComplete))]
    public class WitchKillingPatch
    {
        public static void Prefix(VoteManager __instance)
        {
            foreach (SubRole role in SubRole.All)
            {
                if (role.GetComponent<Witch>())
                {
                    role.OnPlayerEjected(null, GameRole.NotSet);
                }
            }
        }
    }
}
