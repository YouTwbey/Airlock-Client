using AirlockClient.Attributes;
using AirlockClient.Managers;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Roles;
using MelonLoader;
using Il2CppSG.Airlock.XR;
using Il2CppSG.Airlock.Network;
using AirlockClient.Managers.Gamemode;
using UnityEngine;

namespace AirlockClient.Data.Roles.MoreRoles.Crewmate
{
    /// <summary>
    /// A crewmate role than can protect players when they die.
    /// </summary>
    public class GuardianAngel : SubRole
    {
        public static SubRoleData Data = new SubRoleData
        {
            Name = "Guardian Angel",
            Description = "Protect crew",
            AC_Description = "When dead, you have the ability to protect the crew by pointing when near them.",
            Team = GameTeam.Crewmember,
            Amount = 1
        };

        void Start()
        {
            MelonCoroutines.Start(MoreRolesManager.DisplayRoleInfo(PlayerWithRole, this, Data, "Right Hand Point"));
        }

        bool wasGivenGuardian;

        public override void OnPlayerInput(XRRigInput input)
        {
            if (!PlayerWithRole.IsAlive && ModdedGameStateManager.Instance.state.InTaskState() && PlayerWithRole.ActivePowerUps != PowerUps.Guard && !wasGivenGuardian)
            {
                PlayerWithRole.ActivePowerUps = PowerUps.Guard;

                if (PlayerWithRole.ActivePowerUps == PowerUps.Guard)
                {
                    wasGivenGuardian = true;
                }
            }

            if (!PlayerWithRole.IsAlive && ModdedGameStateManager.Instance.state.InTaskState() && PlayerWithRole.ActivePowerUps == PowerUps.Guard)
            {
                if ((PlayerWithRole.LocomotionPlayer._prevLeftHandPose == HandPoses.Point || PlayerWithRole.LocomotionPlayer._prevRightHandPose == HandPoses.Point || input.handPoses == new Vector2Int(1, 2)))
                {
                    PlayerState closestPlayer = null;
                    float closest = float.MaxValue;

                    foreach (NetworkedLocomotionPlayer player in FindObjectsOfType<NetworkedLocomotionPlayer>())
                    {
                        if (player != null)
                        {
                            if (player.PState)
                            {
                                if (player.PState.IsConnected && player.PState.IsAlive)
                                {
                                    if ((player.RigidbodyPosition - PlayerWithRole.LocomotionPlayer.RigidbodyPosition).magnitude <= closest)
                                    {
                                        closestPlayer = player.PState;
                                        closest = (player.RigidbodyPosition - PlayerWithRole.LocomotionPlayer.RigidbodyPosition).magnitude;
                                    }
                                }
                            }
                        }
                    }

                    if (closestPlayer != null)
                    {
                        closestPlayer.Guarded = true;
                        PlayerWithRole.ActivePowerUps = PowerUps.None;
                    }
                }
            }
        }

        public override void OnPlayerEjected(PlayerState ejectedPlayer, GameRole role)
        {
            wasGivenGuardian = false;
        }
    }
}
