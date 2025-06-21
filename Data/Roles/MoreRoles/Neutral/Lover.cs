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
    /// Neutral Role
    /// Player has to protect their lover.
    /// </summary>
    public class Lover : SubRole
    {
        public static SubRoleData Data = new SubRoleData
        {
            Name = "Lover",
            Description = "Protect user:",
            AC_Description = "You must protect your other lover. If anything happens to them, it also will happen to you. Lover: ",
            Team = GameTeam.Crewmember,
            Amount = 1
        };

        void Start()
        {
            List<int> validIds = new List<int>();

            foreach (PlayerState player in ((MoreRolesManager)ModdedGamemode.Current).Crewmates)
            {
                if (player.IsConnected && player != PlayerWithRole && player.GetComponent<SubRole>() == null)
                {
                    validIds.Add(player.PlayerId);
                }
            }

            if (validIds.Count > 0)
            {
                otherLover = GameObject.Find("PlayerState (" + validIds[Random.Range(0, validIds.Count)].ToString() + ")").AddComponent<OtherLover>();
                otherLover.mainLover = this;
                MelonCoroutines.Start(MoreRolesManager.DisplayRoleInfo(PlayerWithRole, this, Data, otherLover.PlayerWithRole.NetworkName.Value));
            }
            else
            {
                Logging.Warn("Could not find Lover for user: " + PlayerWithRole.NetworkName.Value);
                Destroy(this);
            }
        }

        OtherLover otherLover;

        public override void OnPlayerDied(PlayerState killer)
        {
            AntiCheat.KillPlayerWithAntiCheat(killer, otherLover.PlayerWithRole);
        }

        public override void OnPlayerEjected(PlayerState ejectedPlayer, GameRole role)
        {
            if (ejectedPlayer != null)
            {
                if (ejectedPlayer == PlayerWithRole)
                {
                    otherLover.PlayerWithRole.IsAlive = false;
                }
            }
        }
    }

    public class OtherLover : SubRole
    {
        public static SubRoleData Data = new SubRoleData
        {
            Name = "Lover",
            Description = "Protect user:",
            Team = GameTeam.Crewmember,
            Amount = 0
        };

        void Start()
        {
            MelonCoroutines.Start(MoreRolesManager.DisplayRoleInfo(PlayerWithRole, this, Data, mainLover.PlayerWithRole.NetworkName.Value));
        }

        public override void OnPlayerDied(PlayerState killer)
        {
            AntiCheat.KillPlayerWithAntiCheat(killer, mainLover.PlayerWithRole);
        }

        public override void OnPlayerEjected(PlayerState ejectedPlayer, GameRole role)
        {
            if (ejectedPlayer != null)
            {
                if (ejectedPlayer == PlayerWithRole)
                {
                    mainLover.PlayerWithRole.IsAlive = false;
                }
            }
        }

        public Lover mainLover;
    }
}
