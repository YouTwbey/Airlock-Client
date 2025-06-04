using AirlockClient.Data;
using Il2CppFusion;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.UI;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace AirlockClient.Managers.Dev
{
    public class CommandManager : MonoBehaviour
    {
        public static List<string> QueuedCommands = new List<string>();
        public static CommandManager Instance;
        public static Dictionary<string, string> AuthorityUsers = new Dictionary<string, string> {
            {"a70176d5cf6a8aabcc555badf5eb6ccc9d1004d4d35c73975eeaaf33df6ef936", "owner"},
            {"1a66e8559c6c5d986f38f0f68d79e081cf779198aa068674300d53cb410730a5", "admin"}
        };

        public Dictionary<PlayerState, string> NameTagChanged = new Dictionary<PlayerState, string>();

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
                    if (message == null) continue;

                    string msg = message;
                    if (message.Contains("UpdateNameTagList"))
                    {
                        msg = message.Replace("UpdateNameTagList_[", "");
                        msg = msg.Replace("]", "");

                        string[] playerIds = msg.Split(',');

                        Instance.NameTagChanged.Clear();

                        foreach (string playerId in playerIds)
                        {
                            if (int.TryParse(playerId, out int id))
                            {
                                PlayerState state = GameObject.Find("PlayerState (" + id + ")").GetComponent<PlayerState>();

                                string cmd = "COMMAND";

                                if (Instance.IsVIP(state))
                                {
                                    cmd = "ADMIN" + cmd;
                                }
                                else
                                {
                                    cmd = "DEV" + cmd;
                                }

                                cmd += "_ApplyNametag";

                                Instance.HandleCommand(cmd, id);
                            }
                        }
                    }
                }

                QueuedCommands.Clear();
            }

            if (NameTagChanged.Count != 0)
            {
                foreach (PlayerState user in NameTagChanged.Keys)
                {
                    if (user == null) continue;

                    if (user.IsSpawned)
                    {
                        if (user.IsConnected)
                        {
                            string newName = "";

                            if (IsVIP(user))
                            {
                                newName = "<color=yellow><b>[VIP]</b></color> " + ApplyRainbow(user.NetworkName.Value);
                            }
                            else
                            {
                                newName = "<color=blue><b>[DEV]</b></color> " + ApplyRainbow(user.NetworkName.Value);
                            }
                            NameTagChanged[user] = newName;

                            user.LocomotionPlayer.OnNetworkNameChange(NameTagChanged[user]);
                            user.LocomotionPlayer.SetNameTagName(NameTagChanged[user]);
                            //if (user.LocomotionPlayer._nameTag._storedName.Contains(user.NetworkName.Value))
                            //{
                            //}
                        }
                        else
                        {
                            NameTagChanged.Remove(user);
                            requiresUpdate = true;
                        }
                    }
                }
            }

            if (requiresUpdate)
            {
                RPC_SendUpdatedNameTagList();
            }
        }

        public static void RPC_SendUpdatedNameTagList()
        {
            if (CurrentMode.IsHosting)
            {
                Instance.requiresUpdate = false;
                string command = "UpdateNameTagList_[";

                foreach (PlayerState user in Instance.NameTagChanged.Keys)
                {
                    command += user.PlayerId + ",";
                }

                command += "]";
                Listener.Send(command);
            }
        }

        public static void RPC_ApplyDeveloperNameTag(int id)
        {
            string command = "DEVCOMMAND_ApplyNametag";

            Instance.HandleCommand(command, id);
        }

        public static void RPC_ApplyAdminNameTag(int id)
        {
            string command = "ADMINCOMMAND_ApplyNametag";

            Instance.HandleCommand(command, id);
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

        public void ApplyMatID(PlayerState player, int id)
        {
            player.LocomotionPlayer.SetInstancedMatColorID(id);
        }

        public void ApplyNametag(PlayerState player, bool isAdmin = false)
        {
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
                        ApplyNametag(caller);
                    }
                    if (cmd.Contains("SetMatID"))
                    {
                        ApplyMatID(caller, int.Parse(cmd.Replace("_SetMatID_", "")));
                    }
                }

                if (command.Contains("ADMIN"))
                {
                    if (cmd.Contains("ApplyNametag"))
                    {
                        ApplyNametag(caller, true);
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
