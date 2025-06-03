using AirlockClient.Attributes;
using HarmonyLib;
using Il2CppFusion;
using Il2CppSG.Airlock;
using UnityEngine;

namespace AirlockClient.Patches
{
    [HarmonyPatch(typeof(PlayerState), nameof(PlayerState.RPC_ShowDeathAnim))]
    internal class ShowDeathCutscenePatch
    {
        public static void Prefix(PlayerState __instance, PlayerRef victim, PlayerRef killer, bool wasVigilanteKill)
        {
            foreach (SubRole role in SubRole.All)
            {
                if (role.PlayerWithRole.PlayerId == victim)
                {
                    role.OnPlayerDied(GameObject.Find("PlayerState (" + killer.PlayerId + ")").GetComponent<PlayerState>());
                }
            }
        }
    }
}
