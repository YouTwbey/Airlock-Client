using AirlockClient.AC;
using AirlockClient.Attributes;
using AirlockClient.Handlers;
using AirlockClient.Managers;
using AirlockClient.Managers.Gamemode;
using SG.Airlock.Roles;
using System.Collections;
using UnityEngine;

namespace AirlockClient.Data.Roles.MoreRoles.Crewmate
{
    /// <summary>
    /// Crewmate Role
    /// Person can not shut up, not speaking will result in their death.
    /// </summary>
    public class Yapper : SubRole
    {
        public static SubRoleData Data = new SubRoleData
        {
            Name = "Yapper",
            RoleType = "Crewmate",
            Description = "Keep talk or die",
            AC_Description = "Can't stop speaking",
            AC_Color = new Color(179, 251, 255),
            Team = GameTeam.Crewmember,
            Amount = 0
        };

        void Start()
        {
            MelonCoroutines.Start(MoreRolesManager.DisplayRoleInfo(PlayerWithRole, this, Data));
        }


        bool isCheckInProgress;

        void Update()
        {
            if (ModdedGameStateManager.Instance.state.InTaskState() || ModdedGameStateManager.Instance.state.InVotingState())
            {
                if (PlayerWithRole.MicrophoneOutput <= 0.1f && PlayerWithRole.IsAlive && !isCheckInProgress)
                {
                    CoroutineHandler.Start(MicTimer());
                }
            }
        }

        IEnumerator MicTimer()
        {
            isCheckInProgress = true;

            yield return new WaitForSeconds(1);

            if (PlayerWithRole && PlayerWithRole.IsAlive && PlayerWithRole.MicrophoneOutput <= 0.1f)
            {
                AntiCheat.KillPlayerWithAntiCheat(PlayerWithRole, PlayerWithRole);
            }

            isCheckInProgress = false;
        }
    }
}
