using Il2CppSG.Airlock.Roles;
using Il2CppSystem.Collections.Generic;
using Il2CppSG.Airlock;
using UnityEngine;

namespace AirlockClient.Attributes
{
    public class ModdedGamemode : MonoBehaviour
    {
        public static ModdedGamemode Current;
        public Dictionary<PlayerState, SubRole> AssignedRoles = new Dictionary<PlayerState, SubRole>();
        public GameStateManager State;
        public RoleManager Role;
        public VoteManager Vote;

        void Awake()
        {
            if (Current == null)
            {
                Current = this;
                Current.State = FindObjectOfType<GameStateManager>();
                Current.Role = FindObjectOfType<RoleManager>();
                Current.Vote = FindObjectOfType<VoteManager>();
            }
            else
            {
                Destroy(this);
            }
        }

        public virtual void OnKill(PlayerState killer, PlayerState victim, int action) { }
        public virtual void OnPlayerVoted(PlayerState voter, PlayerState voted) { }
        public virtual void OnPlayerVotedSkip(PlayerState voter) { }
        public virtual void OnPlayerEjected(PlayerState ejectedPlayer, GameRole role) { }
        public virtual void OnVotingBegan(PlayerState bodyReported, PlayerState reportingPlayer) { }
        public virtual void OnAllVotesCast() { }
        public virtual void OnGameStart() { }
        public virtual void OnAssignRoles() { }
        public virtual void OnGameEnd(GameTeam teamThatWon) { }

        public GameRole GetTrueRole(PlayerState player)
        {
            foreach (KeyValuePair<GameRole, List<int>> roleEntry in Current.Role.gameRoleToPlayerIds)
            {
                foreach (int id in roleEntry.Value)
                {
                    if (player.IsConnected)
                    {
                        if (id == player.PlayerId)
                        {
                            return roleEntry.Key;
                        }
                    }
                }
            }

            return GameRole.NotSet;
        }
    }
}
