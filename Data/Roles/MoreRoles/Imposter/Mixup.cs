using System;
using AirlockClient.Attributes;
using AirlockClient.Managers;
using AirlockClient.Managers.Gamemode;
using Il2CppSG.Airlock.Roles;
using Il2CppSG.Airlock.XR;
using MelonLoader;

namespace AirlockClient.Data.Roles.MoreRoles.Imposter
{
    public class Mixup : SubRole
    {
        private void Start()
        {
            MoreRolesManager.QueueRoleDisplay(PlayerWithRole, this, Data);
        }

        public override void OnPlayerInput(XRRigInput input)
        {
            if ((this.PlayerWithRole.LocomotionPlayer._prevLeftHandPose == HandPoses.Point || this.PlayerWithRole.LocomotionPlayer._prevRightHandPose == HandPoses.Point || this.PlayerWithRole.LocomotionPlayer._previousBool == "Gesture_Point") && this.mixup && ModdedGameStateManager.Instance.state.InTaskState())
            {
                MixupManager.TriggerMixUp(10f);
                this.mixup = false;
            }
            if (!ModdedGameStateManager.Instance.state.InTaskState())
            {
                this.mixup = true;
            }
        }

        public static SubRoleData Data = new SubRoleData
        {
            Name = "Mixup",
            RoleType = "Imposter",
            Description = "Change Looks",
            AC_Description = "You Can Mixup everything once per round, will reset after every meeting",
            Team = GameTeam.Impostor,
            Amount = 0
        };

        private bool mixup = true;
    }
}