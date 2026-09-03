using AirlockClient.Attributes;
using AirlockClient.Managers.Gamemode;
using Il2CppSG.Airlock.Roles;
using MelonLoader;
using UnityEngine;

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
            AC_Description = "Ability to withstand 2 hits",
            Team = GameTeam.Crewmember,
            AC_Color = Color.gray,
            Amount = 0
        };

        void Start()
        {
            MelonCoroutines.Start(MoreRolesManager.DisplayRoleInfo(PlayerWithRole, this, Data));
        }

        public bool HasTakenHit;
    }
}
