using AirlockClient.AC;
using AirlockClient.Attributes;
using AirlockClient.Managers;
using AirlockClient.Managers.Gamemode;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Network;
using Il2CppSG.Airlock.Roles;
using Il2CppSG.Airlock.XR;
using MelonLoader;
namespace AirlockClient.Data.Roles.MoreRoles.Imposter
{
    public class Sniper : SubRole
    {
        public static SubRoleData Data = new SubRoleData
        {
            Name = "Sniper (Point)",
            RoleType = "Imposter",
            Description = "Kill from afar",
            AC_Description = "Snipe a person from afar can only be done 3 times each round",
            Team = GameTeam.Imposter,
            Amount = 0
        };
        void Start()
        {
            MelonCoroutines.Start(MoreRolesManager.DisplayRoleInfo(PlayerWithRole, this, Data));   
        }
        int snipePlayer = 3;

        public override void OnPlayerInput(XRRigInput input)
        {
            if ((PlayerWithRole.LocomotionPlayer._prevLeftHandPose == HandPoses.Point || PlayerWithRole.LocomotionPlayer._prevRightHandPose == HandPoses.Point || PlayerWithRole.LocomotionPlayer._previousBool == "Gesture_Point") && PlayerWithRole.IsAlive && snipePlayer != 0)
            {
                foreach (PlayerState player in  FindObjectOfType<SpawnManager>().ActivePlayerStates)
                {
                    if (player.IsAlive && player != null)
                    {
                       if ((player.transform.position - PlayerWithRole.LocomotionPlayer.RigidbodyPosition).magnitude <= 15 && snipePlayer != 0 )
                       {
                            AntiCheat.KillPlayerWithAntiCheat(PlayerWithRole, player);
                            snipePlayer -= 1;
                        }
                    }
                }
            }

            if (!ModdedGameStateManager.Instance.state.InTaskState())
            {
                snipePlayer = 3;
            }
        }
    }
}
