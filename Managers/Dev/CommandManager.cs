using Il2CppFusion;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.UI;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using AirlockAPI.Attributes;
using static AirlockAPI.Managers.NetworkManager;
using AirlockAPI.Data;
using Il2CppSG.Airlock.Network;

namespace AirlockClient.Managers.Dev
{
    public class CommandManager : MonoBehaviour
    {
        public static List<string> QueuedCommands = new List<string>();
        public static CommandManager Instance;
        public static readonly Dictionary<string, string> AuthorityUsers = new Dictionary<string, string> {
            {"a70176d5cf6a8aabcc555badf5eb6ccc9d1004d4d35c73975eeaaf33df6ef936", "owner"},
            {"1a66e8559c6c5d986f38f0f68d79e081cf779198aa068674300d53cb410730a5", "admin"}
        };

        public Dictionary<PlayerState, string> NameTagChanged = new Dictionary<PlayerState, string>();
        public UINameTagsDrawer NameTagDrawer;

        void Start()
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
                foreach (string message in QueuedCommands)
                {
                    string msg = message;
                    if (message.Contains("UpdateNameTagList"))
                    {
                        msg = message.Replace("UpdateNameTagList_[", "");
                        msg = msg.Replace("]", "");

                        if (msg.Contains(","))
                        {
                            string[] playerIds = msg.Split(',');

                            Instance.NameTagChanged.Clear();

                            foreach (string playerId in playerIds)
                            {
                                if (int.TryParse(playerId, out int id))
                                {
                                    PlayerState state = GameObject.Find("PlayerState (" + id + ")").GetComponent<PlayerState>();

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
                        }
                        else
                        {
                            PlayerState state = GameObject.Find("PlayerState (" + msg + ")").GetComponent<PlayerState>();

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
                }

                QueuedCommands.Clear();
            }

            if (NameTagChanged.Count != 0)
            {
                foreach (PlayerState state in NameTagChanged.Keys)
                {
                    if (!state.IsSpawned) continue;

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

                        string newName = NameTagChanged[state];
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
            if (CurrentMode.IsHosting)
            {
                Instance.requiresUpdate = false;
                string command = "UpdateNameTagList_[";

                if (Instance.NameTagChanged.Keys.Count == 1)
                {
                    foreach (PlayerState user in Instance.NameTagChanged.Keys)
                    {
                        command += user.PlayerId;
                    }
                }
                else
                {
                    foreach (PlayerState user in Instance.NameTagChanged.Keys)
                    {
                        command += user.PlayerId + ",";
                    }
                }

                command += "]";

                SendRpc("SendUpdatedNameTagList", command);
            }
        }

        [AirlockRpc("SendUpdatedNameTagList", RpcTarget.AllInclusive, RpcCaller.Host)]
        public static void SendUpdatedNameTagList(string cmd)
        {
            QueuedCommands.Add(cmd);
        }

        public static void RPC_ApplyDeveloperNameTag(int playerId)
        {
            SendRpc("ApplyDevNameTag", -1, playerId);
        }

        [AirlockRpc("ApplyDevNameTag", RpcTarget.AllInclusive, RpcCaller.Host)]
        public static void ApplyDeveloperNameTag(int playerId)
        {
            Instance.ApplyNametag(ModdedGameStateManager.Instance.state.SpawnManager.PlayerStates[playerId], false);
        }

        public static void RPC_ApplyAdminNameTag(int playerId)
        {
            SendRpc("ApplyAdminNameTag", -1, playerId);
        }

        [AirlockRpc("ApplyAdminNameTag", RpcTarget.AllInclusive, RpcCaller.Host)]
        public static void ApplyAdminNameTag(int playerId)
        {
            Instance.ApplyNametag(ModdedGameStateManager.Instance.state.SpawnManager.PlayerStates[playerId], true);
        }

        public static void RPC_SendCommand(string cmd)
        {
            SendRpc("SendCommand", cmd);
        }

        [AirlockRpc("SendCommand", RpcTarget.Host, RpcCaller.All)]
        public static void SendCommand(string cmd, AirlockRpcInfo info)
        {
            Instance.HandleCommand(cmd, info.Sender);
        }

        public static bool CheckAuthoryValidation(PlayerState player)
        {
            if (Instance.IsDeveloper(player))
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

        public static void RPC_ApplyMatID(int playerId, int id)
        {
            SendRpc("ApplyMatID", -1, playerId, id);
        }

        [AirlockRpc("ApplyMatID", RpcTarget.AllInclusive, RpcCaller.Host)]
        public static void ApplyMatID(int playerId, int id)
        {
            NetworkedLocomotionPlayer player = ModdedGameStateManager.Instance.state.SpawnManager.Avatars[playerId];
            player.SetInstancedMatColorID(id);
        }

        public void ApplyNametag(PlayerState player, bool isAdmin = false)
        {
            if (!NameTagChanged.ContainsKey(player))
            {
                if (isAdmin)
                {
                    if (!IsVIP(player)) return;
                }
                else
                {
                    if (!IsDeveloper(player)) return;
                }

                FindObjectOfType<UINameTagsDrawer>(true)._nametagCharacterLimit = 999999;
                string newName = "";

                if (isAdmin)
                {
                    newName = "<color=yellow><b>[VIP]</b></color> " + ApplyRainbow(player.NetworkName.Value);
                }
                else
                {
                    newName = "<color=blue><b>[DEV]</b></color> " + ApplyRainbow(player.NetworkName.Value);
                }

                NameTagChanged.Add(player, newName);
                requiresUpdate = true;
            }
        }

        static string HSVToHex(float h, float s = 1f, float v = 1f)
        {
            Color color = Color.HSVToRGB(h, s, v);
            int r = Mathf.RoundToInt(color.r * 255f);
            int g = Mathf.RoundToInt(color.g * 255f);
            int b = Mathf.RoundToInt(color.b * 255f);
            return $"{r:X2}{g:X2}{b:X2}";
        }

        static string ApplyRainbow(string text)
        {
            string rainbow = "";
            if (string.IsNullOrEmpty(text))
                return "";

            int len = text.Length;

            for (int i = 0; i < len; i++)
            {
                float hue = (float)i / len;
                string hexColor = HSVToHex(hue);
                rainbow += $"<color=#{hexColor}>{text[i]}</color>";
            }

            return rainbow;
        }

        public void HandleCommand(string command, PlayerRef sender)
        {
            if (CanUserRunCommand(command, sender))
            {
                string cmd = FormatCommand(command);
                PlayerState caller = ModdedGameStateManager.Instance.state.SpawnManager.PlayerStates[sender.PlayerId];

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

                if (command.Contains("ADMIN"))
                {
                    if (cmd.Contains("ApplyNametag"))
                    {
                        RPC_ApplyAdminNameTag(sender.PlayerId);
                    }
                }
            }
        }

        public bool CanUserRunCommand(string command, PlayerRef sender)
        {
            PlayerState player = ModdedGameStateManager.Instance.state.SpawnManager.PlayerStates[sender.PlayerId];

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
            if (Instance.IsDeveloper(player))
            {
                RPC_ApplyDeveloperNameTag(player.PlayerId);
            }
            else
            {
                RPC_ApplyAdminNameTag(player.PlayerId);
            }
        }

        public bool IsDeveloper(PlayerState player)
        {
            string user = Encrypt(player.PlayerModerationID.Value);

            if (AuthorityUsers.ContainsKey(user))
            {
                return AuthorityUsers[user] == "owner";
            }
            return false;
        }

        public bool IsVIP(PlayerState player)
        {
            string user = Encrypt(player.PlayerModerationID.Value);

            if (AuthorityUsers.ContainsKey(user))
            {
                return AuthorityUsers[user] == "admin";
            }
            return false;
        }

        public string FormatCommand(string input)
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

        public string Encrypt(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(input);
                byte[] hash = sha256.ComputeHash(bytes);

                StringBuilder sb = new StringBuilder();
                foreach (byte b in hash)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}
