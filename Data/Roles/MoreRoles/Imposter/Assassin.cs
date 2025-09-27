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
            RoleType = "Imposter",
            Description = "Kill Player:",
            AC_Description = "Your goal is to kill your target before the first meeting. Target: ",
            Team = GameTeam.Imposter,
            Amount = 0
        };

        void Start()
        {
            List<int> validIds = new List<int>();

            foreach (PlayerState player in ((MoreRolesManager)AirlockClientGamemode.Current).Crewmates)
            {
                if (!player.GetComponent<Troll>()&& player.IsConnected && player != PlayerWithRole)
                {
                    validIds.Add(player.PlayerId);
                }
            }

            if (validIds.Count == 0) Destroy(this);

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
