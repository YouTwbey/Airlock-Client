using AirlockClient.Attributes;
using AirlockClient.Managers.Debug;
using AirlockClient.Managers.Gamemode;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Network;
using Il2CppSG.Airlock.Roles;
using MelonLoader;
using System.Collections.Generic;
using UnityEngine;

namespace AirlockClient.Data.Roles.MoreRoles.Modifiers
{
    /// <summary>
    /// Neutral Role
    /// Player has to protect their lover.
    /// </summary>
    public class Lover : Modifier
    {
        public static SpawnManager spawnManager;
        public static ModifierData Data = new ModifierData
        {
            Name = "Lover",
            ModifierType = "Neutral",
            Description = "Protect user:",
            AC_Description = "You must protect your other lover. If anything happens to them, it also will happen to you. Lover: ",
            Team = GameTeam.Other,
            Amount = 0
        };

        void Start()
        {
            if (spawnManager == null)
            {
                spawnManager = FindObjectOfType<SpawnManager>();
            }
            List<int> validIds = new List<int>();

            foreach (PlayerState player in spawnManager.ActivePlayerStates)
            {
                if (player.IsConnected && player != PlayerWithModifier && player.GetComponent<Modifier>() == null)
                {
                    validIds.Add(player.PlayerId);
                }
            }

            if (validIds.Count > 0)
            {
                otherLover = GameObject.Find("PlayerState (" + validIds[Random.Range(0, validIds.Count)].ToString() + ")").AddComponent<OtherLover>();
                otherLover.mainLover = this;

                PlayerWithModifier.SoulLinkID = otherLover.PlayerWithModifier.PlayerId;
                otherLover.PlayerWithModifier.SoulLinkID = PlayerWithModifier.PlayerId;
            }
            else
            {
                Logging.Warn("Could not find Lover for user: " + PlayerWithModifier.NetworkName.Value);
                Destroy(this);
            }

            MoreRolesManager.QueueModifierDisplay(PlayerWithModifier, Data);
        }

        OtherLover otherLover;
    }

    public class OtherLover : Modifier
    {
        public static ModifierData Data = new ModifierData
        {
            Name = "Lover",
            Description = "Protect user:",
            Team = GameTeam.Other,
            AC_Description = "<size=0>OTHER_MODIFIER</size>",
            Amount = 0
        };

        public Lover mainLover;
    }
}