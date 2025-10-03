using AirlockClient.Attributes;
using AirlockClient.Managers.Gamemode;
using UnityEngine;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Network;
using Il2CppSG.Airlock.Roles;
using Il2CppSG.Airlock.XR;
using MelonLoader;
using AirlockClient.Managers;
using AirlockClient.AC;

namespace AirlockClient.Data.Roles.MoreRoles.Imposter
{
    /// <summary>
    /// An imposter role that allows the player explode killing others around them including themselves.
    /// </summary>
    public class Bomber : SubRole
    {
        public static SubRoleData Data = new SubRoleData
        {
            Name = "Bomber (Point)",
            RoleType = "Imposter",
            Description = "Explode others",
            AC_Description = "When pointing, you will kill yourself but others near you.",
            Team = GameTeam.Impostor,
            Amount = 0
        };

        void Start()
        {
            MelonCoroutines.Start(MoreRolesManager.DisplayRoleInfo(PlayerWithRole, this, Data));
        }

        bool canExplode = true;
        bool gameEnded = false;
        public override void OnPlayerEjected(PlayerState ejectedPlayer, GameRole role)
        {
            canExplode = true;
        }
        public override void OnGameEnd(GameTeam teamThatWon)
        {
            gameEnded = true;
        }

        public override void OnPlayerInput(XRRigInput input)
        {
            if ((PlayerWithRole.LocomotionPlayer._prevLeftHandPose == HandPoses.Point || PlayerWithRole.LocomotionPlayer._prevRightHandPose == HandPoses.Point || input.handPoses == new Vector2Int(1, 2)) && PlayerWithRole.IsAlive && canExplode && !gameEnded && ModdedGameStateManager.Instance.state.InTaskState())
            {
                foreach (NetworkedLocomotionPlayer player in FindObjectsOfType<NetworkedLocomotionPlayer>())
                {
                    if (player != null)
                    {
                        if ((player.RigidbodyPosition - PlayerWithRole.LocomotionPlayer.RigidbodyPosition).magnitude <= 5)
                        {
                            if (player.PState.IsAlive)
                            {
                                AntiCheat.KillPlayerWithAntiCheat(PlayerWithRole, player.PState);
                            }
                        }
                    }
                }

                AntiCheat.KillPlayerWithAntiCheat(PlayerWithRole, PlayerWithRole);

                canExplode = false;
            }
        }
    }
}
