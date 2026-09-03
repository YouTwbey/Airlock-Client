using AirlockClient.AC;
using AirlockAPI.Data;
using AirlockClient.Utils;
using HarmonyLib;
using Fusion;
using SG.Airlock;
using SG.Airlock.Network;
using UnityEngine;

namespace AirlockClient.Patches
{
    [HarmonyPatch(typeof(SpawnManager), nameof(SpawnManager.RPC_SpawnBodyByPlayerId))]
    public class SpawnBodyPatch
    {
        public static bool Prefix(PlayerRef id, NetworkRigidbodyObsolete rb)
        {
            if (!CurrentMode.IsHosting || CurrentMode.Modded) return true;
            
            return GameObject.Find("PlayerState (" + id.PlayerId + ")").GetComponent<PlayerState>().VerifySpawnBody(rb);
        }
    }
}
