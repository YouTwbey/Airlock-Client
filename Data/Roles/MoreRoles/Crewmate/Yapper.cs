using AirlockClient.Attributes;
using AirlockClient.Managers;
using Il2CppSG.Airlock.Roles;
using MelonLoader;
using Il2CppSG.Airlock.Network;
using System.Collections;
using UnityEngine;
using AirlockClient.Managers.Gamemode;

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
            Description = "Keep talk or die",
            AC_Description = "If at any moment you stop talking, you will die.",
            Team = GameTeam.Crewmember,
            Amount = 1
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
                    MelonCoroutines.Start(MicTimer());
                }
            }
        }

        IEnumerator MicTimer()
        {
            isCheckInProgress = true;

            yield return new WaitForSeconds(1);

            if (PlayerWithRole && PlayerWithRole.IsAlive && PlayerWithRole.MicrophoneOutput <= 0.1f)
            {
                NetworkedKillBehaviour killer = FindObjectOfType<NetworkedKillBehaviour>();
                AirlockPeer peer = FindObjectOfType<AirlockPeer>();
                killer.KillPlayer(peer, PlayerWithRole, PlayerWithRole.PlayerId, false);
            }

            isCheckInProgress = false;
        }
    }
}
