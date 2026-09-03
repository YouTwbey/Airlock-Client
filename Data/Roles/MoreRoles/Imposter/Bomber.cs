using AirlockClient.AC;
using AirlockClient.Attributes;
using AirlockClient.Handlers;
using AirlockClient.Managers;
using AirlockClient.Managers.Gamemode;
using SG.Airlock;
using SG.Airlock.Network;
using SG.Airlock.Roles;
using SG.Airlock.XR;
using System;
using System;
using System.Collections;
using AirlockClient.Utils;
using UnityEngine;

namespace AirlockClient.Data.Roles.MoreRoles.Imposter
{
    /// <summary>
    /// An imposter role that allows the player explode killing others around them including themselves.
    /// </summary>
    public class Bomber : SubRole
    {
        public static SubRoleData Data = new SubRoleData
        {
            Name = "Bomber",
            RoleType = "Imposter",
            Description = "Explode others",
            AC_Description = "When pointing, you will kill yourself but others near you.",
            Team = GameTeam.Impostor,
            Amount = 0
        };

        void Start()
        {
            MoreRolesManager.QueueRoleDisplay(PlayerWithRole, this, Data);
			CoroutineHandler.Start(BomberCooldown());
        }

        bool canExplode = false;
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
                                PlayerWithRole.KillPlayerWithAntiCheat(player.PState);
                            }
                        }
                    }
                }

                PlayerWithRole.KillPlayerWithAntiCheat(PlayerWithRole);

                canExplode = false;
            }
        }

        public IEnumerator BomberCooldown()
        {
            yield return new WaitForSeconds(MoreRolesManager.BomberCooldownVar + 10);
            canExplode = true;
        }
    }
}
