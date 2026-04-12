using AirlockClient.Attributes;
using AirlockClient.Managers;
using AirlockClient.Managers.Gamemode;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Network;
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
        public GameStateManager state;
        public NetworkedKillBehaviour killing;

        public static SubRoleData Data = new SubRoleData
        {
            Name = "Executioner",
            RoleType = "Neutral",
            Description = "Eject Player:",
            AC_Description = "Your goal is to eject your target. Target: ",
            Team = GameTeam.Other,
            Amount = 0
        };

        void Start()
        {
            killing = FindObjectOfType<NetworkedKillBehaviour>();
            state = FindObjectOfType<GameStateManager>();
            List<int> validIds = new List<int>();

            foreach (PlayerState player in ((MoreRolesManager)AirlockClientGamemode.Current).Crewmates)
            {
                if (player.IsConnected && player != PlayerWithRole)
                {
                    validIds.Add(player.PlayerId);
                }
            }

            if (validIds.Count == 0) Destroy(this);

            playerToVoteOut = GameObject.Find("PlayerState (" + validIds[Random.Range(0, validIds.Count)].ToString() + ")").GetComponent<PlayerState>();


            PlayerWithRole.SoulLinkID = playerToVoteOut.PlayerId;

            MoreRolesManager.QueueRoleDisplay(PlayerWithRole, this, Data);
        }

        PlayerState playerToVoteOut;

        public override void OnPlayerEjected(PlayerState ejectedPlayer, GameRole role)
        {
            if (ejectedPlayer != null)
            {
                if (ejectedPlayer == playerToVoteOut && PlayerWithRole.IsAlive)
                {
                    killing.AlterRole(GameRole.Sheriff, PlayerWithRole.PlayerId, 0);
                    state.EndGame(GameTeam.Other);
                }
            }
        }

        public static string GetColorName(int colorIndex)
        {
            switch (colorIndex)
            {
                case 0: return "Red";
                case 1: return "Blue";
                case 2: return "Green";
                case 3: return "Pink";
                case 4: return "Orange";
                case 5: return "Yellow";
                case 6: return "Black";
                case 7: return "White";
                case 8: return "Purple";
                case 9: return "Brown";
                case 10: return "Cyan";
                case 11: return "Lime";
                default: return "No Target";
            }
        }
    }
}
