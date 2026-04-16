using AirlockClient.Attributes;
using AirlockClient.Managers;
using AirlockClient.Managers.Gamemode;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Network;
using Il2CppSG.Airlock.Roles;
using MelonLoader;

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
            Team = GameTeam.Other,
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
            killing.AlterRole(GameRole.Sheriff, PlayerWithRole.PlayerId, 0);
            state.EndGame(GameTeam.Other);
        }
    }
}
