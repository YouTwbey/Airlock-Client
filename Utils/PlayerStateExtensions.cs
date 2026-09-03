using System;
using System.Collections.Generic;
using System.Linq;
using AirlockAPI.Data;
using AirlockClient.AC;
using AirlockClient.Attributes;
using AirlockClient.Data.Roles.MoreRoles.Imposter;
using AirlockClient.Data.Roles.MoreRoles.Neutral;
using AirlockClient.Managers.Debug;
using AirlockClient.Managers.Gamemode;
using Fusion;
using SG.Airlock;
using SG.Airlock.Network;
using SG.Airlock.Roles;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.Object;

namespace AirlockClient.Utils;

public static class PlayerStateExtensions
{
    private static AntiCheat Anticheat => AntiCheat.Instance;
    
    private static GameStateManager GetState()
    {
        if (Anticheat && Anticheat.State != null) return Anticheat.State;
        if (StaticRefs.Instance && StaticRefs.Instance.State != null) return StaticRefs.Instance.State;
        return null;
    }

    private static PlayerState GetPlayerStateById(int playerId)
    {
        var states = GetState()?.SpawnManager?.PlayerStates;
        if (states == null) return null;

        foreach (PlayerState p in states)
        {
            if (p != null && p.PlayerId == playerId)
            {
                return p;
            }
        }

        return null;
    }

    public static SubRole GetSubRole(this PlayerState playerState)
    {
        return SubRole.All.FirstOrDefault(role => role.PlayerWithRole == playerState);
    }

    public static void VerifyState(this PlayerState playerState)
    {
        if (!playerState.IsSpawned) return;
        if (!playerState.IsConnected) return;
        if (Anticheat == null) return;

        if (!CurrentMode.Modded && !AntiCheat.Instance.ColorToName.ContainsValue(playerState.NetworkName.Value) &&
            playerState.NetworkName.Value != "Color###")
        {
            var formattedName = playerState.PlayerModerationUsername.Replace(@"\d", "");
            if (formattedName != "" && !formattedName.Contains(playerState.NetworkName.Value))
            {
                if (playerState.GetComponent<SubRole>())
                {
                    if (!playerState.GetComponent<SubRole>().IsDisplayingRole &&
                        playerState.NetworkName.Value != "DEPUTY")
                    {
                        playerState.Alert("invalid player username", true);
                    }
                }
                else
                {
                    playerState.Alert("invalid player username", true);
                }
            }
        }

        if (!playerState.IsSpectating && !Anticheat.State.InLobbyState())
        {
            if (Anticheat.PreviousColor[playerState] != playerState.ColorId ||
                Anticheat.PreviousGlove[playerState] != playerState.HandsId ||
                Anticheat.PreviousHat[playerState] != playerState.HatId ||
                Anticheat.PreviousSkin[playerState] != playerState.SkinId)
            {
                playerState.Alert("invalid cosmetics", true);
            }
        }

        if (playerState.IsSpectating || Anticheat.State.InLobbyState())
        {
            Anticheat.PreviousColor[playerState] = playerState.ColorId;
            Anticheat.PreviousGlove[playerState] = playerState.HandsId;
            Anticheat.PreviousHat[playerState] = playerState.HatId;
            Anticheat.PreviousSkin[playerState] = playerState.SkinId;
        }

        if (playerState.ActivePowerUps != PowerUps.None && !CurrentMode.Modded && CurrentMode.Name != "Infection")
        {
            playerState.Alert("user has powerups at invalid time.", true);
        }

        if (playerState.LocomotionPlayer.NetworkRigidbody.Rigidbody.velocity.x > 10 ||
            playerState.LocomotionPlayer.NetworkRigidbody.Rigidbody.velocity.z > 10)
        {
            //Punish(player, "speedhack detected: " + player.LocomotionPlayer.NetworkRigidbody.Rigidbody.velocity.ToString());
        }

        if (Anticheat.BannedUsers.Contains(playerState.PlayerModerationID.Value))
        {
            playerState.Alert("user has been banned from the lobby.", true);
        }
    }

    public static bool VerifyVent(this PlayerState playerState)
    {
        var state = GetState();

        bool IsCheating = false;

        if (state.GameModeStateValue.GameMode == GameModes.Infection)
        {
            if (playerState.GetTrueRole() != GameRole.Crewmember || playerState.ActivePowerUps != PowerUps.CanVent)
            {
                IsCheating = true;
            }
        }
        else
        {
            if (playerState.GetTrueRole() != GameRole.Impostor && playerState.GetTrueRole() != GameRole.Engineer)
            {
                IsCheating = true;
            }
        }

        if (IsCheating)
        {
            playerState.Alert("suspicious vent data", true);
        }

        return !IsCheating;
    }

