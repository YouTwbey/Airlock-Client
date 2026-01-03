using AirlockClient.AC;
using AirlockClient.Attributes;
using AirlockClient.Managers.Gamemode;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Roles;
using MelonLoader;
using System.Collections.Generic;
using UnityEngine;

namespace AirlockClient.Data.Roles.MoreRoles.Broken
{
    /// <summary>
    /// Imposter Role
    /// A witch can put spells on people. When a meeting ends, those people will be taken away.
    /// </summary>
    public class Witch : SubRole
    {
        public static SubRoleData Data = new SubRoleData
        {
            Name = "Witch",
            RoleType = "Imposter",
            Description = "Cast spells",
            AC_Description = "Cast spells on the crew",
            AC_Color = Color.magenta,
            Team = GameTeam.Impostor,
            Amount = 0
        };

        void Start()
        {
            MelonCoroutines.Start(MoreRolesManager.DisplayRoleInfo(PlayerWithRole, this, Data));
        }

        public Dictionary<PlayerState, string> spellsCasted = new Dictionary<PlayerState, string>();

        void AddSpell(PlayerState state)
        {
            if (!spellsCasted.ContainsKey(state))
            {
                AntiCheat.CastSpellWithAntiCheat(this, state);
            }
        }

        void RemoveSpell(PlayerState state, bool toggleKill = false)
        {
            if (spellsCasted.ContainsKey(state))
            {
                AntiCheat.RemoveSpellWithAntiCheat(this, state, toggleKill);
            }
        }

        public override void OnGameEnd(GameTeam teamThatWon)
        {
            foreach (PlayerState player in spellsCasted.Keys)
            {
                RemoveSpell(player);
            }
        }

        public override void OnPlayerKilled(PlayerState playerKilled)
        {
            AddSpell(playerKilled);
        }

        public override void OnPlayerEjected(PlayerState ejectedPlayer, GameRole role)
        {
            foreach (PlayerState player in spellsCasted.Keys)
            {
                RemoveSpell(player, true);
            }
        }

        public override void OnVotingBegan(PlayerState bodyReported, PlayerState reportingPlayer)
        {
            foreach (PlayerState player in spellsCasted.Keys)
            {
                player.NetworkName.Value = "[†] " + spellsCasted[player];
            }
        }
    }
}
