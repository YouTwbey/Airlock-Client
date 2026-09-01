using AirlockAPI.Data;
using AirlockClient.AC;
using AirlockClient.Data.Roles.MoreRoles.Imposter;
using AirlockClient.Data.Roles.MoreRoles.Neutral;
using AirlockClient.Managers.Debug;
using AirlockClient.Managers.Gamemode;
using HarmonyLib;
using Fusion;
using SG.Airlock;
using SG.Airlock.Network;
using SG.Airlock.Roles;
using System.Collections;
using UnityEngine;
using AirlockClient.Handlers;
using AirlockClient.Attributes;

namespace AirlockClient.Patches
{
    [HarmonyPatch(typeof(NetworkedKillBehaviour), nameof(NetworkedKillBehaviour.RPC_TargetedAction))]
    public class TargetedActionPatch
    {
        public static bool Prefix(NetworkedKillBehaviour __instance, PlayerRef targetedPlayer, PlayerRef perpetrator, ref int action)
        {
            PlayerState perp = GameObject.Find("PlayerState (" + perpetrator.PlayerId + ")").GetComponent<PlayerState>();
            PlayerState target = GameObject.Find("PlayerState (" + targetedPlayer.PlayerId + ")").GetComponent<PlayerState>();

            target.IsSpectating = false;

            if (CurrentMode.IsHosting && !CurrentMode.Modded)
            {
                if (!AntiCheat.Instance.VerifyKill(perp, target, action))
                {
                    return false;
                }
            }

            foreach (SubRole role in SubRole.All)
            {
                if (role is Arsonist)
                {
                    if (role.PlayerWithRole != null && action == (int)ProximityTargetedAction.Kill)
                    {
                        try
                        {
                            if (role.PlayerWithRole == perp)
                            {
                                Logging.Debug_Log($"{perp.NetworkName.Value} has douced {target.NetworkName.Value}!");
                                return false;
                            }
                        }
                        catch { }
                    }
                }
                else if (role is Sniper)
                {
                    if (role.PlayerWithRole != null && action == (int)ProximityTargetedAction.Kill)
                    {
                        try
                        {
                            if (role.PlayerWithRole == perp)
                            {
                                return false;
                            }
                        }
                        catch { }
                    }
                }
                else if (role is Witch)
                {
                    if (role.PlayerWithRole != null && action == (int)ProximityTargetedAction.Kill)
                    {
                        try
                        {
                            if (role.PlayerWithRole == perp)
                            {
                                Logging.Debug_Log($"{perp.NetworkName.Value} has cursed {target.NetworkName.Value}!");
                                ((Witch)role).OnSpellCast(target);
                                return false;
                            }
                        }
                        catch { }
                    }
                }
                else if (role is Duelist)
                {
                    OtherDuelist other = ((Duelist)role).OtherDuelist;
                    if (other != null && action == (int)ProximityTargetedAction.Kill)
                    {
                        try
                        {
                            PlayerState DuelistPlayer = role.PlayerWithRole;
                            PlayerState OtherDuelistPlayer = other.PlayerWithRole;
                            
                            if (perp == role.PlayerWithRole)
                            {
                                if (target == OtherDuelistPlayer)
                                {
                                    DuelistPlayer.SoulLinkID = -1;
                                    OtherDuelistPlayer.SoulLinkID = -1;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            else if (perp == other.PlayerWithRole)
                            {
                                if (target == DuelistPlayer)
                                {
                                    DuelistPlayer.SoulLinkID = -1;
                                    OtherDuelistPlayer.SoulLinkID = -1;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                        }
                        catch { }
                    }
                }
                else if (role is Viper)
                {
                    if (role.PlayerWithRole != null && action == (int)ProximityTargetedAction.Kill)
                    {
                        if (role.PlayerWithRole == perp)
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

                            CoroutineHandler.Start(RestoreFromSnapshot(snapshot));
                        }
                    }
                }
                else if (role is Silencer)
                {
                    if (role.PlayerWithRole != null && ((Silencer)role).CanMutePlayer && action == (int)ProximityTargetedAction.Kill && role.PlayerWithRole.SoulLinkID == -1)
                    {
                        ((Silencer)role).PlayerToMute = target;
                        role.PlayerWithRole.SoulLinkID = ((Silencer)role).PlayerToMute.PlayerId;
                        ((Silencer)role).OriginalRole = MoreRolesManager.GetTrueRoleMR(target);
                        return false;
                    }
                    else if (role.PlayerWithRole != null && !((Silencer)role).CanMutePlayer && action == (int)ProximityTargetedAction.Kill && role.PlayerWithRole.SoulLinkID == role.PlayerWithRole.PlayerId)
                    {
                        Logging.Debug_Log("Deafener has already chosen a player");
                        return true;
                    }

                    if (target == ((Silencer)role).PlayerToMute && perp.IsAlive)
                    {
                        role.PlayerWithRole.SoulLinkID = -1;
                        ((Silencer)role).PlayerToMute = null;
                    }
                }
            }
            return true;
        }
        private static IEnumerator RestoreFromSnapshot(PlayerSnapshot snapshot)
        {
            yield return new WaitForSeconds(0.5f);

            snapshot.Player.ColorId = snapshot.ColorId;
            snapshot.Player.HandsId = snapshot.HandsId;
            snapshot.Player.SkinId = snapshot.SkinId;
            CoroutineHandler.Start(DelayHatRestore(snapshot));
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
