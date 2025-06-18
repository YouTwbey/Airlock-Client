using AirlockClient.AC;
using AirlockClient.Attributes;
using AirlockClient.Data.Roles.MoreRoles.Neutral;
using AirlockClient.Managers.Gamemode;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Roles;
using MelonLoader;
using System.Collections.Generic;
using UnityEngine;

namespace AirlockClient.Data.Roles.MoreRoles.Imposter
{
    /// <summary>
    /// A role that needs to kill one person before the meeting.
    /// </summary>
    public class Assassin : SubRole
    {
        public static SubRoleData Data = new SubRoleData
        {
            Name = "Assassin",
            Description = "Kill Player:",
            AC_Description = "Your goal is to kill your target before the first meeting. Target: ",
            Team = GameTeam.Crewmember,
            Amount = 1
        };

        void Start()
        {
            List<int> validIds = new List<int>();

            foreach (PlayerState player in FindObjectsOfType<PlayerState>())
            {
                if (ModdedGamemode.Current.GetTrueRole(PlayerWithRole) != GameRole.Imposter && !player.GetComponent<Troll>())
                {
                    validIds.Add(player.PlayerId);
                }
            }

            playerToKill = GameObject.Find("PlayerState (" + validIds[Random.Range(0, validIds.Count)].ToString() + ")").GetComponent<PlayerState>();
            MelonCoroutines.Start(MoreRolesManager.DisplayRoleInfo(PlayerWithRole, this, Data, playerToKill.NetworkName.Value));
        }

        PlayerState playerToKill;

        public override void OnPlayerKilled(PlayerState playerKilled)
        {
            if (playerKilled == playerToKill)
            {
                playerToKill = null;
            }
        }

        public override void OnVotingBegan(PlayerState bodyReported, PlayerState reportingPlayer)
        {
            if (playerToKill != null)
            {
                AntiCheat.ChangeIsAliveWithAntiCheat(PlayerWithRole, false);
                playerToKill = null;
            }
        }
    }
}
