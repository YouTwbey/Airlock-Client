using AirlockClient.Attributes;
using AirlockClient.Managers.Gamemode;
using Il2CppSG.Airlock.Roles;
using MelonLoader;

namespace AirlockClient.Data.Roles.MoreRoles.Imposter
{
    public class Wraith : SubRole
    {
        public static SubRoleData Data = new SubRoleData
        {
            Name = "Wraith",
            RoleType = "Imposter",
            Description = "Kill when dead",
            Team = GameTeam.Impostor,
            Amount = 0
        };

        void Start()
        {
            MelonCoroutines.Start(MoreRolesManager.DisplayRoleInfo(PlayerWithRole, this, Data, "", GameRole.Revenger));
        }
    }
}
