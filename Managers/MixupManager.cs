using AirlockClient.AC;
using AirlockClient.Managers;
using AirlockClient.Managers.Gamemode;
using Il2CppFusion.Protocol;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Network;
using Il2CppSG.Platform;
using MelonLoader;
using MelonLoader.TinyJSON;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Il2CppFusion.NetworkCharacterController;

namespace AirlockClient.Managers
{

    public class MixupManager
    {
        public class PlayerSnapshot
        {
            public PlayerState Player;
            public int HatId;
            public int HandsId;
            public int SkinId;
            public int ColorId;
            public string Name;
        }

        private IEnumerator RevertAfterSeconds(List<PlayerSnapshot> snapshots, float delay)
        {
            yield return new WaitForSeconds(delay);

            foreach (PlayerSnapshot snap in snapshots)
            {
                snap.Player.LocomotionPlayer.PlayPowerUpVFX(PowerUps.Vanish, true, true, false, false);

                snap.Player.ColorId = snap.Player.PlayerId + 20;
            }

            foreach (PlayerSnapshot snap in snapshots)
            {
                if (snap.Player != null)
                {
                    snap.Player.HatId = snap.HatId;
                    snap.Player.HandsId = snap.HandsId;
                    snap.Player.SkinId = snap.SkinId;
                    snap.Player.ColorId = snap.ColorId;
                    snap.Player.NetworkName = snap.Name;
                }
            }
        }

        public static void TriggerMixUp(float revertDelay)
        {
            SpawnManager spawnManager = ModdedGameStateManager.Instance.state.SpawnManager;
            if (spawnManager == null || spawnManager.ActivePlayerStates == null)
                return;

            List<PlayerSnapshot> snapshots = new List<PlayerSnapshot>();

            foreach (PlayerState player in spawnManager.ActivePlayerStates)
            {
                if (player == null || !player.IsAlive) continue;

                snapshots.Add(new PlayerSnapshot
                {
                    Player = player,
                    HatId = player.HatId,
                    HandsId = player.HandsId,
                    SkinId = player.SkinId,
                    Name = player.CachedName ?? "Player###"
                });

                player.ColorId = player.PlayerId + 20;
            }

            SwapAvatrs(snapshots);
            MelonCoroutines.Start(new MixupManager().RevertAfterSeconds(snapshots, revertDelay));
        }

        public static void SwapAvatrs(List<PlayerSnapshot> snapshots)
        {
            foreach (PlayerSnapshot snapshot in snapshots)
            {
                List<PlayerSnapshot> randomSnapshots = snapshots.ToArray().ToList();
                randomSnapshots.Remove(snapshot);

                PlayerSnapshot randomSnapshot = randomSnapshots[Random.Range(0, randomSnapshots.Count)];

                AntiCheat.ChangeHatWithAntiCheat(snapshot.Player, randomSnapshot.HatId);
                AntiCheat.ChangeSkinWithAntiCheat(snapshot.Player, randomSnapshot.SkinId);
                AntiCheat.ChangeGlovesWithAntiCheat(snapshot.Player, randomSnapshot.HandsId);
                snapshot.Player.ColorId = randomSnapshot.ColorId;
                snapshot.Player.NetworkName = randomSnapshot.Name;

                snapshot.Player.LocomotionPlayer.PlayPowerUpVFX(PowerUps.Vanish, true, true, false, false);
            }
        }
    }
}
