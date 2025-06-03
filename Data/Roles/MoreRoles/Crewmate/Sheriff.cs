using AirlockClient.Attributes;
using AirlockClient.Managers.Gamemode;
using Il2CppSG.Airlock.Roles;
using MelonLoader;

namespace AirlockClient.Data.Roles.MoreRoles.Crewmate
{
    /// <summary>
    /// Crewmate Role
    /// Sherrif has to kill imposter. If they kill a crewmate, they will die and the crewmate lives.
    /// </summary>
    public class Sheriff : SubRole
    {
        public static SubRoleData Data = new SubRoleData
        {
            Name = "Sheriff",
            Description = "Kill Imposter",
            AC_Description = "Kill the imposters to help the crew win.",
            Team = GameTeam.Crewmember,
            Amount = 1
        };

        void Start()
        {
            MelonCoroutines.Start(MoreRolesManager.DisplayRoleInfo(PlayerWithRole, this, Data, "", GameRole.Vigilante));
        }
    }
}