    public static GameRole GetTrueRole(this PlayerState playerState)
    {
        foreach (var roleEntry in StaticRefs.Instance.Role.gameRoleToPlayerIds)
        {
            foreach (var id in roleEntry.Value)
            {
                if (!playerState.IsConnected) continue;
                if (id == playerState.PlayerId)
                {
                    return roleEntry.Key;
                }
            }
        }

        return GameRole.NotSet;
    }

    public static void Alert(this PlayerState playerState, string reason, bool takeAction)
    {
        if (!Anticheat.VerifyModerationID(playerState.PlayerModerationID.Value))
        {
            playerState.PlayerModerationID.Value = playerState.GetActualModId();
            reason = "hidden moderation id";
            takeAction = true;

            Logging.Warn("CHEATER DETECTED! " + playerState.NetworkName.Value + " (" +
                         playerState.PlayerModerationUsername + ", " + playerState.PlayerModerationID.Value +
                         ") was caught cheating. Reason: " + reason + ". Reporting and banning user from lobby.");
            playerState.NetworkName.Value = "CHEATER";
            Anticheat.BannedUsers.Add(playerState.PlayerModerationID.Value);
            Anticheat.SendReportToDevelopers(playerState, reason);
            return;
        }

        playerState.PlayerModerationID.Value = playerState.GetActualModId();

        if (takeAction)
        {
            if (!Anticheat.BannedUsers.Contains(playerState.PlayerModerationID.Value))
            {
                Logging.Warn("CHEATER DETECTED! " + playerState.NetworkName.Value + " (" +
                             playerState.PlayerModerationUsername + ", " + playerState.PlayerModerationID.Value +
                             ") was caught cheating. Reason: " + reason + ". Reporting and banning user from lobby.");
                playerState.NetworkName.Value = "CHEATER";
                Anticheat.BannedUsers.Add(playerState.PlayerModerationID.Value);
                Anticheat.SendReportToDevelopers(playerState, reason);
            }
            else
            {
                Logging.Warn("CHEATER DETECTED! " + playerState.NetworkName.Value + " (" +
                             playerState.PlayerModerationUsername + ", " + playerState.PlayerModerationID.Value +
                             ") was caught cheating. Reason: " + reason + ". Banning user from lobby.");
                playerState.NetworkName.Value = "CHEATER";
            }

            if (playerState.PlayerId != 9)
            {
                Anticheat.Moderation.Runner.Disconnect(playerState.PlayerId);
            }
            else
            {
                SceneManager.LoadScene("Title");
            }

            return;
        }

        Logging.Warn("CHEATER DETECTED! " + playerState.NetworkName.Value + " (" +
                     playerState.PlayerModerationUsername + ", " + playerState.PlayerModerationID.Value +
                     ") is being suspected of cheating. Reason: " + reason + ".");
    }

    public static string GetActualModId(this PlayerState playerState)
    {
        if (playerState == null) return null;
        
        if (StaticRefs.Instance.Runner == null) return playerState.PlayerModerationID?.Value;

        string userId = StaticRefs.Instance.Runner.GetPlayerUserId(playerState.LocomotionPlayer.PlayerID);
        if (string.IsNullOrEmpty(userId)) return playerState.PlayerModerationID?.Value;

        return userId;
    }

    public static void ChangeIsAliveWithAntiCheat(this PlayerState player, bool isAlive)
    {
        if (Anticheat != null)
        {
            if (isAlive)
            {
                if (Anticheat.IsDead.Contains(player))
                {
                    Anticheat.IsDead.Remove(player);
                }
            }
            else
            {
                if (!Anticheat.IsDead.Contains(player))
                {
                    Anticheat.IsDead.Add(player);
                }
            }
        }

        player.IsAlive = isAlive;
    }

    public static bool VerifySpawnBody(this PlayerState body, NetworkRigidbodyObsolete rb)
    {
        var state = GetState();
        if (state == null) return true;

        bool IsCheating = false;

        if (state.GameModeStateValue.GameMode == GameModes.Infection)
        {
            IsCheating = true;
        }

        if (body.LocomotionPlayer.NetworkRigidbody != rb)
        {
            IsCheating = true;
        }

        if (!IsCheating)
        {
            if (Anticheat != null)
            {
                if (!Anticheat.AllowedBodySpawns.Contains(body))
                {
                    IsCheating = true;
                }
                else
                {
                    Anticheat.AllowedBodySpawns.Remove(body);
                }
            }
        }
        else
        {
            body.Alert("suspicious spawn body data", true);
        }

        return !IsCheating;
    }

