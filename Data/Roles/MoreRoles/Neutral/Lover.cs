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
            RoleType = "Neutral",
            Description = "Protect user:",
            AC_Description = "You must protect your other lover. If anything happens to them, it also will happen to you. Lover: ",
            Team = GameTeam.Crewmember,
            Amount = 0
        };

        void Start()
        {
            List<int> validIds = new List<int>();

            foreach (PlayerState player in ((MoreRolesManager)AirlockClientGamemode.Current).Crewmates)
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

                PlayerWithRole.SoulLinkID = otherLover.PlayerWithRole.PlayerId;
                otherLover.PlayerWithRole.SoulLinkID = PlayerWithRole.PlayerId;
            }
            else
            {
                Logging.Warn("Could not find Lover for user: " + PlayerWithRole.NetworkName.Value);
                Destroy(this);
            }
        }

        OtherLover otherLover;
    }

    public class OtherLover : SubRole
    {
        public static SubRoleData Data = new SubRoleData
        {
            Name = "Lover",
            Description = "Protect user:",
            Team = GameTeam.Crewmember,
            AC_Description = "<size=0>OTHER_ROLE</size>",
            Amount = 0
        };

        void Start()
        {
            MelonCoroutines.Start(MoreRolesManager.DisplayRoleInfo(PlayerWithRole, this, Data, mainLover.PlayerWithRole.NetworkName.Value));
        }

        public Lover mainLover;
    }
}
