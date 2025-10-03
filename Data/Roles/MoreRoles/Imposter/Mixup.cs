using AirlockClient.AC;
using AirlockClient.Attributes;
using AirlockClient.Managers;
using AirlockClient.Managers.Debug;
using AirlockClient.Managers.Gamemode;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Roles;
using Il2CppSG.Airlock.XR;
using MelonLoader;
using System.Collections.Generic;
using UnityEngine;
using AirlockClient.Patches;

namespace AirlockClient.Data.Roles.MoreRoles.Imposter
{
    /// <summary>
    /// Mixup
    /// Imposter Experimental Role
    /// Once per round you can change everybody's Looks and names, resets after every meeting. This Role is highly experimental.
    /// </summary>
    public class Mixup : SubRole
    {
        public static SubRoleData Data = new SubRoleData
        {
            Name = "Mixup (Point)",
            RoleType = "Imposter",
            Description = "Change Looks",
            AC_Description = "You Can Mixup everything once per round, will reset after every meeting",
            Team = GameTeam.Impostor,
            Amount = 0
        };

        void Start()
        {
            MelonCoroutines.Start(MoreRolesManager.DisplayRoleInfo(PlayerWithRole, this, Data));
        }


        bool mixup = true;
        public override void OnPlayerInput(XRRigInput input)
        {
            if ((PlayerWithRole.LocomotionPlayer._prevLeftHandPose == HandPoses.Point || PlayerWithRole.LocomotionPlayer._prevRightHandPose == HandPoses.Point || PlayerWithRole.LocomotionPlayer._previousBool == "Gesture_Point") && mixup && ModdedGameStateManager.Instance.state.InTaskState())
            {
                MixupManager.TriggerMixUp(10);
                mixup = false;
            }

            if (!ModdedGameStateManager.Instance.state.InTaskState())
            {
                mixup = true;
            }
        }
    }
}