    public static bool VerifyMeeting(this PlayerState caller, RpcInfo info)
    {
        if (Anticheat == null) return true;

        int sender = info.Source;
        bool IsCheating = false;
        int TotalMeetings = Anticheat.Button._emergencyMeetingsVar.Value;
        float distance = (caller.LocomotionPlayer.RigidbodyPosition - Anticheat.Button.transform.position).magnitude;

        if (caller.PlayerId != sender && info.Source.IsValid)
        {
            GetPlayerStateById(sender)?.Alert("misuse of meeting rpc", true);
            return false;
        }

        if (!caller.IsAlive)
        {
            IsCheating = true;
        }

        if (distance > 5)
        {
            IsCheating = true;
        }

        if (Anticheat.State.GameModeStateValue.GameMode == GameModes.Infection)
        {
            IsCheating = true;
        }

        if (!Anticheat.MeetingsCalled.ContainsKey(caller))
        {
            Anticheat.MeetingsCalled.Add(caller, 0);
        }

        Anticheat.MeetingsCalled[caller]++;

        if (Anticheat.MeetingsCalled[caller] > TotalMeetings)
        {
            IsCheating = true;
        }

        if (IsCheating)
        {
            caller.Alert("suspicious meeting data", false);
        }

        return !IsCheating;
    }

    public static bool VerifyVote(this PlayerState voter, PlayerState voted, RpcInfo info)
    {
        var state = GetState();
        if (state == null) return true;

        int sender = info.Source;
        bool IsCheating = false;

        if (sender != voter.PlayerId && info.Source.IsValid)
        {
            GetPlayerStateById(sender)?.Alert("misuse of vote rpc", true);
        }

        if (state.GameModeStateValue.GameMode == GameModes.Infection)
        {
            IsCheating = true;
        }

        if (IsCheating)
        {
            GetPlayerStateById(sender)?.Alert("suspicious vote data", true);
        }

        return !IsCheating;
    }

    public static bool VerifyBodyReport(this PlayerState reporter, PlayerState bodyReported, RpcInfo info)
    {
        var state = GetState();
        if (state == null) return true;

        int sender = info.Source;
        bool IsCheating = false;
        var bodyObj = GameObject.Find("NetworkedBody (" + bodyReported.PlayerId + ")");
        if (bodyObj == null)
        {
            GetPlayerStateById(sender)?.Alert("suspicious report body data (missing body)", true);
            return false;
        }
        NetworkedBody body = bodyObj.GetComponent<NetworkedBody>();
        float distance = (body.transform.position - reporter.LocomotionPlayer.RigidbodyPosition).magnitude;

        if (reporter.PlayerId != sender && info.Source.IsValid)
        {
            GetPlayerStateById(sender)?.Alert("misuse of body report data", true);
            return false;
        }

        if (state.GameModeStateValue.GameMode == GameModes.Infection)
        {
            IsCheating = true;
        }

        if (bodyReported.IsAlive || !reporter.IsAlive)
        {
            IsCheating = true;
        }

        if (distance > 5)
        {
            IsCheating = true;
        }
        
        if (Anticheat != null)
        {
            if (Anticheat.BodiesReported.Contains(bodyReported))
            {
                IsCheating = true;
            }
            else
            {
                if (!IsCheating)
                {
                    Anticheat.BodiesReported.Add(bodyReported);
                }
            }
        }

        if (IsCheating)
        {
            reporter.Alert("suspicious report body data", false);
        }

        return !IsCheating;
    }

    public static void KillPlayerWithAntiCheat(this PlayerState killer, PlayerState target)
    {
        if (!Anticheat)
        {
            if (StaticRefs.Instance)
            {
                FindObjectOfType<NetworkedKillBehaviour>()
                    .KillPlayer(FindObjectOfType<AirlockPeer>(), target, killer.PlayerId,
                        killer.GetTrueRole() == GameRole.Vigilante);
            }
            return;
        }

        if (Anticheat.AllowedBodySpawns.Contains(target))
        {
            Anticheat.AllowedBodySpawns.Remove(target);
        }

        if (Anticheat.IsDead.Contains(target))
        {
            Anticheat.IsDead.Remove(target);
        }

        Anticheat.AllowedBodySpawns.Add(target);

        if (!target.Guarded)
        {
            Anticheat.Kill.KillPlayer(Anticheat.Peer, target, killer.PlayerId,
                killer.GetTrueRole() == GameRole.Vigilante);
        }
        else
        {
            killer.PlayShieldBreakWithAntiCheat(target);
        }
    }

