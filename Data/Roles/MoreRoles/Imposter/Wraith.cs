using AirlockClient.Attributes;
using AirlockClient.Managers.Gamemode;
using Il2CppSG.Airlock.Roles;
using MelonLoader;
using UnityEngine;

namespace AirlockClient.Data.Roles.MoreRoles.Imposter
{
    public class Wraith : SubRole
    {
        public static SubRoleData Data = new SubRoleData
        {
            Name = "Wraith",
            RoleType = "Imposter",
            Description = "Haunt others as a ghost",
            AC_Description = "Haunt others as a ghost",
            AC_Color = new Color(255, 0, 0, 150),
            Team = GameTeam.Impostor,
            Amount = 0
        };

        void Start()
        {
            MelonCoroutines.Start(MoreRolesManager.DisplayRoleInfo(PlayerWithRole, this, Data, "", GameRole.Revenger));
        }
    }
}
