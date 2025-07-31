using AirlockClient.Attributes;
using HarmonyLib;
using Il2CppSG.Airlock;

namespace AirlockClient.Patches
{
    [HarmonyPatch(typeof(VoteManager), nameof(VoteManager.EndVote))]
    public class EndVotePatch
    {
        public static void Prefix(VoteManager __instance)
        {
            foreach (SubRole role in SubRole.All)
            {
                role.OnAllVotesCast();
            }
        }
    }
}
