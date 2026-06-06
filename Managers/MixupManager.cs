using AirlockClient.Managers.Gamemode;
using AirlockClient.Patches;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Network;
using MelonLoader;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

        public static void TriggerMixUp(float revertDelay)
        {
            SpawnManager spawnManager = MoreRolesManager.Spawn;
            if (spawnManager == null || spawnManager.ActivePlayerStates == null)
                return;

            List<PlayerSnapshot> snapshots = new List<PlayerSnapshot>();

            foreach (PlayerState player in spawnManager.ActivePlayerStates)
            {
                if (player == null || !player.IsAlive) continue;

                PlayerSavedState.TryGet(player.PlayerId, out int savedColor, out int savedHat);

                snapshots.Add(new PlayerSnapshot
                {
                    Player = player,
                    HatId = savedHat != 0 ? savedHat : player.HatId,
                    HandsId = player.HandsId,
                    SkinId = player.SkinId,
                    ColorId = savedColor != 0 ? savedColor : player.ColorId,
                    Name = player.CachedName ?? "Player###"
                });
            }

            MelonCoroutines.Start(ApplyRandomCosmetics(snapshots));
            MelonCoroutines.Start(new MixupManager().RevertAfterSeconds(snapshots, revertDelay));
        }

        private static readonly Dictionary<int, int> _originalHats = new();

        private static IEnumerator ApplyRandomCosmetics(List<PlayerSnapshot> snapshots)
        {
            List<int> availableColors = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
            availableColors = availableColors.OrderBy(_ => Random.value).ToList();

            for (int i = 0; i < snapshots.Count; i++)
            {
                PlayerSnapshot snapshot = snapshots[i];

                _originalHats[snapshot.Player.PlayerId] = snapshot.HatId;

                var randomSkin = ListsManager.skins[Random.Range(0, ListsManager.skins.Count)];
                var randomHands = ListsManager.hands[Random.Range(0, ListsManager.hands.Count)];
                string randomName = ListsManager.Usernames[Random.Range(0, ListsManager.Usernames.Count)];
                int randomColor = availableColors[i % availableColors.Count];

                snapshot.Player.ColorId = randomColor;
                snapshot.Player.SkinId = randomSkin.id;
                snapshot.Player.HandsId = randomHands.id;
                snapshot.Player.SetNetworkName(randomName);
                snapshot.Player.LocomotionPlayer.PlayPowerUpVFX(PowerUps.Vanish, true, true, false, false);
            }

            yield return new WaitForSeconds(0.5f);

            foreach (PlayerSnapshot snapshot in snapshots)
            {
                if (snapshot.Player == null) continue;
                var randomHat = ListsManager.hats[Random.Range(0, ListsManager.hats.Count)];
                snapshot.Player.HatId = randomHat.id;
            }
        }

        private IEnumerator RevertAfterSeconds(List<PlayerSnapshot> snapshots, float delay)
        {
            yield return new WaitForSeconds(delay);

            foreach (PlayerSnapshot snap in snapshots)
            {
                if (snap.Player == null) continue;

                snap.Player.LocomotionPlayer.PlayPowerUpVFX(PowerUps.Vanish, true, true, false, false);
                snap.Player.ColorId = snap.ColorId;
                snap.Player.HandsId = snap.HandsId;
                snap.Player.SkinId = snap.SkinId;
                snap.Player.NetworkName = snap.Name;
            }

            yield return new WaitForSeconds(0.5f);

            foreach (PlayerSnapshot snap in snapshots)
            {
                if (snap.Player == null) continue;

                if (_originalHats.TryGetValue(snap.Player.PlayerId, out int originalHat))
                {
                    snap.Player.HatId = originalHat;
                    _originalHats.Remove(snap.Player.PlayerId);
                }
            }
        }
    }
}