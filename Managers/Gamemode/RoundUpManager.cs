using AirlockClient.Attributes;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Roles;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace AirlockClient.Managers.Gamemode
{
    public class RoundUpManager : ModdedGamemode
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

        public override void OnVotingBegan(PlayerState bodyReported, PlayerState reportingPlayer)
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
        }

        public override void OnAllVotesCast()
        {
            if (deputy != null)
            {
                Role.AlterPlayerRole(defaultDeputyRole, deputy.PlayerId);
                deputy.NetworkName.Value = defaultDeputyName;
                deputy = null;
            }
        }

        public void OnDeputyVote(PlayerState victim)
        {
            LassoPlayer(victim);
        }

        public void LassoPlayer(PlayerState playerToLasso)
        {
            foreach (PlayerState player in State.SpawnManager.PlayerStates)
            {
                Vote.RPC_Vote(player.PlayerId, playerToLasso.PlayerId);
            }
        }
    }
}