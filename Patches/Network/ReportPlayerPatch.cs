using System.Runtime.CompilerServices;
using AirlockClient.AC;
using AirlockClient.Utils;
using HarmonyLib;
using SG.Airlock.Network;
using SG.Airlock.UI.Moderation;
using UnityEngine.Playables;

namespace AirlockClient.Patches;

[HarmonyPatch(typeof(ReportPlayerPanel),nameof(ReportPlayerPanel.SubmitReport))]
public class ReportPlayerPatch
{
    [HarmonyPostfix]
    public static void Postfix(ReportPlayerPanel __instance)
    {
        var player = __instance._playerState;
        AntiCheat.SendReportToDevelopers(player, player.GetActualModId(), "Reported By Host");
    }
}