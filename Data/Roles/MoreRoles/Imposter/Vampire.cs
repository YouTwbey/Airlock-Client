using AirlockClient.Attributes;
using AirlockClient.Managers.Gamemode;
using Il2CppSG.Airlock.Roles;
using MelonLoader;

namespace AirlockClient.Data.Roles.MoreRoles.Imposter
{
    /// <summary>
    /// Imposter Role
    /// A vampire can only kill in the dark.
    /// </summary>
    public class Vampire : SubRole
    {
        public static SubRoleData Data = new SubRoleData
        {
            Name = "Vampire",
            Description = "Only kill @ dark",
            AC_Description = "You can only kill others when the lights are out.",
            Team = GameTeam.Imposter,
            Amount = 1
        };

        void Start()
        {
            MelonCoroutines.Start(MoreRolesManager.DisplayRoleInfo(PlayerWithRole, this, Data));
        }
    }
}
