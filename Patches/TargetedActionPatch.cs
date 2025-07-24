using AirlockClient.Attributes;
using HarmonyLib;
using Il2CppFusion;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Network;
using UnityEngine;
using AirlockClient.Data.Roles.MoreRoles.Imposter;
using AirlockClient.Data.Roles.MoreRoles.Crewmate;
using Il2CppSG.Airlock.Roles;
using AirlockClient.AC;
using AirlockAPI.Data;

namespace AirlockClient.Patches
{
    [HarmonyPatch(typeof(NetworkedKillBehaviour), nameof(NetworkedKillBehaviour.RPC_TargetedAction))]
    public class TargetedActionPatch
    {
        public static bool Prefix(NetworkedKillBehaviour __instance, PlayerRef targetedPlayer, PlayerRef perpetrator, ref int action)
        {
            PlayerState perp = GameObject.Find("PlayerState (" + perpetrator.PlayerId + ")").GetComponent<PlayerState>();
            PlayerState target = GameObject.Find("PlayerState (" + targetedPlayer.PlayerId + ")").GetComponent<PlayerState>();
            SubRole targetSubRole = target.GetComponent<SubRole>();
            GameRole targetRole = ModdedGamemode.Current.GetTrueRole(target);

            if (CurrentMode.IsHosting && !CurrentMode.Modded)
            {
                if (!AntiCheat.Instance.VerifyKill(perp, target, action))
                {
                    return false;
                }
            }

            if (CurrentMode.Modded)
            {
                if (CurrentMode.IsHosting)
                {
                    if (CurrentMode.Name == "Infection")
                    {
                        return true;
                    }

                    if (ModdedGamemode.Current)
                    {
                        ModdedGamemode.Current.OnKill(perp, target, action);
                    }

                    if (CurrentMode.Name == "Hide N Seek")
                    {
                        AntiCheat.KillPlayerWithAntiCheat(perp, target);
                        return false;
                    }

                    if (target.Guarded)
                    {
                        AntiCheat.PlayShieldBreakWithAntiCheat(perp, target);

                        return false;
                    }

                    foreach (SubRole role in SubRole.All)
                    {
                        if (role == null) continue;
                        if (role.PlayerWithRole == null) continue;

                        if (role.PlayerWithRole.PlayerId == perpetrator.PlayerId)
                        {
                            if (perp.GetComponent<Vampire>())
                            {
                                perp.GetComponent<Vampire>().DelayedKill(target, action);
                                return false;
                            }

                            if (perp.GetComponent<Sheriff>())
                            {
                                if (targetRole == GameRole.Imposter)
                                {
                                    if (targetSubRole != null)
                                    {
                                        targetSubRole.OnPlayerDied(perp);
                                    }
                                    role.OnPlayerKilled(target);
                                    role.OnPlayerAction(action);
                                    return true;
                                }
                                else
                                {
                                    AntiCheat.KillPlayerWithAntiCheat(perp, perp);
                                    return false;
                                }
                            }

                            if (perp.GetComponent<Witch>())
                            {
                                if (target.GetComponent<Bait>() == null)
                                {
                                    if (targetSubRole != null)
                                    {
                                        targetSubRole.OnPlayerDied(perp);
                                    }
                                }
                                role.OnPlayerAction(action);

                                return false;
                            }

                            if (perp.GetComponent<Poisoner>())
                            {
                                role.OnPlayerKilled(target);
                                role.OnPlayerAction(action);
                                return false;
                            }

                            if (targetSubRole != null)
                            {
                                targetSubRole.OnPlayerDied(perp);
                            }

                            role.OnPlayerKilled(target);
                            role.OnPlayerAction(action);
                        }
                        else if (role.PlayerWithRole.PlayerId == target.PlayerId)
                        {
                            Armorer armorer = target.GetComponent<Armorer>();
                            if (armorer)
                            {
                                if (armorer.HasTakenHit)
                                {
                                    return true;
                                }
                                else
                                {
                                    armorer.HasTakenHit = true;
                                    return false;
                                }
                            }
                        }
                    }

                    if (targetSubRole != null)
                    {
                        targetSubRole.OnPlayerDied(perp);
                    }
                }
            }

            return true;
        }
    }
}
