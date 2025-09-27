using AirlockClient.AC;
using AirlockClient.Attributes;
using AirlockClient.Managers;
using AirlockClient.Managers.Gamemode;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Network;
using Il2CppSG.Airlock.Roles;
using Il2CppSG.Airlock.XR;
using MelonLoader;
using System.Collections.Generic;

namespace AirlockClient.Data.Roles.MoreRoles.Neutral
{
    public class Arsonist : SubRole
    {
        public static SubRoleData Data = new SubRoleData
        {
            Name = "Arsonist",
            RoleType = "Neutral",
            Description = "Douse Everyone",
            AC_Description = "Douse everyone then (HandPoses.Thumbsup) to win",
            Team = GameTeam.Crewmember,
            Amount = 0
        };

        void Start()
        {
            MelonCoroutines.Start(MoreRolesManager.DisplayRoleInfo(PlayerWithRole, this, Data, "", GameRole.Vigilante));
        }

        public Dictionary<PlayerState, string> dousedPlayers = new Dictionary<PlayerState, string>();

        public override void OnPlayerKilled(PlayerState playerKilled)
        {
            AntiCheat.DousePlayerWithAntiCheat(this, playerKilled);
        }

        public override void OnPlayerInput(XRRigInput input)
        {
            if ((PlayerWithRole.LocomotionPlayer._prevLeftHandPose == HandPoses.ThumbsUp || PlayerWithRole.LocomotionPlayer._prevRightHandPose == HandPoses.ThumbsUp || PlayerWithRole.LocomotionPlayer._previousBool == "Gesture_ThumbsUp") && PlayerWithRole.IsAlive)
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
                if (EveryoneDoused)
                {
                    ModdedGameStateManager.Instance.QueueWin(PlayerWithRole, FindObjectOfType<GameStateManager>().NoImpostorsLeftWin, GameplayStates.Task, 1);
                }
            }
        }
    }
}
