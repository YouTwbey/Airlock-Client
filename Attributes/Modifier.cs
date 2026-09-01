using AirlockAPI.Handlers;
using SG.Airlock;
using SG.Airlock.Minigames;
using SG.Airlock.Network;
using SG.Airlock.Roles;
using SG.Airlock.Venting;
using SG.Airlock.XR;
using SG.GlobalEvents;
using System.Collections.Generic;
using UnityEngine;

namespace AirlockClient.Attributes
{
    public class Modifier : MonoBehaviour
    {
        public static List<Modifier> All = new List<Modifier>();
        public PlayerState PlayerWithModifier;
        public bool IsDisplayingRole;
        public bool ModifierName;

        void OnDestroy()
        {
            if (AirlockClientGamemode.Get())
            {
                if (AirlockClientGamemode.Get().AssignedModifiers.ContainsKey(PlayerWithModifier))
                {
                    AirlockClientGamemode.Get().AssignedModifiers.Remove(PlayerWithModifier);
                }
            }

            OnModifierRemoved();
            All.Remove(this);
        }

        void Awake()
        {
            PlayerWithModifier = GetComponent<PlayerState>();

            if (AirlockClientGamemode.Get())
            {
                if (AirlockClientGamemode.Get().AssignedModifiers.ContainsKey(PlayerWithModifier))
                {
                    AirlockClientGamemode.Get().AssignedModifiers.Remove(PlayerWithModifier);
                }

                AirlockClientGamemode.Get().AssignedModifiers.Add(PlayerWithModifier, this);
            }

            OnPlayerRecievedRole();
            All.Add(this);

            if (PlayerWithModifier == null)
            {
                Destroy(this);
            }
            if (!PlayerWithModifier.IsConnected)
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
        public virtual void OnModifierRemoved() { }
        public virtual void OnGameEnd(GameTeam teamThatWon) { }
        public virtual void OnTaskComplete(MinigameManager minigamemlayer) { }
        public virtual void OnSpellCast(PlayerState player) { }
    }
}
