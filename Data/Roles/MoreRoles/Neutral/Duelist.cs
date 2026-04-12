using AirlockClient.AC;
using AirlockClient.Attributes;
using AirlockClient.Managers.Debug;
using AirlockClient.Managers.Gamemode;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Roles;
using MelonLoader;
using System.Collections.Generic;
using UnityEngine;


namespace AirlockClient.Data.Roles.MoreRoles.Neutral
{
    /// <summary>
    /// Neutral
    /// At the start of a game two players are duelists, one has to kill the other before the first meeting or they both die
    /// </summary>
    public class Duelist : SubRole
    {
        public static SubRoleData Data = new SubRoleData
        {
            Name = "Duelist",
            RoleType = "Neutral",
            Description = "Kill Duelist:",
            AC_Description = "Kill the other duelist, before the first meeting or you both die.",
            Team = GameTeam.Crewmember,
            Amount = 0
        };

        void Start()
        {
            MoreRolesManager moreRoles = (MoreRolesManager)AirlockClientGamemode.Current;

            if (moreRoles != null)
            {
                List<PlayerState> validIds = new List<PlayerState>();

                foreach (PlayerState player in moreRoles.Crewmates)
                {
                    if (player.IsConnected && player != PlayerWithRole && player.GetComponent<SubRole>() == null)
                    {
                        validIds.Add(player);
                    }
                }

                if (validIds.Count > 0)
                {
                    PlayerState rival = validIds[Random.Range(0, validIds.Count)];
                    playerToKill = rival;
                    OtherDuelist = rival.gameObject.AddComponent<OtherDuelist>();
                    OtherDuelist.MainDuelist = this;
                    OtherDuelist.playerToKill = PlayerWithRole;

                    PlayerWithRole.SoulLinkID = playerToKill.PlayerId;
                    MoreRolesManager.QueueRoleDisplay(PlayerWithRole, this, Data, playerToKill.NetworkName.Value, GameRole.Vigilante);
                }
                else
                {
                    Logging.Error("Found no players to equip other duelist. Removing role...");
                    Destroy(this);
                }
            }
            else
            {
                Logging.Error("Cannot add role outside of More Roles.");
            }
        }

        public PlayerState playerToKill;

        public override void OnPlayerKilled(PlayerState playerKilled)
        {
            if (playerKilled == playerToKill)
            {
                playerToKill = null;

                if (OtherDuelist != null)
                    OtherDuelist.playerToKill = null;
            }
        }

        public override void OnVotingBegan(PlayerState bodyReported, PlayerState reportingPlayer)
        {
            if (this.PlayerWithRole.IsAlive && OtherDuelist.PlayerWithRole.IsAlive)
            {
                AntiCheat.ChangeIsAliveWithAntiCheat(PlayerWithRole, false);
                AntiCheat.ChangeIsAliveWithAntiCheat(playerToKill, false);

                playerToKill = null;
                if (OtherDuelist != null)
                    OtherDuelist.playerToKill = null;
            }
        }

        OtherDuelist OtherDuelist;
    }
    public class OtherDuelist : SubRole
    {
        public static SubRoleData Data = new SubRoleData
        {
            Name = "Duelist",
            Description = "Other Duelist:",
            AC_Description = "<size=0>OTHER_ROLE</size>",
            Team = GameTeam.Crewmember,
            Amount = 0
        };

        public Duelist MainDuelist;
        public PlayerState playerToKill;

        void Start()
        {
            MoreRolesManager.QueueRoleDisplay(PlayerWithRole, this, Data, "", GameRole.Vigilante);
        }

        public override void OnPlayerKilled(PlayerState playerKilled)
        {
            if (playerKilled == playerToKill)
            {
                playerToKill = null;
                if (MainDuelist != null)
                {
                    playerToKill = null;
                }
            }
        }

        public override void OnVotingBegan(PlayerState bodyReported, PlayerState reportingPlayer)
        {
            if (playerToKill != null)
            {
                AntiCheat.ChangeIsAliveWithAntiCheat(PlayerWithRole, false);
                AntiCheat.ChangeIsAliveWithAntiCheat(playerToKill, false);
                playerToKill = null;
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