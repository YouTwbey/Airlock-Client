using System.Collections.Generic;
using AirlockClient.AC;
using AirlockClient.Attributes;
using AirlockClient.Managers;
using AirlockClient.Managers.Gamemode;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Network;
using Il2CppSG.Airlock.Roles;
using Il2CppSG.Airlock.XR;
using Il2CppSG.SoundCore;
using JetBrains.Annotations;
using MelonLoader;

namespace AirlockClient.Data.Roles.MoreRoles.Neutral
{
    public class Arsonist : SubRole
    {
        public GameStateManager state;
        public NetworkedKillBehaviour killing;

        public static SubRoleData Data = new SubRoleData
        {
            Name = "Arsonist",
            RoleType = "Neutral",
            Description = "Douse Everyone",
            AC_Description = "Douse everyone then (Thumbsup) to win",
            Team = GameTeam.Crewmember,
            Amount = 0
        };

        void Start()
        {
            killing = FindObjectOfType<NetworkedKillBehaviour>();
            state = FindObjectOfType<GameStateManager>();
            MoreRolesManager.QueueRoleDisplay(PlayerWithRole, this, Data, "", GameRole.Vigilante);
        }

        public Dictionary<PlayerState, string> dousedPlayers = new Dictionary<PlayerState, string>();

        public override void OnPlayerKilled(PlayerState playerKilled)
        {
            AntiCheat.DousePlayerWithAntiCheat(this, playerKilled);
        }

        public override void OnPlayerInput(XRRigInput input)
        {
            if ((PlayerWithRole.LocomotionPlayer._prevLeftHandPose == HandPoses.ThumbsUp || PlayerWithRole.LocomotionPlayer._prevRightHandPose == HandPoses.ThumbsUp || PlayerWithRole.LocomotionPlayer._previousBool == "Gesture_Thumbs_Up") && PlayerWithRole.IsAlive)
            {
                bool EveryoneDoused = true;
                foreach (PlayerState player in FindObjectOfType<SpawnManager>().ActivePlayerStates)
                {
                    if (player != PlayerWithRole && player.IsAlive)
                    {
                        if (!dousedPlayers.ContainsKey(player))
                        {
                            EveryoneDoused = false;
                            break;
                        }
                    }
                }
                if (EveryoneDoused && !state.InVotingState())
                {
                    killing.AlterRole(GameRole.Sheriff, PlayerWithRole.PlayerId, 0);
                    state.EndGame(GameTeam.Other);
                }
            }
        }

        public override void OnGameEnd(GameTeam teamThatWon)
        {
            dousedPlayers.Clear();
        }
    }
}