    public static void PlayShieldBreakWithAntiCheat(this PlayerState killer, PlayerState target)
    {
        if (!Anticheat)
        {
            NetworkedKillBehaviour Kill = FindObjectOfType<NetworkedKillBehaviour>();
            Kill.RPC_GuardVFX(target.PlayerId, true, false, false);
            Kill.RPC_GuardVFX(target.PlayerId, false, false, true);
            target.Guarded = false;
            return;
        }

        Anticheat.Kill.RPC_GuardVFX(target.PlayerId, true, false, false);
        Anticheat.Kill.RPC_GuardVFX(target.PlayerId, false, false, true);
        target.Guarded = false;
    }

    public static void InfectPlayerWithAntiCheat(this PlayerState killer, PlayerState target)
    {
        if (!Anticheat)
        {
            FindObjectOfType<NetworkedKillBehaviour>().InfectPlayer(target, killer.PlayerId, GameRole.Infected,
                FindObjectOfType<AirlockPeer>());
            return;
        }

        Anticheat.Kill.InfectPlayer(target, killer.PlayerId, GameRole.Infected, Anticheat.Peer);
    }

    public static void ChangeHatWithAntiCheat(this PlayerState player, int hatId)
    {
        if (Anticheat)
        {
            Anticheat.PreviousHat[player] = hatId;
        }

        player.HatId = hatId;
    }

    public static void ChangeSkinWithAntiCheat(this PlayerState player, int SkinId)
    {
        if (Anticheat)
        {
            Anticheat.PreviousSkin[player] = SkinId;
        }

        player.SkinId = SkinId;
    }

    public static void ChangeGlovesWithAntiCheat(this PlayerState player, int handsId)
    {
        if (Anticheat)
        {
            Anticheat.PreviousGlove[player] = handsId;
        }

        player.HandsId = handsId;
    }

    public static void RemoveSpellWithAntiCheat(this PlayerState caller, PlayerState victim, bool toggleKill)
    {
        if (Anticheat)
        {
            if (caller.GetSubRole() is not Witch) return;

            if (Anticheat.RoleTargets.ContainsKey(caller))
            {
                if (Anticheat.RoleTargets[caller].Contains(victim))
                {
                    Anticheat.RoleTargets[caller].Remove(victim);
                }
            }
        }

        if (caller.GetSubRole() is not Witch) return;

        var witch = (Witch)caller.GetSubRole();
        if (!witch.spellsCasted.ContainsKey(victim)) return;

        victim.NetworkName.Value = witch.spellsCasted[victim];
        witch.spellsCasted.Remove(victim);

        if (toggleKill)
        {
            victim.IsAlive = false;
        }
    }

    public static void CastSpellWithAntiCheat(this PlayerState caller, PlayerState victim)
    {
        if (Anticheat)
        {
            if (caller.GetSubRole() is not Witch) return;

            if (!Anticheat.RoleTargets.ContainsKey(caller))
            {
                Anticheat.RoleTargets.Add(caller, new List<PlayerState>());
            }

            Anticheat.RoleTargets[caller].Add(victim);
        }

        if (caller.GetSubRole() is not Witch) return;
        var witch = (Witch)caller.GetSubRole();

        witch.spellsCasted[victim] = victim.NetworkName.Value;
    }

    public static void DousePlayerWithAntiCheat(this PlayerState player, PlayerState victim)
    {
        if (Anticheat)
        {
            if (player.GetSubRole() is not Arsonist) return;

            if (!Anticheat.RoleTargets.ContainsKey(player))
            {
                Anticheat.RoleTargets.Add(player, new List<PlayerState>());
            }

            Anticheat.RoleTargets[player].Add(victim);
        }

        if (player.GetSubRole() is not Arsonist) return;

        var arsonist = (Arsonist)player.GetSubRole();

        arsonist.dousedPlayers[victim] = victim.NetworkName.Value;
    }

