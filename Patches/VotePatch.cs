using AirlockClient.Attributes;
using Il2CppFusion;
using Il2CppSG.Airlock;
using HarmonyLib;
using UnityEngine;
using AirlockClient.Data;
using AirlockClient.Managers.Gamemode;
using AirlockClient.AC;

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

            if (ModdedGamemode.Current)
            {
                ModdedGamemode.Current.OnPlayerVoted(voter, voted);
            }

            if (CurrentMode.Name == "Round Up")
            {
                if (sourcePlayer != ((RoundUpManager)ModdedGamemode.Current).deputy.PlayerId)
                {
                    return false;
                }
                else
                {
                    ((RoundUpManager)ModdedGamemode.Current).OnDeputyVote(voted);
                }
            }

            foreach (SubRole role in SubRole.All)
            {
                if (role.PlayerWithRole.PlayerId == sourcePlayer.PlayerId)
                {
                    role.OnPlayerVoted(voted);
                }
            }

            return true;
        }

        [HarmonyPatch(typeof(VoteManager), nameof(VoteManager.RPC_Vote), new System.Type[] { typeof(PlayerRef), typeof(RpcInfo) })]
        [HarmonyPrefix]
        public static bool Prefix2(PlayerRef sourcePlayer, RpcInfo info)
        {
            if (ModdedGamemode.Current)
            {
                ModdedGamemode.Current.OnPlayerVotedSkip(GameObject.Find("PlayerState (" + sourcePlayer.PlayerId + ")").GetComponent<PlayerState>());
            }

            if (CurrentMode.Name == "Round Up")
            {
                if (sourcePlayer != ((RoundUpManager)ModdedGamemode.Current).deputy.PlayerId)
                {
                    return false;
                }
                else
                {
                    ((RoundUpManager)ModdedGamemode.Current).OnDeputyVote(GameObject.Find("PlayerState (" + sourcePlayer.PlayerId + ")").GetComponent<PlayerState>());
                }
            }

            foreach (SubRole role in SubRole.All)
            {
                if (role.PlayerWithRole.PlayerId == sourcePlayer.PlayerId)
                {
                    role.OnPlayerVotedSkip();
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
                if (!AntiCheat.Instance.VerifyBodyReport(caller, bodyFound))
                {
                    return false;
                }
            }

            if (ModdedGamemode.Current)
            {
                ModdedGamemode.Current.OnVotingBegan(caller, bodyFound);
            }

            foreach (SubRole role in SubRole.All)
            {
                role.OnVotingBegan(null, null);

                if (role.PlayerWithRole.PlayerId == sourcePlayer.PlayerId)
                {
                    role.OnPlayerReportedBody(bodyFound);
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
                if (!AntiCheat.Instance.VerifyMeeting(caller))
                {
                    return false;
                }
            }

            if (ModdedGamemode.Current)
            {
                ModdedGamemode.Current.OnVotingBegan(null, caller);
            }

            foreach (SubRole role in SubRole.All)
            {
                role.OnVotingBegan(null, null);

                if (role.PlayerWithRole.PlayerId == sourcePlayer.PlayerId)
                {
                    role.OnPlayerCalledMeeting();
                }
            }

            return true;
        }
    }
}
