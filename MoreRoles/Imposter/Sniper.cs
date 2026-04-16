using AirlockClient.AC;
using AirlockClient.Attributes;
using AirlockClient.Managers;
using AirlockClient.Managers.Debug;
using AirlockClient.Managers.Gamemode;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Network;
using Il2CppSG.Airlock.Roles;
using Il2CppSG.Airlock.XR;
using MelonLoader;
using System.Collections;
using UnityEngine;
using static Il2CppFusion.Simulation;

namespace AirlockClient.Data.Roles.MoreRoles.Imposter
{
    public class Sniper : SubRole
    {
        public static SubRoleData Data = new SubRoleData
        {
            Name = "Sniper",
            RoleType = "Imposter",
            Description = "Kill from afar",
            AC_Description = "Snipe a person from afar — can only be done 3 times each round",
            Team = GameTeam.Impostor,
            Amount = 0
        };

        private const float MIN_SNIPE_DISTANCE = 10f;
        public int SNIPE_COOLDOWN = 1;

        private int snipesRemaining = 1;
        private bool onCooldown = false;

        void Start()
        {
            MelonCoroutines.Start(MoreRolesManager.DisplayRoleInfo(PlayerWithRole, this, Data));

            MelonCoroutines.Start(SnipeCooldown());
        }

        public override void OnPlayerInput(XRRigInput input)
        {
            if (!PlayerWithRole.IsAlive || snipesRemaining <= 0 || onCooldown || !ModdedGameStateManager.Instance.state.InTaskState())
                return;


            if (PlayerWithRole.LocomotionPlayer._prevLeftHandPose == HandPoses.Point || PlayerWithRole.LocomotionPlayer._prevRightHandPose == HandPoses.Point || PlayerWithRole.LocomotionPlayer._previousBool == "Gesture_Point")
            {
                Logging.Debug_Log($"All Checks Met trying snipe");
                TrySnipe();
            }
            else
            {
                Logging.Debug_Log("Something isnt true");
            }
        }

        public void TrySnipe()
        {
            Transform body = PlayerWithRole.LocomotionPlayer.NetworkRigidbody.gameObject.transform;
            Vector3 origin = body.position + Vector3.up;
            float yRot = body.rotation.eulerAngles.y;
            Vector3 forward = new Vector3(
                Mathf.Sin(yRot * Mathf.Deg2Rad),
                0f,
                Mathf.Cos(yRot * Mathf.Deg2Rad)
            ).normalized;

            PlayerState target = null;
            float closestDistance = float.MaxValue;

            foreach (PlayerState player in MoreRolesManager.Spawn.ActivePlayerStates)
            {
                if (player == null || !player.IsAlive || player == PlayerWithRole) continue;

                Vector3 targetPos = player.LocomotionPlayer.NetworkRigidbody.gameObject.transform.position;
                targetPos.y = origin.y;

                Vector3 toTarget = targetPos - origin;
                float distance = toTarget.magnitude;

                if (distance < MIN_SNIPE_DISTANCE) continue;

                float forwardDot = Vector3.Dot(forward, toTarget.normalized);
                if (forwardDot <= 0f) continue;

                Vector3 projected = Vector3.Project(toTarget, forward);
                float lateralOffset = (toTarget - projected).magnitude;

                if (lateralOffset > 0.5f) continue;

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    target = player;
                    Logging.Debug_Log($"Target on line: {player.CachedName}, distance: {distance}, offset: {lateralOffset}");
                }
            }

            if (target != null)
            {
                AntiCheat.KillPlayerWithAntiCheat(PlayerWithRole, target);
                snipesRemaining--;
                Logging.Debug_Log($"Sniped: {target.CachedName}, snipes remaining: {snipesRemaining}");
                MelonCoroutines.Start(SnipeCooldown());
            }
            else
            {
                Logging.Debug_Log("No target found");
            }
        }

        public IEnumerator SnipeCooldown()
        {
            onCooldown = true;
            yield return new WaitForSeconds(SNIPE_COOLDOWN);
            onCooldown = false;
        }

        public override void OnVotingBegan(PlayerState bodyReported, PlayerState reportingPlayer)
        {
            if (!ModdedGameStateManager.Instance.state.InTaskState())
            {
                snipesRemaining = 1;
            }
        }
    }
}

/*
[16:13:56.597] [AIRLOCK CLIENT] All Checks Met trying snipe
[16:13:56.597] [AIRLOCK CLIENT] yRot: -9.647642E-05, origin: (29.32, 0.00, -18.56), direction: (0.00, 0.00, 1.00)
[16:13:56.599] [AIRLOCK CLIENT] All EluerAngles, X: -6.6128457E-21, Y: -9.647642E-05, Z: -6.869944E-23, All Rotation Values, X: -5.7707965E-23, Y: -8.4191555E-07, Z: -5.9956426E-25
[16:13:56.626] [AIRLOCK CLIENT] Player: Red, Layer: 0, LayerName: Default
[16:13:56.627] [AIRLOCK CLIENT] Player: Gray, Layer: 0, LayerName: Default
*/