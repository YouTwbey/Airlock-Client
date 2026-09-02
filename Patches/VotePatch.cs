using AirlockAPI.Data;
using AirlockClient.AC;
using HarmonyLib;
using Fusion;
using SG.Airlock;
using UnityEngine;

namespace AirlockClient.Patches
{
    [HarmonyPatch()]
    public class VotePatch
    {
        [HarmonyPatch(typeof(VoteManager), nameof(VoteManager.RPC_Vote), new System.Type[] { typeof(PlayerRef), typeof(PlayerRef), typeof(RpcInfo) })]
        [HarmonyPrefix]
        public static bool Prefix1(PlayerRef voteAgainstPlayer, PlayerRef sourcePlayer, RpcInfo info)
        {
            var voter = GameObject.Find("PlayerState (" + voteAgainstPlayer.PlayerId + ")").GetComponent<PlayerState>();
            var voted = GameObject.Find("PlayerState (" + sourcePlayer.PlayerId + ")").GetComponent<PlayerState>();

            if (!CurrentMode.IsHosting || CurrentMode.Modded) return true;
            
            return AntiCheat.Instance.VerifyVote(voter, voted, info);
        }

        [HarmonyPatch(typeof(VoteManager), nameof(VoteManager.RPC_Vote), new System.Type[] { typeof(PlayerRef), typeof(RpcInfo) })]
        [HarmonyPrefix]
        public static bool Prefix2(PlayerRef sourcePlayer, RpcInfo info)
        {
            var voter = GameObject.Find("PlayerState (" + sourcePlayer.PlayerId + ")").GetComponent<PlayerState>();

            if (!CurrentMode.IsHosting || CurrentMode.Modded) return true;
            
            return AntiCheat.Instance.VerifyVote(voter, null, info);
        }
    }

    [HarmonyPatch()]
    public class CallVote
    {
        [HarmonyPatch(typeof(VoteManager), nameof(VoteManager.RPC_CallVote), new System.Type[] { typeof(int), typeof(PlayerRef), typeof(NetworkBool), typeof(RpcInfo) })]
        [HarmonyPrefix]
        public static bool Prefix1(int foundPlayer, PlayerRef sourcePlayer, NetworkBool forceVote, RpcInfo info)
        {
            var caller = GameObject.Find("PlayerState (" + sourcePlayer.PlayerId + ")").GetComponent<PlayerState>();
            var bodyFound = GameObject.Find("PlayerState (" + foundPlayer + ")").GetComponent<PlayerState>();

            if (!CurrentMode.IsHosting || CurrentMode.Modded) return true;
            return AntiCheat.Instance.VerifyBodyReport(caller, bodyFound, info);
        }

        [HarmonyPatch(typeof(VoteManager), nameof(VoteManager.RPC_CallVote), new System.Type[] { typeof(PlayerRef), typeof(NetworkBool), typeof(RpcInfo) })]
        [HarmonyPrefix]
        public static bool Prefix(PlayerRef sourcePlayer, NetworkBool forceVote, RpcInfo info)
        {
            var caller = GameObject.Find("PlayerState (" + sourcePlayer.PlayerId + ")").GetComponent<PlayerState>();

            if (!CurrentMode.IsHosting || CurrentMode.Modded) return true;
            
            return AntiCheat.Instance.VerifyMeeting(caller, info);
        }
    }
}
