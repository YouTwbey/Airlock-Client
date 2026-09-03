using AirlockClient.Attributes;
using AirlockClient.Managers;
using AirlockClient.Managers.Gamemode;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Roles;
using MelonLoader;
using UnityEngine;

namespace AirlockClient.Data.Roles.MoreRoles.Crewmate
{
    /// <summary>
    /// An imposter role that allows you to explode, killing others around you including yourself.
    /// </summary>
    public class Magician : SubRole
    {
        public static SubRoleData Data = new SubRoleData
        {
            Name = "Magician",
            RoleType = "Crewmate",
            Description = "Go Invisible",
            AC_Description = "Can vanish every time a meeting ends",
            AC_Color = Color.blue,
            Team = GameTeam.Crewmember,
            Amount = 0
        };

        void Start()
        {
            MelonCoroutines.Start(MoreRolesManager.DisplayRoleInfo(PlayerWithRole, this, Data));
        }

        bool canRecieveVanish;
        void Update()
        {
            if (ModdedGameStateManager.Instance.state.InTaskState())
            {
                if (canRecieveVanish && PlayerWithRole.IsAlive)
                {
                    PlayerWithRole.ActivePowerUps = PowerUps.Vanish;
                    canRecieveVanish = false;
                }
            }
            else
            {
                canRecieveVanish = true;
            }
        }
    }
}
