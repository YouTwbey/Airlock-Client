using System;
using System.Collections.Generic;
using AirlockClient.Managers.Debug;
using AirlockClient.Managers;
using AirlockClient.Utils;
using BepInEx.Unity.IL2CPP.Utils;
using SG.Airlock;
using SG.Airlock.Network;
using SG.Airlock.Roles;
using UnityEngine;
using UnityEngine.Networking;
using static AirlockClient.Utils.PlayerStateExtensions;

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
        public List<PlayerState> IsDead = new List<PlayerState>();
        public List<PlayerState> AllowedBodySpawns = new List<PlayerState>();
        public List<string> BannedUsers = new List<string>();
        public List<PlayerState> BodiesReported = new List<PlayerState>();
        public Dictionary<PlayerState, List<PlayerState>> RoleTargets = new Dictionary<PlayerState, List<PlayerState>>();

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
        public ChatManager Chat;
        //SHA256 encrypt;

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
                Chat = FindObjectOfType<ChatManager>();
                //encrypt = SHA256.Create();
            }
            else
            {
                Destroy(this);
            }
        }

        public void OnEndGame()
        {
            IsDead.Clear();
            MeetingsCalled.Clear();
            BodiesReported.Clear();
            AllowedBodySpawns.Clear();
            RoleTargets.Clear();
        }
        /* Kept for futher ref though we probably no longer need it
        List<string> BlacklistedUsers = new List<string>();
        const string BlacklistUrl = "https://raw.githubusercontent.com/YouTwbey/Airlock-Client/main/AC/blacklisted_user_list.txt";
        
        System.Collections.IEnumerator FetchBlacklist()
        {
            while (gameObject != null)
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
                yield return new WaitForSecondsRealtime(300);
            }
        }
        */
        
        private const string ServerBaseUrl = "https://teammesshall-server.tail381a40.ts.net";
        
        public static System.Collections.IEnumerator CheckBanned(string steamId, Action<bool> onResult)
        {
            string url = $"{ServerBaseUrl}/api/isuserbanned/{steamId}";

            UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Logging.Error($"Failed to check ban status for {steamId}: {www.error}");
                onResult(false);
                yield break;
            }

            string body = www.downloadHandler.text.Trim();

            bool banned = bool.TryParse(body, out var parsed) && parsed;

            onResult(banned);
        }
        /*
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
        */
        
        void Update()
        {
            foreach (PlayerState state in State.SpawnManager.ActivePlayerStates)
            {
                if (state == null) continue;
                if (!state.IsSpawned) continue;
                if (!state.IsConnected) continue;
                
                if (!PreviousColor.ContainsKey(state))
                {
                    PreviousColor.Add(state, state.ColorId);
                    PreviousGlove.Add(state, state.HandsId);
                    PreviousHat.Add(state, state.HatId);
                    PreviousSkin.Add(state, state.SkinId);
                }

                state.VerifyState();
            }
        }

        public float GetCooldownForRole(GameRole role, bool whenAlive = true)
        {
            return Role.GetRoleData(role).GetTargetedActionCooldown(whenAlive);
        }

        public bool VerifyJoin(NetworkedLocomotionPlayer joiningPlayer, int color, int hat, int hands, int skin, string Name, string moderationID, string moderationUsername, string accountID, bool is3D)
        {
            bool IsCheating = false;

            if (color < 0 || color > 12)
            {
                IsCheating = true;
            }

            joiningPlayer.PState.PlayerModerationID = joiningPlayer.PState.GetActualModId();

            if (IsCheating)
            {
                Alert(joiningPlayer.PState, "suspicious join data", true);
            }

            return !IsCheating;
        }

        public bool VerifyModerationID(string modId)
        {
            bool isValid = !string.IsNullOrEmpty(modId);

            if (!modId.StartsWith("PS5_") && !modId.StartsWith("Steam_") && !modId.StartsWith("Meta_")) isValid = false;

            return isValid;
        }
        
        // does this not work?
        public static void SendReportToDevelopers(PlayerState guilty, string id, string reason)
        {
            /*
            ReportPlayerPanel Reporting = FindObjectOfType<ReportPlayerPanel>(true);
            
            if (Reporting != null)
            {
                Reporting.ShowPanel(guilty.PlayerId, guilty);
                Reporting._playerReportAE.ReportCategory = "[AIRLOCK CLIENT | ANTI CHEAT] Category: Cheating/Hacking. Reason provided from Airlock Client: " + reason + ".";
                Reporting.SubmitReport();
            }
            */
            
            AirlockClientManager.Instance.StartCoroutine(PostCheaterReport(guilty, id, reason));
        }

        private static System.Collections.IEnumerator PostCheaterReport(PlayerState guilty, string id, string reason)
        {
            string json = BuildReportJson(guilty, id, reason);

            UnityWebRequest www = new UnityWebRequest($"{ServerBaseUrl}/api/cheater-reports", "POST");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Logging.Error($"Failed to send cheater report: {www.error}");
            }
            else
            {
                Logging.Debug_Log("Cheater report sent successfully.");
            }
        }

        private static string BuildReportJson(PlayerState guilty, string ID, string reason)
        {
            string Escape(string s) => s?.Replace("\\", "\\\\").Replace("\"", "\\\"") ?? "";

            string timestamp = DateTimeOffset.UtcNow.ToString("o");

            return "{"
                   + $"\"Username\":\"{Escape(guilty.NetworkName.Value)}\","
                   + $"\"ID\":\"{Escape(ID)}\","
                   + $"\"Reason\":\"{Escape(reason)}\","
                   + $"\"Timestamp\":\"{timestamp}\""
                   + "}";
        }
    }
}