    public static bool VerifyKill(this PlayerState killer, PlayerState target, int action)
    {
        var state = GetState();
        if (state == null) return true;

        bool IsCheating = false;

        float distance = (killer.LocomotionPlayer.RigidbodyPosition - target.LocomotionPlayer.RigidbodyPosition)
            .magnitude;
        GameRole killerRole = killer.GetTrueRole();
        GameRole targetRole = target.GetTrueRole();

        if (killer == target)
        {
            IsCheating = true;
        }
        
        if (!CurrentMode.Modded && Anticheat != null)
        {
            if (Anticheat.TargetActionCheck.TryGetValue(killer, out var previousKill))
            {
                var difference = (DateTime.Now - previousKill).TotalSeconds;

                if (killerRole != GameRole.Revenger)
                {
                    if (killerRole == GameRole.Crewmember)
                    {
                        if (Anticheat.State._GameModeStateValue.GameMode == GameModes.Infection)
                        {
                            if (difference < Anticheat.GetCooldownForRole(GameRole.Crewmember) - 1 ||
                                killer.ActivePowerUps == PowerUps.None)
                            {
                                IsCheating = true;
                            }
                        }
                        else
                        {
                            if (difference < Anticheat.GetCooldownForRole(GameRole.Crewmember) - 1)
                            {
                                IsCheating = true;
                            }
                        }
                    }
                    else
                    {
                        if (difference < Anticheat.GetCooldownForRole(killerRole) - 1)
                        {
                            IsCheating = true;
                        }
                    }
                }
                else
                {
                    if (difference < Anticheat.GetCooldownForRole(GameRole.Revenger, false) - 1)
                    {
                        IsCheating = true;
                    }
                }
            }
            else
            {
                Anticheat.TargetActionCheck.Add(killer, DateTime.Now);
            }
        }

        if (distance > 6 || !target.IsAlive)
        {
            IsCheating = true;
        }

        var targetAction = (ProximityTargetedAction)action;

        switch (targetAction)
        {
            case ProximityTargetedAction.None:
                IsCheating = true;
                break;

            case ProximityTargetedAction.Kill:
                if (CurrentMode.Name == "Hide N Seek")
                {
                    if (killerRole != GameRole.Infected || targetRole != GameRole.Crewmember ||
                        state.GameModeStateValue.GameMode == GameModes.Infection || !state.InTaskState())
                    {
                        IsCheating = true;
                    }
                }
                else
                {
                    if (killerRole == GameRole.Impostor)
                    {
                        if (targetRole == GameRole.Impostor ||
                            state.GameModeStateValue.GameMode == GameModes.Infection || !state.InTaskState())
                        {
                            IsCheating = true;
                        }
                    }
                    else
                    {
                        if (killerRole != GameRole.Vigilante ||
                            state.GameModeStateValue.GameMode == GameModes.Infection || !state.InTaskState())
                        {
                            IsCheating = true;
                        }
                    }
                }

                if (!IsCheating && Anticheat != null)
                {
                    Anticheat.AllowedBodySpawns.Add(target);
                }

                break;

            case ProximityTargetedAction.Neutralize:
                if (killer.ActivePowerUps != PowerUps.Stun || targetRole != GameRole.Infected ||
                    state.GameModeStateValue.GameMode != GameModes.Infection || !state.InTaskState())
                {
                    IsCheating = true;
                }

                break;

            case ProximityTargetedAction.Infect:
                if (killerRole != GameRole.Infected || targetRole == GameRole.Infected ||
                    state.GameModeStateValue.GameMode != GameModes.Infection || !state.InTaskState())
                {
                    IsCheating = true;
                }

                break;

            case ProximityTargetedAction.Guard:
                if (state.GameModeStateValue.GameMode == GameModes.Infection)
                {
                    if (killer.ActivePowerUps != PowerUps.Guard || targetRole != GameRole.Crewmember ||
                        !state.InTaskState())
                    {
                        IsCheating = true;
                    }
                }
                else
                {
                    if (killerRole != GameRole.GuardianAngel || !state.InTaskState())
                    {
                        IsCheating = true;
                    }
                }

                break;

            case ProximityTargetedAction.Vote:
                if (killerRole != GameRole.Sheriff ||
                    state.GameModeStateValue.GameMode != GameModes.Sheriff || !state.InVotingState())
                {
                    IsCheating = true;
                }

                break;

            case ProximityTargetedAction.KillSelf:
                if (killerRole != GameRole.Revenger ||
                    state.GameModeStateValue.GameMode != GameModes.BuffGhosts || !state.InVotingState())
                {
                    IsCheating = true;
                }

                if (!IsCheating && Anticheat != null)
                {
                    Anticheat.AllowedBodySpawns.Add(target);
                }

                break;
            case ProximityTargetedAction.Scan:
            case ProximityTargetedAction.Track:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (!IsCheating)
        {
            if (Anticheat != null)
            {
                Anticheat.TargetActionCheck[killer] = DateTime.Now;
            }
        }
        else
        {
            killer.Alert("suspicious kill data", false);
        }

        return !IsCheating;
    }
}