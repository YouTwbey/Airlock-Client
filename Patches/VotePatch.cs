using AirlockAPI.Data;
using AirlockClient.AC;
using HarmonyLib;
using Il2CppFusion;
using Il2CppSG.Airlock;
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
            PlayerState voter = GameObject.Find("PlayerState (" + voteAgainstPlayer.PlayerId + ")").GetComponent<PlayerState>();
            PlayerState voted = GameObject.Find("PlayerState (" + sourcePlayer.PlayerId + ")").GetComponent<PlayerState>();

            if (CurrentMode.IsHosting && !CurrentMode.Modded)
            {
                if (!AntiCheat.Instance.VerifyVote(voter, voted, info))
                {
                    return false;
                }
            }

            return true;
        }

        [HarmonyPatch(typeof(VoteManager), nameof(VoteManager.RPC_Vote), new System.Type[] { typeof(PlayerRef), typeof(RpcInfo) })]
        [HarmonyPrefix]
        public static bool Prefix2(PlayerRef sourcePlayer, RpcInfo info)
        {
            PlayerState voter = GameObject.Find("PlayerState (" + sourcePlayer.PlayerId + ")").GetComponent<PlayerState>();

            if (CurrentMode.IsHosting && !CurrentMode.Modded)
            {
                if (!AntiCheat.Instance.VerifyVote(voter, null, info))
                {
                    return false;
                }
            }

            return true;
        }
    }

    [HarmonyPatch()]
    public class CallVote
    {
        [HarmonyPatch(typeof(VoteManager), nameof(VoteManager.RPC_CallVote), new System.Type[] { typeof(int), typeof(PlayerRef), typeof(NetworkBool), typeof(RpcInfo) })]
        [HarmonyPrefix]
        public static bool Prefix1(int foundPlayer, PlayerRef sourcePlayer, NetworkBool forceVote, RpcInfo info)
        {
            PlayerState caller = GameObject.Find("PlayerState (" + sourcePlayer.PlayerId + ")").GetComponent<PlayerState>();
            PlayerState bodyFound = GameObject.Find("PlayerState (" + foundPlayer + ")").GetComponent<PlayerState>();

            if (CurrentMode.IsHosting && !CurrentMode.Modded)
            {
                if (!AntiCheat.Instance.VerifyBodyReport(caller, bodyFound, info))
                {
                    return false;
                }
            }

            return true;
        }

        [HarmonyPatch(typeof(VoteManager), nameof(VoteManager.RPC_CallVote), new System.Type[] { typeof(PlayerRef), typeof(NetworkBool), typeof(RpcInfo) })]
        [HarmonyPrefix]
        public static bool Prefix(PlayerRef sourcePlayer, NetworkBool forceVote, RpcInfo info)
        {
            PlayerState caller = GameObject.Find("PlayerState (" + sourcePlayer.PlayerId + ")").GetComponent<PlayerState>();

            if (CurrentMode.IsHosting && !CurrentMode.Modded)
            {
                if (!AntiCheat.Instance.VerifyMeeting(caller, info))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
