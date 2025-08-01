using AirlockClient.Attributes;
using AirlockClient.Managers.Gamemode;
using Il2CppSG.Airlock.Roles;
using MelonLoader;

namespace AirlockClient.Data.Roles.MoreRoles.Crewmate
{
    /// <summary>
    /// A crewmate role than can withstand 2 kills.
    /// </summary>
    public class Armorer : SubRole
    {
        public static SubRoleData Data = new SubRoleData
        {
            Name = "Armorer",
            RoleType = "Crewmate",
            Description = "Take 2 Hits",
            AC_Description = "You are able to withstand 2 kills from an imposter.",
            Team = GameTeam.Crewmember,
            Amount = 0
        };

        void Start()
        {
            MelonCoroutines.Start(MoreRolesManager.DisplayRoleInfo(PlayerWithRole, this, Data));
        }

        public bool HasTakenHit;
    }
}
