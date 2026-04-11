using System.Collections.Generic;
using AirlockClient.AC;
using AirlockClient.Attributes;
using AirlockClient.Managers.Debug;
using AirlockClient.Managers.Gamemode;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Roles;
using MelonLoader;
using static UnityEngine.GraphicsBuffer;

namespace AirlockClient.Data.Roles.MoreRoles.Imposter
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
            AC_Description = "You can cast spells as a witch. Crewmates you cast spells on will be flagged with [†] when a meeting is called and be killed after.",
            Team = GameTeam.Impostor,
            Amount = 0
        };

        void Start()
        {
            MoreRolesManager.QueueRoleDisplay(PlayerWithRole, this, Data);
        }

        public Dictionary<PlayerState, string> spellsCasted = new Dictionary<PlayerState, string>();

        void AddSpell(PlayerState target)
        {
            if (!spellsCasted.ContainsKey(target))
            {
                spellsCasted[target] = target.NetworkName.Value;
                AntiCheat.CastSpellWithAntiCheat(this, target);
            }
        }


        void RemoveSpell(PlayerState target, bool toggleKill = false)
        {
            if (spellsCasted.ContainsKey(target))
            {
                string OGName = spellsCasted[target];

                AntiCheat.RemoveSpellWithAntiCheat(this, target, toggleKill);

                // Note: Remove these two lines if you want it to keep the [†] as a sign of death from the witch post meeting
                if (target.NetworkName.Value.Contains("[†]"))
                    target.NetworkName = OGName;

                spellsCasted.Remove(target);
            }
        }


        public override void OnGameEnd(GameTeam teamThatWon)
        {
            foreach (PlayerState player in spellsCasted.Keys)
            {
                RemoveSpell(player);
            }
        }

        public override void OnSpellCast(PlayerState cursed)
        {
            AddSpell(cursed);
        }

        public override void OnPlayerEjected(PlayerState ejectedPlayer, GameRole role)
        {
            var witchPlayer = FindObjectOfType<Witch>().PlayerWithRole;

            if (ejectedPlayer == witchPlayer)
            {
                foreach (PlayerState player in spellsCasted.Keys)
                {
                    RemoveSpell(player, false);
                    Logging.Log($"ejected: {ejectedPlayer}, clearing: {player.PlayerModerationUsername ?? "nobody"}");
                }
                return;
            }

            if (witchPlayer != null && witchPlayer.IsAlive)
            {
                foreach (PlayerState player in spellsCasted.Keys)
                {
                    RemoveSpell(player, true);
                    Logging.Log($"voted: {ejectedPlayer} killing: {player.PlayerModerationUsername ?? "nobody"}");
                }
            }
        }

        public override void OnVotingBegan(PlayerState bodyReported, PlayerState reportingPlayer)
        {
            foreach (KeyValuePair<PlayerState, string> Spell in spellsCasted)
            {
                PlayerState Target = Spell.Key;
                if (Target.IsAlive && !Target.NetworkName.Value.Contains("[†]"))
                    Target.NetworkName = "[†] " + Spell.Value;
            }
        }

        public override void OnPlayerKilled(PlayerState playerKilled)
        {
            if (playerKilled == PlayerWithRole)
            {
                foreach (PlayerState player in spellsCasted.Keys)
                {
                    RemoveSpell(player, false);
                }
            }
        }
    }
}