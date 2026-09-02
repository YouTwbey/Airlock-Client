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
// ReSharper disable InconsistentNaming

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

            foreach (var role in SubRole.All)
            {
                switch (role)
                {
                    case Arsonist:
                    {
                        if (role.PlayerWithRole != null && action == (int)ProximityTargetedAction.Kill)
                        {
                            try
                            {
                                if (role.PlayerWithRole == perp)
                                {
                                    Logging.Debug_Log($"{perp.NetworkName.Value} has doused {target.NetworkName.Value}!");
                                    return false;
                                }
                            }
                            catch
                            {
                                // ignored
                            }
                        }

                        break;
                    }
                    case Sniper:
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
                            catch
                            {
                                // ignored
                            }
                        }

                        break;
                    }
                    case Witch witch:
                    {
                        if (witch.PlayerWithRole != null && action == (int)ProximityTargetedAction.Kill)
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
                            catch
                            {
                                // ignored
                            }
                        }

                        break;
                    }
                    case Duelist duelist:
                    {
                        var other = duelist.OtherDuelist;
                        if (other != null && action == (int)ProximityTargetedAction.Kill)
                        {
                            try
                            {
                                var duelistPlayer = duelist.PlayerWithRole;
                                var otherDuelistPlayer = other.PlayerWithRole;
                            
                                if (perp == duelist.PlayerWithRole)
                                {
                                    if (target == otherDuelistPlayer)
                                    {
                                        duelistPlayer.SoulLinkID = -1;
                                        otherDuelistPlayer.SoulLinkID = -1;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                                else if (perp == other.PlayerWithRole)
                                {
                                    if (target == duelistPlayer)
                                    {
                                        duelistPlayer.SoulLinkID = -1;
                                        otherDuelistPlayer.SoulLinkID = -1;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                            }
                            catch
                            {
                                // ignored
                            }
                        }

                        break;
                    }
                    case Viper:
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

                        break;
                    }
                    case Silencer silencer when silencer.PlayerWithRole != null && silencer.CanMutePlayer && action == (int)ProximityTargetedAction.Kill && silencer.PlayerWithRole.SoulLinkID == -1:
                        silencer.PlayerToMute = target;
                        silencer.PlayerWithRole.SoulLinkID = silencer.PlayerToMute.PlayerId;
                        silencer.OriginalRole = MoreRolesManager.GetTrueRoleMR(target);
                        return false;
                    case Silencer silencer when silencer.PlayerWithRole != null && !silencer.CanMutePlayer && action == (int)ProximityTargetedAction.Kill && silencer.PlayerWithRole.SoulLinkID == silencer.PlayerWithRole.PlayerId:
                        Logging.Debug_Log("Deafener has already chosen a player");
                        return true;
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
