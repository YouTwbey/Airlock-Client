using AirlockClient.Attributes;
using AirlockClient.Managers.Debug;
using AirlockClient.Managers.Gamemode;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Roles;
using MelonLoader;

namespace AirlockClient.Data.Roles.MoreRoles.Crewmate
{
    /// <summary>
    /// A crewmate role that forces the imposter to report your body once you die.
    /// </summary>
    public class Bait : SubRole
    {
        public static SubRoleData Data = new SubRoleData
        {
            Name = "Bait",
            RoleType = "Crewmate",
            Description = "Auto Report Body",
            AC_Description = "When an imposter kills you, they will automatically report your body.",
            Team = GameTeam.Crewmember,
            Amount = 0
        };

        void Start()
        {
            MelonCoroutines.Start(MoreRolesManager.DisplayRoleInfo(PlayerWithRole, this, Data));
        }

        public override void OnPlayerDied(PlayerState killer)
        {
            if (FindObjectOfType<VoteManager>())
            {
                FindObjectOfType<VoteManager>().RPC_CallVote(PlayerWithRole.PlayerId, killer.PlayerId, true);
                return;
            }

            Logging.Error("VoteManager happens to be null in OnPlayerDied.");
        }
    }
}
