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
    /// <summary>
    /// Imposter Role
    /// The trapper can trap one body per round (resets after each meeting).
    /// Trapped Bodies can be reported but kills the reporter
    /// </summary>
    public class Trapper : SubRole
    {
        public static SubRoleData Data = new SubRoleData
        {
            Name = "Trapper (Point)",
            RoleType = "Imposter",
            Description = "Trap Bodies",
            AC_Description = "As the trapper, you have the ability to point at bodies and make it so if a crewmember tries to report it, said crewmember dies.",
            Team = GameTeam.Impostor,
            Amount = 0
        };

        void Start()
        {
            MelonCoroutines.Start(MoreRolesManager.DisplayRoleInfo(PlayerWithRole, this, Data));
        }
        NetworkedKillBehaviour killing;
        AirlockPeer Peer;
        PlayerState trappedbody = null;
        bool cantrapbody = true;
        public override void OnVotingBegan(PlayerState bodyReported, PlayerState reportingPlayer)
        {
            if (trappedbody != null && bodyReported != null)
            { 
                if (bodyReported == trappedbody)
                {
                    AntiCheat.KillPlayerWithAntiCheat(PlayerWithRole, reportingPlayer);
                    trappedbody = null;
                }
            }
        }
        public override void OnPlayerInput(XRRigInput input)
        {
            if ((PlayerWithRole.LocomotionPlayer._prevLeftHandPose == HandPoses.Point || PlayerWithRole.LocomotionPlayer._prevRightHandPose == HandPoses.Point || PlayerWithRole.LocomotionPlayer._previousBool == "Gesture_Point") && PlayerWithRole.IsAlive && cantrapbody)
            {
                foreach (NetworkedBody body in FindObjectsOfType<NetworkedBody>())
                {
                    if (body != null)
                    {
                        if ((body.transform.position - PlayerWithRole.LocomotionPlayer.RigidbodyPosition).magnitude <= 2 && cantrapbody && body.IsActive)
                        {
                            trappedbody = body._playerState;
                            cantrapbody = false;
                        }
                    }
                }
            }

            if (!ModdedGameStateManager.Instance.state.InTaskState())
            {
                cantrapbody = true;
                trappedbody = null;
            }
        }
    }
}
