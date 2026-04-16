using AirlockClient.Attributes;
using AirlockClient.Managers;
using AirlockClient.Managers.Gamemode;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Network;
using Il2CppSG.Airlock.Roles;
using MelonLoader;

namespace AirlockClient.Data.Roles.MoreRoles.Neutral
{
    /// <summary>
    /// Neutral Role
    /// This player needs to be voted out to win.
    /// </summary>
    public class Jester : SubRole
    {
        public GameStateManager state;
        public NetworkedKillBehaviour killing;

        public static SubRoleData Data = new SubRoleData
        {
            Name = "Jester",
            RoleType = "Neutral",
            Description = "Ejected = Win",
            AC_Description = "You need to be voted out to win.",
            Team = GameTeam.Crewmember,
            Amount = 0
        };

        void Start()
        {
            state = FindObjectOfType<GameStateManager>();
            killing = FindObjectOfType<NetworkedKillBehaviour>();
            MoreRolesManager.QueueRoleDisplay(PlayerWithRole, this, Data);
        }

        public override void OnPlayerEjected(PlayerState ejectedPlayer, GameRole role)
        {
            if (ejectedPlayer != null)
            {
                if (ejectedPlayer == PlayerWithRole)
                {
                    killing.AlterRole(GameRole.Sheriff, PlayerWithRole.PlayerId, 0);
                    state.EndGame(GameTeam.Other);
                }
            }
        }
    }
}
