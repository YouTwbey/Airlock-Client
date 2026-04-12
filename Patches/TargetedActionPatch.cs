using AirlockAPI.Data;
using AirlockClient.AC;
using AirlockClient.Data.Roles.MoreRoles.Imposter;
using AirlockClient.Data.Roles.MoreRoles.Neutral;
using AirlockClient.Managers.Debug;
using AirlockClient.Managers.Gamemode;
using HarmonyLib;
using Il2CppFusion;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Network;
using Il2CppSG.Airlock.Roles;
using MelonLoader;
using System.Collections;
using UnityEngine;

namespace AirlockClient.Patches
{
    [HarmonyPatch(typeof(NetworkedKillBehaviour), nameof(NetworkedKillBehaviour.RPC_TargetedAction))]
    public class TargetedActionPatch
    {
        public static bool Prefix(NetworkedKillBehaviour __instance, PlayerRef targetedPlayer, PlayerRef perpetrator, ref int action)
        {
            PlayerState perp = GameObject.Find("PlayerState (" + perpetrator.PlayerId + ")").GetComponent<PlayerState>();
            PlayerState target = GameObject.Find("PlayerState (" + targetedPlayer.PlayerId + ")").GetComponent<PlayerState>();

            if (CurrentMode.IsHosting && !CurrentMode.Modded)
            {
                if (!AntiCheat.Instance.VerifyKill(perp, target, action))
                {
                    return false;
                }
            }

            Arsonist arsonist = UnityEngine.Object.FindObjectOfType<Arsonist>();

            if (arsonist != null && arsonist.PlayerWithRole != null && action == (int)ProximityTargetedAction.Kill)
            {
                try
                {
                    if (arsonist.PlayerWithRole == perp)
                    {
                        Logging.Debug_Log($"{perp.NetworkName.Value} has douced {target.NetworkName.Value}!");
                        return false;
                    }
                }
                catch { }
            }

            Witch witch = UnityEngine.Object.FindObjectOfType<Witch>();

            if (witch != null && witch.PlayerWithRole != null && action == (int)ProximityTargetedAction.Kill)
            {
                try
                {
                    if (witch.PlayerWithRole == perp)
                    {
                        Logging.Debug_Log($"{perp.NetworkName.Value} has cursed {target.NetworkName.Value}!");
                        witch.OnSpellCast(target);
                        return false;
                    }
                }
                catch { }
            }

            Duelist duelist = UnityEngine.Object.FindObjectOfType<Duelist>();
            OtherDuelist otherDuelist = UnityEngine.Object.FindObjectOfType<OtherDuelist>();

            if (duelist != null && otherDuelist != null && action == (int)ProximityTargetedAction.Kill)
            {
                try
                {
                    PlayerState DuelistPlayer = duelist.PlayerWithRole;
                    PlayerState OtherDuelistPlayer = otherDuelist.PlayerWithRole;

                    if (target != DuelistPlayer && target != OtherDuelistPlayer)
                        return false;
                }
                catch { }
            }

            Viper viper = Object.FindObjectOfType<Viper>();
            if (viper != null && viper.PlayerWithRole != null && action == (int)ProximityTargetedAction.Kill)
            {
                if (viper.PlayerWithRole == perp)
                {
                    PlayerSavedState.TryGet(target.PlayerId, out int savedColor, out int savedHat);

                    PlayerSnapshot snapshot = new PlayerSnapshot
                    {
                        Player = target,
                        ColorId = savedColor,
                        HatId = savedHat,
                        HandsId = target.HandsId,
                        SkinId = target.SkinId,
                        Name = target.NetworkName.Value
                    };

                    target.ColorId = 18;

                    MelonCoroutines.Start(RestoreFromSnapshot(snapshot));
                }
            }

            Deafener deafener = Object.FindObjectOfType<Deafener>();
            if (deafener != null && deafener.PlayerWithRole != null && deafener.CanMutePlayer && action == (int)ProximityTargetedAction.Kill && deafener.PlayerWithRole.SoulLinkID == -1)
            {
                deafener.PlayerToMute = target;
                deafener.PlayerWithRole.SoulLinkID = deafener.PlayerToMute.PlayerId;
                deafener.OriginalRole = MoreRolesManager.GetTrueRoleMR(target);
                return false;
            }
            else if (deafener != null && deafener.PlayerWithRole != null && !deafener.CanMutePlayer && action == (int)ProximityTargetedAction.Kill && deafener.PlayerWithRole.SoulLinkID == deafener.PlayerWithRole.PlayerId)
            {
                Logging.Debug_Log("Deafener has already chosen a player");
                return true;
            }
            return true;
        }
        private static IEnumerator RestoreFromSnapshot(PlayerSnapshot snapshot)
        {
            yield return new WaitForSeconds(0.5f);

            snapshot.Player.ColorId = snapshot.ColorId;
            snapshot.Player.HandsId = snapshot.HandsId;
            snapshot.Player.SkinId = snapshot.SkinId;
            MelonCoroutines.Start(DelayHatRestore(snapshot));
        }
        private static IEnumerator DelayHatRestore(PlayerSnapshot snapshot)
        {
            yield return new WaitForSeconds(0.5f);

            snapshot.Player.HatId = 12;
            PlayerSavedState.SetHat(snapshot.Player.PlayerId, snapshot.HatId);
        }
    }
    public class PlayerSnapshot
    {
        public PlayerState Player;
        public int HatId;
        public int HandsId;
        public int SkinId;
        public int ColorId;
        public string Name;
    }
}
