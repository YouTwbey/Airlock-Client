using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Roles;
using Il2CppSG.Airlock.XR;
using System.Collections.Generic;
using UnityEngine;

namespace AirlockClient.Attributes
{
    public class SubRole : MonoBehaviour
    {
        public static List<SubRole> All = new List<SubRole>();
        public PlayerState PlayerWithRole;
        public bool IsDisplayingRole;

        void OnDestroy()
        {
            if (ModdedGamemode.Current)
            {
                ModdedGamemode.Current.AssignedRoles.Remove(PlayerWithRole);
            }

            OnRoleRemoved();
            All.Remove(this);
        }

        void Awake()
        {
            PlayerWithRole = GetComponent<PlayerState>();

            if (ModdedGamemode.Current)
            {
                ModdedGamemode.Current.AssignedRoles.Add(PlayerWithRole, this);
            }

            OnPlayerRecievedRole();
            All.Add(this);

            if (PlayerWithRole == null)
            {
                Destroy(this);
            }
            if (!PlayerWithRole.IsConnected)
            {
                Destroy(this);
            }
        }

        void LateUpdate()
        {
            if (PlayerWithRole.IsConnected)
            {
                OnPlayerInput(PlayerWithRole.LocomotionPlayer.input);
            }
            else
            {
                Destroy(this);
            }
        }

        public virtual void OnPlayerRecievedRole() { }
        public virtual void OnPlayerAction(int action) { }
        public virtual void OnPlayerKilled(PlayerState playerKilled) { }
        public virtual void OnPlayerInput(XRRigInput input) { }
        public virtual void OnPlayerDied(PlayerState killer) { }
        public virtual void OnPlayerVoted(PlayerState votedPlayer) { }
        public virtual void OnPlayerVotedSkip() { }
        public virtual void OnPlayerEjected(PlayerState ejectedPlayer, GameRole role) { }
        public virtual void OnVotingBegan(PlayerState bodyReported, PlayerState reportingPlayer) { }
        public virtual void OnPlayerReportedBody(PlayerState bodyReported) { }
        public virtual void OnPlayerCalledMeeting() { }
        public virtual void OnAllVotesCast() { }
        public virtual void OnRoleRemoved() { }
        public virtual void OnGameEnd(GameTeam teamThatWon) { }
    }
}
