using AirlockClient.Attributes;
using AirlockClient.Managers;
using AirlockClient.Managers.Gamemode;
using SG.Airlock;
using SG.Airlock.Roles;
using SG.Airlock.XR;

using UnityEngine;

namespace AirlockClient.Data.Roles.MoreRoles.Imposter
{
    /// <summary>
    /// An imposter role that allows the player to clean one body every time a meeting ends.
    /// </summary>
    public class Janitor : SubRole
    {
        public static SubRoleData Data = new SubRoleData
        {
            Name = "Janitor (Point)",
            RoleType = "Imposter",
            Description = "Remove Bodies",
            AC_Description = "Clean up a body, resets every meeting",
            AC_Color = new Color(255, 175, 84),
            Team = GameTeam.Impostor,
            Amount = 0
        };

        void Start()
        {
            MelonCoroutines.Start(MoreRolesManager.DisplayRoleInfo(PlayerWithRole, this, Data));
        }


        bool canHideBody = true;
        public override void OnPlayerInput(XRRigInput input)
        {
            if ((PlayerWithRole.LocomotionPlayer._prevLeftHandPose == HandPoses.Point || PlayerWithRole.LocomotionPlayer._prevRightHandPose == HandPoses.Point || PlayerWithRole.LocomotionPlayer._previousBool == "Gesture_Point") && PlayerWithRole.IsAlive && canHideBody)
            {
                foreach (NetworkedBody body in FindObjectsOfType<NetworkedBody>())
                {
                    if (body != null)
                    {
                        if ((body.transform.position - PlayerWithRole.LocomotionPlayer.RigidbodyPosition).magnitude <= 2 && canHideBody && body.IsActive)
                        {
                            body.RPC_ToggleBody(false);
                            canHideBody = false;
                        }
                    }
                }
            }
            
            if (!ModdedGameStateManager.Instance.state.InTaskState())
            {
                canHideBody = true;
            }
        }
    }
}
