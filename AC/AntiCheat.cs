using AirlockAPI.Data;
using AirlockClient.Attributes;
using AirlockClient.Managers.Debug;
using Il2CppFusion;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Network;
using Il2CppSG.Airlock.Roles;
using Il2CppSG.Airlock.UI.Moderation;
using Il2CppSystem.IO;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace AirlockClient.AC
{
    public class AntiCheat : MonoBehaviour
    {
        public static AntiCheat Instance;

        public Dictionary<PlayerState, DateTime> TargetActionCheck = new Dictionary<PlayerState, DateTime>();
        public Dictionary<PlayerState, int> PreviousHat = new Dictionary<PlayerState, int>();
        public Dictionary<PlayerState, int> PreviousGlove = new Dictionary<PlayerState, int>();
        public Dictionary<PlayerState, int> PreviousSkin = new Dictionary<PlayerState, int>();
        public Dictionary<PlayerState, int> PreviousColor = new Dictionary<PlayerState, int>();
        public Dictionary<PlayerState, int> MeetingsCalled = new Dictionary<PlayerState, int>();
        public List<PlayerState> AllowedBodySpawns = new List<PlayerState>();
        public List<string> BannedUsers = new List<string>();
        public List<PlayerState> BodiesReported = new List<PlayerState>();
        public List<PlayerState> WitchTargets = new List<PlayerState>();

        public readonly Dictionary<int, string> ColorToName = new Dictionary<int, string> {
            {0, "Red"},
            {1, "Blue"},
            {2, "Green"},
            {3, "Pink"},
            {4, "Orange"},
            {5, "Yellow"},
            {6, "Gray"},
            {7, "White"},
            {8, "Purple"},
            {9, "Brown"},
            {10, "Cyan"},
            {11, "Lime"},
        };

        public ModerationManager Moderation;
        public RoleManager Role;
        public GameStateManager State;
        public EmergencyButton Button;
        public NetworkedKillBehaviour Kill;
        public AirlockPeer Peer;
        SHA256 encrypt;

        void Start()
        {
            if (Instance == null)
            {
                Instance = this;
                Moderation = FindObjectOfType<ModerationManager>();
                Role = FindObjectOfType<RoleManager>();
                State = FindObjectOfType<GameStateManager>();
                Button = FindObjectOfType<EmergencyButton>();
                Kill = FindObjectOfType<NetworkedKillBehaviour>();
                Peer = FindObjectOfType<AirlockPeer>();
                encrypt = SHA256.Create();

                MelonCoroutines.Start(FetchBlacklist());
            }
            else
            {
                Destroy(this);
            }
        }

        public void OnEndGame()
        {
            MeetingsCalled.Clear();
            BodiesReported.Clear();
            AllowedBodySpawns.Clear();
        }

        public static void KillPlayerWithAntiCheat(PlayerState killer, PlayerState target)
        {
            if (!Instance)
            {
                FindObjectOfType<NetworkedKillBehaviour>().KillPlayer(FindObjectOfType<AirlockPeer>(), target, killer.PlayerId, false);
                return;
            }

            if (Instance.AllowedBodySpawns.Contains(target))
            {
                Instance.AllowedBodySpawns.Remove(target);
            }

            Instance.AllowedBodySpawns.Add(target);

            if (!target.Guarded)
            {
                Instance.Kill.KillPlayer(Instance.Peer, target, killer.PlayerId, Instance.GetTrueRole(killer) == GameRole.Vigilante);
            }
            else
            {
                PlayShieldBreakWithAntiCheat(killer, target);
            }
        }

        public static void PlayShieldBreakWithAntiCheat(PlayerState killer, PlayerState target)
        {
            if (!Instance)
            {
                NetworkedKillBehaviour Kill = FindObjectOfType<NetworkedKillBehaviour>();
                Kill.GuardTarget(target);
                Kill.InfectPlayer(target, killer.PlayerId, GameRole.Infected, FindObjectOfType<AirlockPeer>());
                return;
            }

            Instance.Kill.GuardTarget(target);
            Instance.Kill.InfectPlayer(target, killer.PlayerId, GameRole.Infected, Instance.Peer);
        }

        public static void InfectPlayerWithAntiCheat(PlayerState killer, PlayerState target)
        {
            if (!Instance)
            {
                FindObjectOfType<NetworkedKillBehaviour>().InfectPlayer(target, killer.PlayerId, GameRole.Infected, FindObjectOfType<AirlockPeer>());
                return;
            }

            Instance.Kill.InfectPlayer(target, killer.PlayerId, GameRole.Infected, Instance.Peer);
        }

        public static void ChangeIsAliveWithAntiCheat(PlayerState player, bool isAlive)
        {
            player.IsAlive = isAlive;
        }

        public static void ChangeHatWithAntiCheat(PlayerState player, int hatId)
        {
            if (Instance)
            {
                Instance.PreviousHat.Add(player, hatId);
            }

            player.HatId = hatId;
        }

        public bool VerifyBodyReport(PlayerState reporter, PlayerState bodyReported, RpcInfo info)
        {
            int sender = info.Source;
            bool IsCheating = false;
            NetworkedBody body = GameObject.Find("NetworkedBody (" + bodyReported.PlayerId + ")").GetComponent<NetworkedBody>();
            float distance = (body.transform.position - reporter.LocomotionPlayer.RigidbodyPosition).magnitude;

            if (reporter.PlayerId != sender)
            {
                Alert(State.SpawnManager.PlayerStates[sender], "misuse of body report data", true);
                return false;
            }

            if (State.GameModeStateValue.GameMode == GameModes.Infection)
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
            if (BodiesReported.Contains(bodyReported))
            {
                IsCheating = true;
            }
            else
            {
                if (!IsCheating)
                {
                    BodiesReported.Add(bodyReported);
                }
            }

            if (IsCheating)
            {
                Alert(reporter, "suspicious report body data", false);
            }

            return !IsCheating;
        }

        public bool VerifyMeeting(PlayerState caller, RpcInfo info)
        {
            int sender = info.Source;
            bool IsCheating = false;
            int TotalMeetings = Button._emergencyMeetingsVar.Value;
            float distance = (caller.LocomotionPlayer.RigidbodyPosition - Button.transform.position).magnitude;

            if (caller.PlayerId != sender)
            {
                Alert(State.SpawnManager.PlayerStates[sender], "misuse of meeting rpc", true);
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

            if (State.GameModeStateValue.GameMode == GameModes.Infection)
            {
                IsCheating = true;
            }

            if (!MeetingsCalled.ContainsKey(caller))
            {
                MeetingsCalled.Add(caller, 0);
            }

            MeetingsCalled[caller]++;

            if (MeetingsCalled[caller] > TotalMeetings)
            {
                IsCheating = true;
            }

            if (IsCheating)
            {
                Alert(caller, "suspicious meeting data", false);
            }

            return !IsCheating;
        }

        public bool VerifySpawnBody(PlayerState body, NetworkRigidbodyObsolete rb)
        {
            bool IsCheating = false;

            if (State.GameModeStateValue.GameMode == GameModes.Infection)
            {
                IsCheating = true;
            }

            if (body.LocomotionPlayer.NetworkRigidbody != rb)
            {
                IsCheating = true;
            }

            if (!IsCheating)
            {
                if (!AllowedBodySpawns.Contains(body))
                {
                    IsCheating = true;
                }
                else
                {
                    AllowedBodySpawns.Remove(body);
                }
            }
            else
            {
                Alert(body, "suspicious spawn body data", true);
            }

            return !IsCheating;
        }

        List<string> BlacklistedUsers = new List<string>();
        const string BlacklistUrl = "https://raw.githubusercontent.com/YouTwbey/Airlock-Client/main/AC/blacklisted_user_list.txt";

        System.Collections.IEnumerator FetchBlacklist()
        {
            UnityWebRequest www = UnityWebRequest.Get(BlacklistUrl);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Logging.Error($"Failed to fetch blacklist: {www.error}");
                yield break;
            }

            string[] ids = www.downloadHandler.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string id in ids)
            {
                Logging.Log($"Added {id} to the blacklist!");
                BlacklistedUsers.Add(id);
            }
        }

        string ModerationIDToSHA256(PlayerRef player)
        {
            string playerId = State.Runner.GetPlayerUserId(player);

            byte[] inputBytes = Encoding.UTF8.GetBytes(playerId);
            byte[] hashBytes = encrypt.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }

        int checkDelay;

        void Update()
        {
            if (checkDelay == 0)
            {
                foreach (PlayerRef player in State.Runner.ActivePlayers.ToArray())
                {
                    if (BlacklistedUsers.Contains(ModerationIDToSHA256(player)))
                    {
                        Alert(State.SpawnManager.PlayerStates[player], "user is on blacklist", true);
                    }
                }
                checkDelay = 60;
            }
            else
            {
                checkDelay -= 1;
            }

            foreach (PlayerState state in State.SpawnManager.ActivePlayerStates)
            {
                if (state != null)
                {
                    if (state.IsSpawned)
                    {
                        if (state.IsConnected)
                        {
                            if (!PreviousColor.ContainsKey(state))
                            {
                                PreviousColor.Add(state, state.ColorId);
                                PreviousGlove.Add(state, state.HandsId);
                                PreviousHat.Add(state, state.HatId);
                                PreviousSkin.Add(state, state.SkinId);
                            }

                            VerifyState(state);
                        }
                    }
                }
            }
        }

        public void VerifyState(PlayerState player)
        {
            if (player.IsSpawned)
            {
                if (player.IsConnected)
                {
                    if (!ColorToName.ContainsValue(player.NetworkName.Value) && player.NetworkName.Value != "Color###")
                    {
                        string formattedName = Regex.Replace(player.PlayerModerationUsername, @"\d", "");
                        if (formattedName != "" && !formattedName.Contains(player.NetworkName.Value))
                        {
                            if (player.GetComponent<SubRole>())
                            {
                                if (!player.GetComponent<SubRole>().IsDisplayingRole && player.NetworkName.Value != "DEPUTY")
                                {
                                    Alert(player, "invalid player username", true);
                                }
                            }
                            else
                            {
                                Alert(player, "invalid player username", true);
                            }
                        }
                    }

                    if (!player.IsSpectating && !State.InLobbyState())
                    {
                        if (PreviousColor[player] != player.ColorId ||
                            PreviousGlove[player] != player.HandsId ||
                            PreviousHat[player] != player.HatId ||
                            PreviousSkin[player] != player.SkinId)
                        {
                            Alert(player, "invalid cosmetics", true);
                        }
                    }

                    if (player.IsSpectating || State.InLobbyState())
                    {
                        PreviousColor[player] = player.ColorId;
                        PreviousGlove[player] = player.HandsId;
                        PreviousHat[player] = player.HatId;
                        PreviousSkin[player] = player.SkinId;
                    }

                    if (player.ActivePowerUps != PowerUps.None && !CurrentMode.Modded && CurrentMode.Name != "Infection")
                    {
                        Alert(player, "user has powerups at invalid time.", true);
                    }

                    if (player.LocomotionPlayer.NetworkRigidbody.Rigidbody.velocity.x > 10 || player.LocomotionPlayer.NetworkRigidbody.Rigidbody.velocity.z > 10)
                    {
                        //Punish(player, "speedhack detected: " + player.LocomotionPlayer.NetworkRigidbody.Rigidbody.velocity.ToString());
                    }

                    if (BannedUsers.Contains(player.PlayerModerationID.Value))
                    {
                        Alert(player, "user has been banned from the lobby.", true);
                    }
                }
            }
        }

        public bool VerifyVent(PlayerState venter)
        {
            bool IsCheating = false;

            if (State.GameModeStateValue.GameMode == GameModes.Infection)
            {
                if (GetTrueRole(venter) != GameRole.Crewmember || venter.ActivePowerUps != PowerUps.CanVent)
                {
                    IsCheating = true;
                }
            }
            else
            {
                if (GetTrueRole(venter) != GameRole.Imposter && GetTrueRole(venter) != GameRole.Engineer)
                {
                    IsCheating = true;
                }
            }

            if (IsCheating)
            {
                Alert(venter, "suspicious vent data", true);
            }

            return !IsCheating;
        }

        public bool VerifyVote(PlayerState voter, PlayerState voted, RpcInfo info)
        {
            int sender = info.Source;
            bool IsCheating = false;

            if (sender != voter.PlayerId)
            {
                Alert(State.SpawnManager.PlayerStates[sender], "misuse of vote rpc", true);
            }

            if (State.GameModeStateValue.GameMode == GameModes.Infection)
            {
                IsCheating = true;
            }

            if (IsCheating)
            {
                Alert(State.SpawnManager.PlayerStates[sender], "suspicious vote data", true);
            }

            return !IsCheating;
        }

        public bool VerifyKill(PlayerState killer, PlayerState target, int action)
        {
            bool IsCheating = false;

            float distance = (killer.LocomotionPlayer.RigidbodyPosition - target.LocomotionPlayer.RigidbodyPosition).magnitude;
            GameRole killerRole = GetTrueRole(killer);
            GameRole targetRole = GetTrueRole(target);

            if (killer == target)
            {
                IsCheating = true;
            }

            if (CurrentMode.Name != "Hide N Seek")
            {
                if (State.GameModeStateValue.GameMode == GameModes.Infection)
                {
                    if (TargetActionCheck.ContainsKey(killer))
                    {
                        DateTime previousKill = TargetActionCheck[killer];
                        double difference = (DateTime.Now - previousKill).TotalSeconds;

                        if (difference < Kill.TagCooldown.Value - 1)
                        {
                            IsCheating = true;
                        }
                    }
                    else
                    {
                        TargetActionCheck.Add(killer, DateTime.Now);
                    }
                }
                else
                {
                    if (TargetActionCheck.ContainsKey(killer))
                    {
                        DateTime previousKill = TargetActionCheck[killer];
                        double difference = (DateTime.Now - previousKill).TotalSeconds;

                        if (GetTrueRole(killer) == GameRole.Imposter)
                        {
                            if (difference < Kill.KillCooldownVar.Value - 1)
                            {
                                IsCheating = true;
                            }
                        }
                        else
                        {
                            if (difference < Kill.VigilanteKillCooldownVar.Value - 1)
                            {
                                IsCheating = true;
                            }
                        }
                    }
                    else
                    {
                        TargetActionCheck.Add(killer, DateTime.Now);
                    }
                }
            }

            if (distance > 6 || !target.IsAlive)
            {
                IsCheating = true;
            }

            if (action == (int)TargetedAction.Kill)
            {
                if (CurrentMode.Name == "Hide N Seek")
                {
                    if (killerRole != GameRole.Infected || targetRole != GameRole.Crewmember || State.GameModeStateValue.GameMode == GameModes.Infection || !State.InTaskState())
                    {
                        IsCheating = true;
                    }
                }
                else
                {
                    if (killerRole != GameRole.Imposter || targetRole == GameRole.Imposter || State.GameModeStateValue.GameMode == GameModes.Infection || !State.InTaskState())
                    {
                        IsCheating = true;
                    }
                }
                
                if (!IsCheating)
                {
                    AllowedBodySpawns.Add(target);
                }
            }

            if (action == (int)TargetedAction.Neutralize)
            {
                if (killer.ActivePowerUps != PowerUps.Stun || targetRole != GameRole.Infected || State.GameModeStateValue.GameMode != GameModes.Infection || !State.InTaskState())
                {
                    IsCheating = true;
                }
            }

            if (action == (int)TargetedAction.Infect)
            {
                if (killerRole != GameRole.Infected || targetRole == GameRole.Infected || State.GameModeStateValue.GameMode != GameModes.Infection || !State.InTaskState())
                {
                    IsCheating = true;
                }
            }

            if (action == (int)TargetedAction.Guard)
            {
                if (killer.ActivePowerUps != PowerUps.Guard || targetRole != GameRole.Crewmember || State.GameModeStateValue.GameMode != GameModes.Infection || !State.InTaskState())
                {
                    IsCheating = true;
                }
            }

            if (action == (int)TargetedAction.None)
            {
                IsCheating = true;
            }

            if (action == (int)TargetedAction.Vote)
            {
                if (killerRole != GameRole.Sheriff || State.GameModeStateValue.GameMode != GameModes.Sheriff || !State.InVotingState())
                {
                    IsCheating = true;
                }
            }

            if (!IsCheating)
            {
                TargetActionCheck[killer] = DateTime.Now;
            }
            else
            {
                Alert(killer, "suspicious kill data", true);
            }

            return !IsCheating;
        }

        public GameRole GetTrueRole(PlayerState player)
        {
            foreach (Il2CppSystem.Collections.Generic.KeyValuePair<GameRole, Il2CppSystem.Collections.Generic.List<int>> roleEntry in Role.gameRoleToPlayerIds)
            {
                foreach (int id in roleEntry.Value)
                {
                    if (player.IsConnected)
                    {
                        if (id == player.PlayerId)
                        {
                            return roleEntry.Key;
                        }
                    }
                }
            }

            return GameRole.NotSet;
        }

        public void Alert(PlayerState guilty, string reason, bool takeAction)
        {
            if (takeAction)
            {
                if (!BannedUsers.Contains(guilty.PlayerModerationID.Value))
                {
                    Logging.Warn("CHEATER DETECTED! " + guilty.NetworkName.Value + " (" + guilty.PlayerModerationUsername + ", " + guilty.PlayerModerationID.Value + ") was caught cheating. Reason: " + reason + ". Reporting and banning user from lobby.");
                    guilty.NetworkName.Value = "CHEATER";
                    BannedUsers.Add(guilty.PlayerModerationID.Value);
                    SendReportToDevelopers(guilty, reason);
                }
                else
                {
                    Logging.Warn("CHEATER DETECTED! " + guilty.NetworkName.Value + " (" + guilty.PlayerModerationUsername + ", " + guilty.PlayerModerationID.Value + ") was caught cheating. Reason: " + reason + ". Banning user from lobby.");
                    guilty.NetworkName.Value = "CHEATER";
                }

                if (guilty.PlayerId != 9)
                {
                    Moderation.Runner.Disconnect(guilty.PlayerId);
                }
                else
                {
                    SceneManager.LoadScene("Title");
                }

                return;
            }

            Logging.Warn("CHEATER DETECTED! " + guilty.NetworkName.Value + " (" + guilty.PlayerModerationUsername + ", " + guilty.PlayerModerationID.Value + ") is being suspected of cheating. Reason: " + reason + ".");
        }

        public void SendReportToDevelopers(PlayerState guilty, string reason)
        {
            return;

            ReportPlayerPanel Reporting = FindObjectOfType<ReportPlayerPanel>(true);

            if (Reporting != null)
            {
                Reporting.ShowPanel(guilty.PlayerId, guilty);
                Reporting._playerReportAE.ReportCategory = "[AIRLOCK CLIENT | ANTI CHEAT] Category: Cheating/Hacking. Reason provided from Airlock Client: " + reason + ".";
                Reporting.SubmitReport();
            }
        }
    }
}