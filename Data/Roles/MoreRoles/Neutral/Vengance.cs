using AirlockClient.Attributes;
using AirlockClient.Managers.Gamemode;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Roles;
using MelonLoader;
using static AirlockClient.AC.AntiCheat;

namespace AirlockClient.Data.Roles.MoreRoles.Neutral
{
    /// <summary>
    ///  If the Vengeance gets voted out the person they voted gets ejected/killed as well.
    /// </summary>
    public class Vengance : SubRole
    {
        public PlayerState PlayerWithRoleVoted;
        public bool PlayerWithRoleSkipped = false;
        public VoteManager voteManager;

        public static SubRoleData Data = new SubRoleData
        {
            Name = "Vengance",
            RoleType = "Neutral",
            Description = "If This Role is Voted out the person they voted dies",
            AC_Description = "If the Vengeance gets voted out the person they voted gets ejected/killed as well.",
            Team = GameTeam.Crewmember,
            Amount = 0
        };
        void Start()
        {
            voteManager = FindObjectOfType<VoteManager>();
            MoreRolesManager.QueueRoleDisplay(PlayerWithRole, this, Data);
        }

        public override void OnPlayerVoted(PlayerState votedPlayer)
        {
            if (voteManager.SheriffId == -1)
            {
                PlayerWithRoleVoted = votedPlayer;
            }
            else
            {
                PlayerWithRoleSkipped = true;
            }
        }

        public override void OnPlayerVotedSkip()
        {
            PlayerWithRoleSkipped = true;
        }

        public override void OnPlayerEjected(PlayerState ejectedPlayer, GameRole role)
        {
            if (ejectedPlayer == PlayerWithRole)
            {
                ChangeIsAliveWithAntiCheat(PlayerWithRoleVoted, false);
            }
            else if (PlayerWithRoleSkipped)
            {
                return;
            }
        }

        public override void OnGameEnd(GameTeam teamThatWon)
        {
            PlayerWithRoleSkipped = false;
            PlayerWithRoleVoted = null;
        }

        ///NOTE: If the Vengeance gets voted out the person they voted gets ejected/killed as well.
    }
}
