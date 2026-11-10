using AirlockClient.Attributes;
using AirlockClient.Handlers;
using AirlockClient.Managers;
using AirlockClient.Managers.Debug;
using AirlockClient.Managers.Gamemode;
using AirlockClient.Utils;
using SG.Airlock;
using SG.Airlock.Network;
using SG.Airlock.Roles;
using SG.Airlock.XR;
using UnityEngine;

namespace AirlockClient.Data.Roles.MoreRoles.Broken;

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

    private const float AimRadius = 0.5f;
    private const float ArmDelay = 60f;
    private const float MaxAngleDeg = 5f;
    LayerMask worldMask;

    int shotsLeft = 3;
    bool wasPointing;
    Transform rightFingerJoint1;
    Transform leftFingerJoint1;
    Transform rightFingerJoint3;
    Transform leftFingerJoint3;

    void Start()
    {
        CoroutineHandler.Start(MoreRolesManager.DisplayRoleInfo(PlayerWithRole, this, Data));
        worldMask = 1 << 0;

        var loco = PlayerWithRole.LocomotionPlayer;
        rightFingerJoint1 = FindDeepChild(loco.rightHandTransform.transform, "rightFinger1_Joint1");
        leftFingerJoint1  = FindDeepChild(loco.leftHandTransform.transform, "leftFinger1_Joint1");
        rightFingerJoint3 = FindDeepChild(loco.rightHandTransform.transform, "rightFinger1_Joint3");
        leftFingerJoint3  = FindDeepChild(loco.leftHandTransform.transform, "leftFinger1_Joint3");
        Logging.Log($"[Sniper] joint1 refs — right={rightFingerJoint1} left={leftFingerJoint1}");
    }

    Transform FindDeepChild(Transform parent, string name)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name == name) return child;

            var found = FindDeepChild(child, name);
            if (found != null) return found;
        }
        return null;
    }
    // this shit is so ass lmfao
    public override void OnPlayerInput(XRRigInput input)
    {
        if (shotsLeft <= 0 || !PlayerWithRole.IsAlive
                           || !ModdedGameStateManager.Instance.state.InTaskState())
        {
            wasPointing = false;
            if (aimLine != null) aimLine.enabled = false;
            return;
        }

        bool pointing = GetPointerRay(out Vector3 origin, out Vector3 dir);

        if (pointing)
        {
            aimLine.enabled = true;
            aimLine.SetPosition(0, origin);
            aimLine.SetPosition(1, origin + dir * 30f);
        }
        else
        {
            aimLine.enabled = false;
        }

        if (pointing && !wasPointing)
        {
            PlayerState target = AcquireTarget(origin, dir);
            if (target != null)
            {
                shotsLeft -= 1;
                PlayerWithRole.KillPlayerWithAntiCheat(target);
            }
        }

        wasPointing = pointing;
    }

    bool GetPointerJoint(out Transform j)
    {
        var loco = PlayerWithRole.LocomotionPlayer;
        j = null;

        if (loco._prevLeftHandPose == HandPoses.Point) j = leftFingerJoint1;
        else if (loco._prevRightHandPose == HandPoses.Point || loco._previousBool == "Gesture_Point") j = rightFingerJoint1;

        return j != null;
    }

    void DiagnoseAxes(Vector3 origin, Transform pointerJoint)
    {
        foreach (var p in StaticRefs.Spawn.ActivePlayerStates)
        {
            if (p == PlayerWithRole || !p.IsAlive) continue;
            var loco = p.LocomotionPlayer;
            if (loco == null) continue;
            var netRb = loco.NetworkRigidbody;
            if (netRb == null) continue;

            var bodyPos = netRb.transform.position;
            var toTarget = (bodyPos - origin).normalized;
            var dist = (bodyPos - origin).magnitude;
            if (dist < 0.5f) continue;

            float aFwd    = Vector3.Angle(pointerJoint.forward, toTarget);
            float aRight  = Vector3.Angle(pointerJoint.right, toTarget);
            float aUp     = Vector3.Angle(pointerJoint.up, toTarget);
            float aNFwd   = Vector3.Angle(-pointerJoint.forward, toTarget);
            float aNRight = Vector3.Angle(-pointerJoint.right, toTarget);
            float aNUp    = Vector3.Angle(-pointerJoint.up, toTarget);

            Logging.Log($"[Sniper] AXIS CHECK vs {p.name} (dist={dist:F1}): " +
                        $"fwd={aFwd:F1} right={aRight:F1} up={aUp:F1} " +
                        $"-fwd={aNFwd:F1} -right={aNRight:F1} -up={aNUp:F1}  <-- lowest wins");
        }
    }
    
    bool GetPointerRay(out Vector3 origin, out Vector3 dir)
    {
        var loco = PlayerWithRole.LocomotionPlayer;
        Transform baseJ = null, tipJ = null;

        if (loco._prevLeftHandPose == HandPoses.Point) { baseJ = leftFingerJoint1; tipJ = leftFingerJoint3; }
        else if (loco._prevRightHandPose == HandPoses.Point || loco._previousBool == "Gesture_Point")
        { baseJ = rightFingerJoint1; tipJ = rightFingerJoint3; }

        if (baseJ == null || tipJ == null)
        {
            origin = default;
            dir = default;
            return false;
        }

        origin = tipJ.position;
        dir = (tipJ.position - baseJ.position).normalized;
        return true;
    }
        
    LineRenderer aimLine;

    void EnsureAimLine()
    {
        if (aimLine != null) return;

        var go = new GameObject("SniperAimLine");
        aimLine = go.AddComponent<LineRenderer>();
        aimLine.positionCount = 2;
        aimLine.startWidth = 0.01f;
        aimLine.endWidth = 0.01f;
        aimLine.material = new Material(Shader.Find("Sprites/Default"));
        aimLine.startColor = Color.red;
        aimLine.endColor = new Color(1f, 0f, 0f, 0.2f); // fades toward the far end
        aimLine.enabled = false;
    }

    PlayerState AcquireTarget(Vector3 origin, Vector3 dir)
    {
        PlayerState best = null;
        var bestAngle = MaxAngleDeg;

        foreach (var p in StaticRefs.Spawn.ActivePlayerStates)
        {
            if (p == PlayerWithRole || !p.IsAlive) continue;

            var loco = p.LocomotionPlayer;
            if (loco == null) continue;
            var netRb = loco.NetworkRigidbody;
            if (netRb == null) continue;

            var bodyPos = netRb.transform.position;
            var toTarget = bodyPos - origin;
            var dist = toTarget.magnitude;
            if (dist < 0.5f) continue;

            var angle = Vector3.Angle(dir, toTarget);
            if (angle >= bestAngle) continue;

            bool blocked = Physics.Raycast(origin, toTarget / dist, out RaycastHit hitInfo,
                dist - 0.25f, worldMask, QueryTriggerInteraction.Ignore);
            if (blocked) continue;

            bestAngle = angle;
            best = p;
        }

        return best;
    }

    public override void OnPlayerKilled(PlayerState playerKilled) { }
}