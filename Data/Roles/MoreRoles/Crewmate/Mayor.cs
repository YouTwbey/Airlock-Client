using AirlockClient.Attributes;
using AirlockClient.Managers.Gamemode;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Roles;
using MelonLoader;

namespace AirlockClient.Data.Roles.MoreRoles.Crewmate
{
    /// <summary>
    /// Crewmate Role
    /// When a player votes for someone with this role, it votes for them twice.
    /// </summary>
    public class Mayor : SubRole
    {
        public static SubRoleData Data = new SubRoleData
        {
            Name = "Mayor",
            Description = "Votes doubled",
            AC_Description = "During meetings, your votes are doubled.",
            Team = GameTeam.Crewmember,
            Amount = 1
        };

        void Start()
        {
            MelonCoroutines.Start(MoreRolesManager.DisplayRoleInfo(PlayerWithRole, this, Data));
        }

        PlayerState playerToVote;
        bool playerVotedSkip;

        public override void OnPlayerVoted(PlayerState votedPlayer)
        {
            playerToVote = votedPlayer;
        }

        public override void OnPlayerVotedSkip()
        {
            playerVotedSkip = true;
        }

        public override void OnAllVotesCast()
        {
            if (playerToVote != null)
            {
                Vote vote = new Vote();
                vote.Voter = PlayerWithRole.PlayerId;
                vote.VotedAgainst = new Il2CppSystem.Nullable<Il2CppFusion.PlayerRef>(playerToVote.PlayerId);

                FindObjectOfType<VoteManager>()._votes.Add(vote);
                playerToVote = null;
            }

            if (playerVotedSkip)
            {
                FindObjectOfType<VoteManager>()._skipVoting.Add(PlayerWithRole.PlayerId);
                playerVotedSkip = false;
            }
        }
    }
}
