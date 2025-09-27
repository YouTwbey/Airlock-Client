using AirlockClient.Attributes;
using AirlockClient.Managers;
using AirlockClient.Managers.Gamemode;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Roles;
using Il2CppSG.Airlock.XR;
using MelonLoader;

namespace AirlockClient.Data.Roles.MoreRoles.Neutral
{
    /// <summary>
    /// Neutral Role
    /// As the vulture you must eat 4 bodies total to win
    /// </summary>
    public class Vulture : SubRole
    {
        public static SubRoleData Data = new SubRoleData
        {
            Name = "Vulture (Point)",
            RoleType = "Neutral",
            Description = "Eat Corpses",
            AC_Description = "Point Near a dead body makes you eat said body, you must eat 4 bodies to win",
            Team = GameTeam.Crewmember,
            Amount = 0
        };

        void Start()
        {
            MelonCoroutines.Start(MoreRolesManager.DisplayRoleInfo(PlayerWithRole, this, Data));
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
                        if ((body.transform.position - PlayerWithRole.LocomotionPlayer.RigidbodyPosition).magnitude <= 2 && eatbody && body.IsActive)
                        {
                            body.RPC_ToggleBody(false);
                            bodiesEaten += 1;
                        }
                    }
                }
            }
            if (bodiesEaten >= 4)
            {
                ModdedGameStateManager.Instance.QueueWin(PlayerWithRole, FindObjectOfType<GameStateManager>().NoImpostorsLeftWin, GameplayStates.Task, 1);
                bodiesEaten = 0;
            }
        }
    }   
}
