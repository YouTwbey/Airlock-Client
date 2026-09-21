using HarmonyLib;
using SG.Airlock.Network;
using UnityEngine;

namespace AirlockClient.Patches;

[HarmonyPatch(typeof(ModerationManager),nameof(ModerationManager.RPC_KickVote))]
public class KickPatch
{
    private static int calls;
    private static float windowStart = -1f;
    private const float WindowSeconds = 0.2f;

    [HarmonyPrefix]
    public static bool Prefix()
    {
        var now = Time.time;

        if (windowStart < 0f || now - windowStart >= WindowSeconds)
        {
            windowStart = now;
            calls = 0;
        }

        calls++;

        return calls < 3;
    }
}