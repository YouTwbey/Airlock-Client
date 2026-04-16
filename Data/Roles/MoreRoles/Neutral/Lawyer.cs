using AirlockClient.Attributes;
using AirlockClient.Managers.Debug;
using AirlockClient.Managers.Gamemode;
using AirlockClient.Patches;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Network;
using Il2CppSG.Airlock.Roles;
using MelonLoader;
using System.Collections.Generic;
using UnityEngine;

namespace AirlockClient.Data.Roles.MoreRoles.Neutral
{
    public class Lawyer : SubRole
    {
        public PlayerState Client;
        public SpawnManager Spawn;
        public bool LawyerWinsWithClient = true;
        public static Lawyer Instance;

        public static SubRoleData Data = new SubRoleData
        {
            Name = "Lawyer",
            RoleType = "Neutral",
            Description = "Protect Client",
            AC_Description = "As the Lawyer If your Client wins you win with them, otherwise your just a normal crew if they get voted out",
            Team = GameTeam.Crewmember,
            Amount = 0
        };

        void Start()
        {
            Instance = this;
            MoreRolesManager moreRoles = (MoreRolesManager)AirlockClientGamemode.Current;
            Spawn = FindObjectOfType<SpawnManager>();

            if (moreRoles != null && Spawn != null)
            {
                List<PlayerState> validIds = new List<PlayerState>();

                foreach (PlayerState player in Spawn.ActivePlayerStates)
                {
                    if (player.IsConnected && player != PlayerWithRole)
                    {
                        validIds.Add(player);
                    }
                }

                if (validIds.Count > 0)
                {
                    Client = validIds[Random.Range(0, validIds.Count)];

                    PlayerWithRole.SoulLinkID = Client.PlayerId;
                    MoreRolesManager.QueueRoleDisplay(PlayerWithRole, this, Data);
                }
                else
                {
                    Logging.Error("Found no players to make Lawyer's Client. Removing role...");
                    Destroy(this);
                }
            }
            else
            {
                Logging.Error("Cannot add role outside of More Roles.");
            }
        }

        public override void OnPlayerEjected(PlayerState ejectedPlayer, GameRole role)
        {
            if (ejectedPlayer == Client)
            {
                PlayerWithRole.SoulLinkID = -1;
                LawyerWinsWithClient = false;
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
