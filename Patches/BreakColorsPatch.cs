using HarmonyLib;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Network;
using MelonLoader;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AirlockClient.Patches
{
    internal static class PlayerSavedState
    {
        private static readonly Dictionary<int, (int color, int hat)> _store = new();

        public static void SetColor(int id, int color)
        {
            if (color == 18 || color == 0) return;
            _store.TryGetValue(id, out var current);
            _store[id] = (color, current.hat);
        }

        public static void SetHat(int id, int hat)
        {
            if (hat == 18) return;
            _store.TryGetValue(id, out var current);
            _store[id] = (current.color, hat);
        }

        public static void Init(int id, int color, int hat)
        {
            if (!_store.ContainsKey(id))
                _store[id] = (color, hat);
        }

        public static bool TryGet(int id, out int color, out int hat)
        {
            if (_store.TryGetValue(id, out var val))
            {
                color = val.color;
                hat = val.hat;
                return true;
            }
            color = hat = 0;
            return false;
        }
    }

    [HarmonyPatch(typeof(NetworkedLocomotionPlayer), nameof(NetworkedLocomotionPlayer.RPC_OnEnterCustomization))]
    public class BreakColorsPatch
    {
        internal static readonly Dictionary<int, int> StateIdToPlayerId = new();

        public static void Postfix(NetworkedLocomotionPlayer __instance)
        {
            StateIdToPlayerId[__instance._playerState.PlayerId] = __instance.PlayerID;
            PlayerSavedState.Init(__instance.PlayerID, __instance._playerState.ColorId, __instance._playerState.HatId);

            if (__instance._playerState.ColorId != 18)
                __instance._playerState.ColorId = 18;
        }
    }

    [HarmonyPatch(typeof(NetworkedLocomotionPlayer), nameof(NetworkedLocomotionPlayer.RPC_OnExitCustomization))]
    public class BreakColorsPatch2
    {
        public static void Prefix(NetworkedLocomotionPlayer __instance)
        {
            __instance.PState.ColorId = 18;

            if (PlayerSavedState.TryGet(__instance.PlayerID, out int color, out int hat))
                MelonCoroutines.Start(RestoreAfterDelay(__instance, color, hat));
        }

        private static IEnumerator RestoreAfterDelay(NetworkedLocomotionPlayer player, int color, int hat)
        {
            yield return new WaitForSeconds(5f);

            BreakColorsPatch.StateIdToPlayerId.Remove(player._playerState.PlayerId);
            player._playerState.ColorId = color;

            yield return new WaitForSeconds(2f);

            player._playerState.HatId = hat;
        }
    }

    [HarmonyPatch(typeof(PlayerState), nameof(PlayerState.UpdateColorID))]
    public class BreakColorsPatch4
    {
        public static void Prefix(PlayerState __instance, ref int i)
        {
            if (i == 18) return;

            if (BreakColorsPatch.StateIdToPlayerId.TryGetValue(__instance.PlayerId, out int playerId))
                PlayerSavedState.SetColor(playerId, i);
        }
    }

    [HarmonyPatch(typeof(NetworkedLocomotionPlayer), nameof(NetworkedLocomotionPlayer.OnHatChange))]
    public class BreakColorsPatch5
    {
        public static void Prefix(NetworkedLocomotionPlayer __instance, ref int hatId)
        {
            if (hatId == 18) return;

            PlayerSavedState.SetHat(__instance.PlayerID, hatId);
        }
    }
    [HarmonyPatch(typeof(NetworkedLocomotionPlayer),nameof(NetworkedLocomotionPlayer.RPC_SpawnInitialization))]
    public class savecosmetics
    {
        public static void Postfix(NetworkedLocomotionPlayer __instance, ref int color, ref int hat, ref int hands, ref int skin)
        {
            PlayerSavedState.SetHat(__instance.PlayerID, hat);
            PlayerSavedState.SetColor(__instance.PlayerID, color);
        }
    }
}