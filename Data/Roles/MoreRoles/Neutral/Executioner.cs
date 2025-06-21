using AirlockClient.Attributes;
using AirlockClient.Managers;
using AirlockClient.Managers.Gamemode;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Roles;
using MelonLoader;
using System.Collections.Generic;
using UnityEngine;

namespace AirlockClient.Data.Roles.MoreRoles.Neutral
{
    /// <summary>
    /// Neutral Role
    /// At the start of a game, the player with this role is given a player.
    /// This player needs to get voted out for the executioner to win.
    /// </summary>
    public class Executioner : SubRole
    {
        public static SubRoleData Data = new SubRoleData
        {
            Name = "Executioner",
            Description = "Eject Player:",
            AC_Description = "Your goal is to eject your target. Target: ",
            Team = GameTeam.Crewmember,
            Amount = 1
        };

        void Start()
        {
            List<int> validIds = new List<int>();

            foreach (PlayerState player in ((MoreRolesManager)ModdedGamemode.Current).Crewmates)
            {
                if (player.IsConnected && player != PlayerWithRole)
                {
                    validIds.Add(player.PlayerId);
                }
            }

            if (validIds.Count == 0) Destroy(this);

            playerToVoteOut = GameObject.Find("PlayerState (" + validIds[Random.Range(0, validIds.Count)].ToString() + ")").GetComponent<PlayerState>();
            MelonCoroutines.Start(MoreRolesManager.DisplayRoleInfo(PlayerWithRole, this, Data, playerToVoteOut.NetworkName.Value));
        }

        PlayerState playerToVoteOut;

        public override void OnPlayerEjected(PlayerState ejectedPlayer, GameRole role)
        {
            if (ejectedPlayer != null)
            {
                if (ejectedPlayer == playerToVoteOut && PlayerWithRole.IsAlive)
                {
                    ModdedGameStateManager.Instance.QueueWin(PlayerWithRole, FindObjectOfType<GameStateManager>().NoImpostorsLeftWin, GameplayStates.Task, 1);
                }
            }
        }
    }
}
