using AirlockClient.Attributes;
using AirlockClient.Managers;
using AirlockClient.Managers.Gamemode;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Network;
using Il2CppSG.Airlock.Roles;
using Il2CppSG.Airlock.XR;
using MelonLoader;

namespace AirlockClient.Data.Roles.MoreRoles.Neutral
{
    /// <summary>
    /// Neutral Role
    /// As the vulture you must eat X bodies total to win
    /// </summary>
    /// x = chosen by host
    public class Vulture : SubRole
    {
        public GameStateManager state;
        public NetworkedKillBehaviour killing;

        public static SubRoleData Data = new SubRoleData
        {
            Name = "Vulture",
            RoleType = "Neutral",
            Description = "Eat Corpses",
            AC_Description = $"Point Near a dead body makes you eat said body, you must eat {MoreRolesManager.MaxBodiesEatenCount} bodies to win",
            Team = GameTeam.Crewmember,
            Amount = 0
        };

        void Start()
        {
            killing = FindObjectOfType<NetworkedKillBehaviour>();
            state = FindObjectOfType<GameStateManager>();
            MoreRolesManager.QueueRoleDisplay(PlayerWithRole, this, Data);
        }

        bool eatbody = true;
        int bodiesEaten = 0;
        public override void OnPlayerInput(XRRigInput input)
        {
            if ((PlayerWithRole.LocomotionPlayer._prevLeftHandPose == HandPoses.Point || PlayerWithRole.LocomotionPlayer._prevRightHandPose == HandPoses.Point || PlayerWithRole.LocomotionPlayer._previousBool == "Gesture_Point") && PlayerWithRole.IsAlive && eatbody)
            {
                foreach (NetworkedBody body in FindObjectsOfType<NetworkedBody>())
                {
                    if (body != null)
                    {
                        if ((body.transform.position - PlayerWithRole.LocomotionPlayer.RigidbodyPosition).magnitude <= 3 && eatbody && body.IsActive)
                        {
                            body.RPC_ToggleBody(false);
                            bodiesEaten += 1;
                        }
                    }
                }
            }
            if (bodiesEaten >= MoreRolesManager.MaxBodiesEatenCount)
            {
                killing.AlterRole(GameRole.Sheriff, PlayerWithRole.PlayerId, 0);
                state.EndGame(GameTeam.Other);
                bodiesEaten = 0;
            }
        }
    }
}
