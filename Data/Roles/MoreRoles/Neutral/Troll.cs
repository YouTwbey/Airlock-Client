using AirlockClient.Attributes;
using AirlockClient.Managers;
using AirlockClient.Managers.Gamemode;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Roles;
using MelonLoader;

namespace AirlockClient.Data.Roles.MoreRoles.Neutral
{
    public class Troll : SubRole
    {
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
            MelonCoroutines.Start(MoreRolesManager.DisplayRoleInfo(PlayerWithRole, this, Data));
        }

        public override void OnPlayerDied(PlayerState killer)
        {
            ModdedGameStateManager.Instance.QueueWin(PlayerWithRole, -1, GameplayStates.Task, 0);
        }
    }
}
