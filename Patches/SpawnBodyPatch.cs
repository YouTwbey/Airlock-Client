using AirlockClient.AC;
using AirlockAPI.Data;
using HarmonyLib;
using Il2CppFusion;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Network;
using UnityEngine;

namespace AirlockClient.Patches
{
    [HarmonyPatch(typeof(SpawnManager), nameof(SpawnManager.RPC_SpawnBodyByPlayerId))]
    public class SpawnBodyPatch
    {
        public static bool Prefix(PlayerRef id, NetworkRigidbodyObsolete rb)
        {
            if (CurrentMode.IsHosting && !CurrentMode.Modded)
            {
                if (!AntiCheat.Instance.VerifySpawnBody(GameObject.Find("PlayerState (" + id.PlayerId + ")").GetComponent<PlayerState>(), rb, RpcData.Info))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
