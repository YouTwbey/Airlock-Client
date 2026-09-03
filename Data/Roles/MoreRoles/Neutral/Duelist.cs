using AirlockClient.AC;
using AirlockClient.Attributes;
using AirlockClient.Managers.Debug;
using AirlockClient.Managers.Gamemode;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Roles;
using MelonLoader;
using System.Collections.Generic;
using AirlockClient.Utils;
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
            AC_Description = "Duel it out with your chosen target",
            Team = GameTeam.Crewmember,
            AC_Color = new Color(255, 100, 0),
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
                    otherDuelist = rival.gameObject.AddComponent<OtherDuelist>();
                    otherDuelist.mainDuelist = this;
                    otherDuelist.playerToKill = PlayerWithRole;
                    MelonCoroutines.Start(MoreRolesManager.DisplayRoleInfo(PlayerWithRole, this, Data, otherDuelist.PlayerWithRole.NetworkName.Value, GameRole.Vigilante));
                }
                else
                {
                    Logging.Error("Found no players to equip other duelist. Removing role...");
                    Destroy(this);
                }
            }
            else
            {
                Logging.Error("Cannot addd role outside of More Roles.");
            }
        }

        public PlayerState playerToKill;

        public override void OnPlayerKilled(PlayerState playerKilled)
        {
            if (playerKilled == playerToKill)
            {
                playerToKill = null;

                if (otherDuelist != null)
                    otherDuelist.playerToKill = null;
            }
        }

        public override void OnVotingBegan(PlayerState bodyReported, PlayerState reportingPlayer)
        {
            if (playerToKill != null)
            {
                PlayerWithRole.ChangeIsAliveWithAntiCheat(false);
                playerToKill.ChangeIsAliveWithAntiCheat(false);

                playerToKill = null;
                if (otherDuelist != null)
                    otherDuelist.playerToKill = null;
            }
        }

        OtherDuelist otherDuelist;
    }
    public class OtherDuelist : SubRole
    {
        public static SubRoleData Data = new SubRoleData
        {
            Name = "Duelist",
            Description = "Other Duelist:",
            AC_Description = "<size=0>OTHER_ROLE</size>Duel it out with your chosen target",
            Team = GameTeam.Crewmember,
            AC_Color = new Color(255, 100, 0),
            Amount = 0
        };

        public Duelist mainDuelist;
        public PlayerState playerToKill;

        void Start()
        {
            MelonCoroutines.Start(MoreRolesManager.DisplayRoleInfo(PlayerWithRole, this, Data, mainDuelist.PlayerWithRole.NetworkName.Value, GameRole.Vigilante));
        }

        public override void OnPlayerKilled(PlayerState playerKilled)
        {
            if (playerKilled == playerToKill)
            {
                playerToKill = null;
                if (mainDuelist != null)
                {
                    playerToKill = null;
                }
            }
        }

        public override void OnVotingBegan(PlayerState bodyReported, PlayerState reportingPlayer)
        {
            if (playerToKill != null)
            {
                PlayerWithRole.ChangeIsAliveWithAntiCheat(false);
                playerToKill.ChangeIsAliveWithAntiCheat(false);
                playerToKill = null;
            }
        }
    }
}