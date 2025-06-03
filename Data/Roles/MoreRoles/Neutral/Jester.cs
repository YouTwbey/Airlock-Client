using AirlockClient.Attributes;
using AirlockClient.Managers;
using AirlockClient.Managers.Gamemode;
using Il2CppSG.Airlock;
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
        public static SubRoleData Data = new SubRoleData
        {
            Name = "Jester",
            Description = "Ejected = Win",
            AC_Description = "You need to be voted out to win.",
            Team = GameTeam.Crewmember,
            Amount = 1
        };

        void Start()
        {
            MelonCoroutines.Start(MoreRolesManager.DisplayRoleInfo(PlayerWithRole, this, Data));
        }

        public override void OnPlayerEjected(PlayerState ejectedPlayer, GameRole role)
        {
            if (ejectedPlayer != null)
            {
                if (ejectedPlayer == PlayerWithRole)
                {
                    ModdedGameStateManager.Instance.QueueWin(PlayerWithRole, -1, GameplayStates.Task, 0);
                }
            }
        }
    }
}
