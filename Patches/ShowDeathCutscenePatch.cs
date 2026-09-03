using System.Linq;
using AirlockClient.Attributes;
using HarmonyLib;
using Fusion;
using SG.Airlock;
using UnityEngine;

namespace AirlockClient.Patches
{
    [HarmonyPatch(typeof(PlayerState), nameof(PlayerState.RPC_ShowDeathAnim))]
    internal class ShowDeathCutscenePatch
    {
        public static void Prefix(PlayerState __instance, PlayerRef victim, PlayerRef killer, bool wasVigilanteKill)
        {
            foreach (var role in SubRole.All.Where(role => role.PlayerWithRole.PlayerId == victim))
            {
                role.OnPlayerDied(GameObject.Find("PlayerState (" + killer.PlayerId + ")").GetComponent<PlayerState>());
            }
        }
    }
}
