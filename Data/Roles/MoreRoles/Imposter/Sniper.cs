using AirlockClient.AC;
using AirlockClient.Attributes;
using AirlockClient.Handlers;
using AirlockClient.Managers;
using AirlockClient.Managers.Gamemode;
using AirlockClient.Utils;
using SG.Airlock;
using SG.Airlock.Roles;
using SG.Airlock.XR;

namespace AirlockClient.Data.Roles.MoreRoles.Imposter
{
    public class Sniper : SubRole
    {
        public static SubRoleData Data = new SubRoleData
        {
            Name = "Sniper",
            RoleType = "Imposter",
            Description = "Kill from afar",
            AC_Description = "Snipe a person from afar",
            Team = GameTeam.Impostor,
            Amount = 0
        };


        void Start()
        {
			CoroutineHandler.Start(MoreRolesManager.DisplayRoleInfo(PlayerWithRole, this, Data));
        }

        PlayerState target;

        public override void OnPlayerInput(XRRigInput input)
        {
            if (!PlayerWithRole.IsAlive || !ModdedGameStateManager.Instance.state.InTaskState()) return;

            if (PlayerWithRole.LocomotionPlayer._prevLeftHandPose == HandPoses.Point || PlayerWithRole.LocomotionPlayer._prevRightHandPose == HandPoses.Point || PlayerWithRole.LocomotionPlayer._previousBool == "Gesture_Point")
            {
                if (target != null)
                {
                    PlayerWithRole.KillPlayerWithAntiCheat(target);
                }
            }
        }

        public override void OnPlayerKilled(PlayerState playerKilled)
        {
            target = playerKilled;
        }
    }
}