using Fusion;
using SG.Airlock;
using SG.Airlock.UI;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using AirlockAPI.Data;
using AirlockClient.Utils;
using MessHallAPI.Networking;
using SG.Airlock.Network;

namespace AirlockClient.Managers.Dev
{
    public class CommandManager : MonoBehaviour
    {
        private static List<string> QueuedCommands = [];
        public static CommandManager Instance;
        private static readonly Dictionary<string, string> AuthorityUsers = new Dictionary<string, string> {
            {"a70176d5cf6a8aabcc555badf5eb6ccc9d1004d4d35c73975eeaaf33df6ef936", "owner"},
            {"1a66e8559c6c5d986f38f0f68d79e081cf779198aa068674300d53cb410730a5", "admin"},
        };

        private Dictionary<PlayerState, string> NameTagChanged = new Dictionary<PlayerState, string>();
        public UINameTagsDrawer NameTagDrawer;

        private void Start()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }
        }

        public bool requiresUpdate = false;
        void Update()
        {
            if (QueuedCommands.Count != 0)
            {
                foreach (var msg in from message in QueuedCommands let msg = message where message.Contains("UpdateNameTagList") select message.Replace("UpdateNameTagList_[", "") into msg select msg.Replace("]", ""))
                {
                    if (msg.Contains(","))
                    {
                        var playerIds = msg.Split(',');

                        Instance.NameTagChanged.Clear();

                        foreach (var playerId in playerIds)
                        {
                            if (!int.TryParse(playerId, out var id)) continue;
                            
                            var state = GameObject.Find("PlayerState (" + id + ")").GetComponent<PlayerState>();

                            if (Instance.IsVIP(state))
                            {
                                ApplyNametag(state, true);
                            }
                            else
                            {
                                ApplyNametag(state);
                            }
                        }
                    }
                    else
                    {
                        var state = GameObject.Find("PlayerState (" + msg + ")").GetComponent<PlayerState>();

                        if (Instance.IsVIP(state))
                        {
                            ApplyNametag(state, true);
                        }
                        else
                        {
                            ApplyNametag(state);
                        }
                    }
                }

                QueuedCommands.Clear();
            }

            if (NameTagChanged.Count == 0) return;
            {
                foreach (var state in NameTagChanged.Keys.Where(state => state.IsSpawned))
                {
                    if (state.IsConnected)
                    {
                        if (NameTagDrawer == null)
                        {
                            NameTagDrawer = FindObjectOfType<UINameTagsDrawer>(true);
                            if (NameTagDrawer == null) continue;
                        }

                        if (NameTagDrawer._nametagCharacterLimit != 999999)
                        {
                            NameTagDrawer._nametagCharacterLimit = 999999;
                        }

                        var newName = NameTagChanged[state];
                        if (state.LocomotionPlayer._nameTag._storedName == newName) continue;
                        state.LocomotionPlayer.SetNameTagName(newName);
                        state.LocomotionPlayer.OnNetworkNameChange(newName);
                    }
                    else
                    {
                        NameTagChanged.Remove(state);
                    }
                }
            }
        }

        public static void RPC_SendUpdatedNameTagList()
        {
            if (!CurrentMode.IsHosting) return;
            
            Instance.requiresUpdate = false;
            var command = "UpdateNameTagList_[";

            if (Instance.NameTagChanged.Keys.Count == 1)
            {
                command = Instance.NameTagChanged.Keys.Aggregate(command, (current, user) => current + user.PlayerId);
            }
            else
            {
                command = Instance.NameTagChanged.Keys.Aggregate(command, (current, user) => current + (user.PlayerId + ","));
            }

            command += "]";

            NetworkManager.InvokeRPC("com.airlockclient.youtubey", "SendUpdatedNameTagList", command);
        }

        [MessHallRPC(RPCTarget.AllInclusive, RPCCaller.HostOnly)]
        public static void SendUpdatedNameTagList(string cmd)
        {
            QueuedCommands.Add(cmd);
        }

        private static void RPC_ApplyDeveloperNameTag(int playerId)
        {
            NetworkManager.InvokeRPC("com.airlockclient.youtubey","ApplyDevNameTag", playerId);
        }

        [MessHallRPC(RPCTarget.AllInclusive, RPCCaller.HostOnly)]
        public static void ApplyDeveloperNameTag(int playerId)
        {
            Instance.ApplyNametag(ModdedGameStateManager.Instance.state.SpawnManager.PlayerStates[playerId], false);
        }

        private static void RPC_ApplyAdminNameTag(int playerId)
        {
            NetworkManager.InvokeRPC("com.airlockclient.youtubey","ApplyAdminNameTag", playerId);
        }

        [MessHallRPC(RPCTarget.AllInclusive, RPCCaller.HostOnly)]
        public static void ApplyAdminNameTag(int playerId)
        {
            Instance.ApplyNametag(ModdedGameStateManager.Instance.state.SpawnManager.PlayerStates[playerId], true);
        }

        public static void RPC_SendCommand(string cmd)
        {
            NetworkManager.InvokeRPC("com.airlockclient.youtubey","SendCommand", cmd);
        }

        [MessHallRPC(RPCTarget.Host, RPCCaller.Anyone)]
        public static void SendCommand(string cmd, [RPCInfo] MessHallRpcInfo info)
        {
            Instance.HandleCommand(cmd, info.Sender);
        }

        public static bool CheckAuthorityValidation(PlayerState player)
        {
            if (IsDeveloper(player))
            {
                return true;
            }
            else if (Instance.IsVIP(player))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static void RPC_ApplyMatID(int playerId, int id)
        {
            NetworkManager.InvokeRPC("com.airlockclient.youtubey","ApplyMatID",playerId, id);
        }

        [MessHallRPC(RPCTarget.AllInclusive, RPCCaller.HostOnly)]
        public static void ApplyMatID(int playerId, int id)
        {
            var player = ModdedGameStateManager.Instance.state.SpawnManager.Avatars[playerId];
            player.SetInstancedMatColorID(id);
        }

        public void ApplyNametag(PlayerState player, bool isAdmin = false)
        {
            if (NameTagChanged.ContainsKey(player)) return;
            
            if (isAdmin)
            {
                if (!IsVIP(player)) return;
            }
            else
            {
                if (!IsDeveloper(player)) return;
            }

            FindObjectOfType<UINameTagsDrawer>(true)._nametagCharacterLimit = 999999;
            var newName = "";

            if (isAdmin)
            {
                newName = "<color=yellow><b>[VIP]</b></color> " + ApplyGradient(player.NetworkName.Value);
            }
            else
            {
                newName = "<color=blue><b>[DEV]</b></color> " + ApplyRainbow(player.NetworkName.Value);
            }

            NameTagChanged.Add(player, newName);
            requiresUpdate = true;
        }

        private static string HSVToHex(float h, float s = 1f, float v = 1f)
        {
            var color = Color.HSVToRGB(h, s, v);
            var r = Mathf.RoundToInt(color.r * 255f);
            var g = Mathf.RoundToInt(color.g * 255f);
            var b = Mathf.RoundToInt(color.b * 255f);
            return $"{r:X2}{g:X2}{b:X2}";
        }

        private static string ApplyRainbow(string text)
        {
            var rainbow = "";
            if (string.IsNullOrEmpty(text))
                return "";

            var len = text.Length;

            for (var i = 0; i < len; i++)
            {
                var hue = (float)i / len;
                var hexColor = HSVToHex(hue);
                rainbow += $"<color=#{hexColor}>{text[i]}</color>";
            }

            return rainbow;
        }

        private static string ApplyGradient(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            var gradient = "";
            var len = text.Length;

            var startColor = new Color32(64, 64, 64, 255);
            var endColor = new Color32(255, 255, 255, 255);

            for (var i = 0; i < len; i++)
            {
                var t = len > 1 ? (float)i / (len - 1) : 0f;
                var hexColor = LerpToHex(startColor, endColor, t);
                gradient += $"<color=#{hexColor}>{text[i]}</color>";
            }

            return gradient;
        }

        private static string LerpToHex(Color32 a, Color32 b, float t)
        {
            var r = (byte)Mathf.RoundToInt(Mathf.Lerp(a.r, b.r, t));
            var g = (byte)Mathf.RoundToInt(Mathf.Lerp(a.g, b.g, t));
            var bl = (byte)Mathf.RoundToInt(Mathf.Lerp(a.b, b.b, t));

            return $"{r:X2}{g:X2}{bl:X2}";
        }
        

        public void HandleCommand(string command, PlayerRef sender)
        {
            if (!CanUserRunCommand(command, sender)) return;
            var cmd = FormatCommand(command);
            var caller = ModdedGameStateManager.Instance.state.SpawnManager.PlayerStates[sender.PlayerId];

            if (command.Contains("DEV"))
            {
                if (cmd.Contains("ApplyNametag"))
                {
                    RPC_ApplyDeveloperNameTag(sender.PlayerId);
                }
                if (cmd.Contains("SetMatID"))
                {
                    RPC_ApplyMatID(sender.PlayerId, int.Parse(cmd.Replace("_SetMatID_", "")));
                }
            }

            if (!command.Contains("ADMIN")) return;
            
            if (cmd.Contains("ApplyNametag"))
            {
                RPC_ApplyAdminNameTag(sender.PlayerId);
            }
        }

        private bool CanUserRunCommand(string command, PlayerRef sender)
        {
            var player = ModdedGameStateManager.Instance.state.SpawnManager.PlayerStates[sender.PlayerId];

            if (command.StartsWith("ADMINCOMMAND"))
            {
                if (IsVIP(player) || IsDeveloper(player))
                {
                    return true;
                }
            }
            else if (command.StartsWith("DEVCOMMAND"))
            {
                if (IsDeveloper(player))
                {
                    return true;
                }
            }

            return false;
        }

        public void CheckAuthorityForNameTag(PlayerState player)
        {
            if (IsDeveloper(player))
            {
                RPC_ApplyDeveloperNameTag(player.PlayerId);
            }
            else
            {
                RPC_ApplyAdminNameTag(player.PlayerId);
            }
        }

        private static bool IsDeveloper(PlayerState player)
        {
            var user = Encrypt(player.GetActualModId());

            if (AuthorityUsers.TryGetValue(user, out var authorityUser))
            {
                return authorityUser == "owner";
            }
            return false;
        }

        private bool IsVIP(PlayerState player)
        {
            var user = Encrypt(player.GetActualModId());

            if (AuthorityUsers.ContainsKey(user))
            {
                return AuthorityUsers[user] == "admin" || AuthorityUsers[user] == "admin_b";
            }
            
            return false;
        }

        private static string FormatCommand(string input)
        {
            if (input.Contains("ADMIN"))
            {
                input = input.Replace("ADMIN", "");
            }
            if (input.Contains("DEV"))
            {
                input = input.Replace("DEV", "");
            }
            if (input.Contains("COMMAND"))
            {
                input = input.Replace("COMMAND", "");
            }

            return input;
        }

        private static string Encrypt(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha256.ComputeHash(bytes);

            var sb = new StringBuilder();
            foreach (var b in hash)
                sb.Append(b.ToString("x2"));
            
            return sb.ToString();
        }
    }
}
