using AirlockClient.Attributes;
using AirlockClient.Managers;
using AirlockClient.Managers.Gamemode;
using SG.Airlock;
using SG.Airlock.Network;
using SG.Airlock.Roles;


namespace AirlockClient.Data.Roles.MoreRoles.Neutral
{
    public class Troll : SubRole
    {
        public GameStateManager state;
        public NetworkedKillBehaviour killing;
        /// <summary>
        /// A neutral role that needs to die to win.
        /// </summary>
        public static SubRoleData Data = new SubRoleData
        {
            Name = "Troll",
            RoleType = "Neutral",
            Description = "Death = Win",
            AC_Description = "You need to be killed to win.",
            Team = GameTeam.Crewmember,
            Amount = 0
        };

        void Start()
        {
            killing = FindObjectOfType<NetworkedKillBehaviour>();
            state = FindObjectOfType<GameStateManager>();
            MoreRolesManager.QueueRoleDisplay(PlayerWithRole, this, Data);
        }

        public override void OnPlayerDied(PlayerState killer)
        {
            if (killer.KnownGameRole != GameRole.Sheriff)
            {
                killing.AlterRole(GameRole.Sheriff, PlayerWithRole.PlayerId, 0);
                state.EndGame(GameTeam.Other);
            }
        }
    }
}
