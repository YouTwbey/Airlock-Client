using AirlockClient.Attributes;
using AirlockClient.Managers;
using AirlockClient.Managers.Gamemode;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Roles;
using Il2CppSG.Airlock.XR;
using MelonLoader;
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
            AC_Description = "Pointing near a body will remove it. You can only remove one body every time a meeting ends.",
            Team = GameTeam.Imposter,
            Amount = 0
        };

        void Start()
        {
            MelonCoroutines.Start(MoreRolesManager.DisplayRoleInfo(PlayerWithRole, this, Data));
        }


        bool canHideBody = true;
        public override void OnPlayerInput(XRRigInput input)
        {
            if ((PlayerWithRole.LocomotionPlayer._prevLeftHandPose == HandPoses.Point || PlayerWithRole.LocomotionPlayer._prevRightHandPose == HandPoses.Point || input.handPoses == new Vector2Int(1, 2)) && PlayerWithRole.IsAlive && canHideBody)
            {
                foreach (NetworkedBody body in FindObjectsOfType<NetworkedBody>())
                {
                    if (body != null)
                    {
                        if ((body.transform.position - PlayerWithRole.LocomotionPlayer.RigidbodyPosition).magnitude <= 2 && canHideBody)
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
