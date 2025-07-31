using AirlockClient.Attributes;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Roles;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

namespace AirlockClient.Managers.Gamemode
{
    public class RoundUpManager : AirlockClientGamemode
    {
        public PlayerState deputy;
        GameRole defaultDeputyRole;
        string defaultDeputyName;

        void Update()
        {
            if (deputy != null)
            {
                deputy.NetworkName.Value = "DEPUTY";
            }
        }

        public override bool OnVotingBegan(ref PlayerState bodyReported, ref PlayerState reportingPlayer)
        {
            System.Random rng = new System.Random();
            List<PlayerState> validPlayers = new List<PlayerState>();

            foreach (PlayerState player in State.SpawnManager.PlayerStates)
            {
                if (player.IsAlive)
                {
                    validPlayers.Add(player);
                }
            }

            validPlayers = validPlayers.OrderBy(_ => rng.Next()).ToList();
            defaultDeputyName = validPlayers[0].NetworkName.Value;
            deputy = validPlayers[0];

            defaultDeputyRole = GetTrueRole(deputy);
            Role.AlterPlayerRole(GameRole.Sheriff, deputy.PlayerId);

            return true;
        }

        public override bool OnPlayerVoted(ref PlayerState voter, ref PlayerState voted)
        {
            if (voter != deputy)
            {
                return false;
            }
            else
            {
                OnDeputyVote(voted);
            }

            return true;
        }

        public override bool OnPlayerVotedSkip(ref PlayerState voter)
        {
            if (voter != deputy)
            {
                return false;
            }
            else
            {
                OnDeputyVote(null);
            }

            return true;
        }

        public override bool OnAllVotesCast()
        {
            if (deputy != null)
            {
                Role.AlterPlayerRole(defaultDeputyRole, deputy.PlayerId);
                deputy.NetworkName.Value = defaultDeputyName;
                deputy = null;
            }

            return true;
        }

        public void OnDeputyVote(PlayerState victim)
        {
            LassoPlayer(victim);
        }

        public void LassoPlayer(PlayerState playerToLasso)
        {
            if (playerToLasso != null)
            {
                foreach (PlayerState player in State.SpawnManager.PlayerStates)
                {
                    Vote.RPC_Vote(player.PlayerId, playerToLasso.PlayerId);
                }
            }
        }
    }
}