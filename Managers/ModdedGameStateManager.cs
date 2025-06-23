using AirlockAPI.Attributes;
using AirlockAPI.Data;
using AirlockClient.Attributes;
using AirlockClient.Core;
using AirlockClient.Data;
using AirlockClient.Managers.Debug;
using AirlockClient.Managers.Dev;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Roles;
using Il2CppSG.Airlock.Settings;
using System.Collections.Generic;
using UnityEngine;
using static AirlockAPI.Managers.NetworkManager;

namespace AirlockClient.Managers
{
    public class ModdedGameStateManager : MonoBehaviour
    {
        public static ModdedGameStateManager Instance;
        public GameStateManager state;
        QueuedWin queuedWin;

        public static void RPC_JoinedModdedGame(int playerId)
        {
            SendRpc("JoinedModdedGame", playerId);
        }

        [AirlockRpc("JoinedModdedGame", RpcTarget.All, RpcCaller.Host)]
        public static void JoinedModdedGame()
        {
            Base.SceneStorage.AddComponent<PetManager>();
            Base.SceneStorage.AddComponent<CommandManager>();
        }

        void Start()
        {
            if (Instance == null)
            {
                Instance = this;
                state = FindObjectOfType<GameStateManager>();
            }
            else
            {
                Destroy(this);
            }
        }

        public void QueueWin(List<PlayerState> winningPlayers, int reason = -1, GameplayStates runInState = GameplayStates.NotSet, int authority = 99999)
        {
            if (queuedWin != null)
            {
                if (queuedWin.Authority < authority)
                {
                    Logging.Debug_Log("Win was overruled. " + queuedWin.Authority + " < " + authority);
                    return;
                }
            }

            if (reason == -1)
            {
                reason = 999999999;
            }

            state._preventMatchEnding.Value = true;

            foreach (PlayerState player in winningPlayers)
            {
                player.RPC_KnownGameRole(GameRole.Jester);
                state.RoleManager.AlterPlayerRole(GameRole.Jester, player.PlayerId);
            }

            queuedWin = new QueuedWin { WinningPlayers = winningPlayers, Reason = reason, RunInState = runInState };
        }

        public void QueueWin(PlayerState winningPlayer, int reason = -1, GameplayStates runInState = GameplayStates.NotSet, int authority = 99999)
        {
            QueueWin(new List<PlayerState> { winningPlayer }, reason, runInState, authority);
        }

        public BoolSettingsItem GetMatchSetting(Enums.MatchBoolSettings setting)
        {
            foreach (BoolSettingsItem set in state.MatchSettings.LocalBoolSettings)
            {
                if (set.name.EndsWith(setting.ToString()))
                {
                    return set;
                }
            }

            return null;
        }

        public FloatSettingsItem GetMatchSetting(Enums.MatchFloatSettings setting)
        {
            foreach (FloatSettingsItem set in state.MatchSettings.LocalFloatSettings)
            {
                if (set.name.EndsWith(setting.ToString()))
                {
                    return set;
                }
            }

            return null;
        }

        public IntSettingsItem GetMatchSetting(Enums.MatchIntSettings setting)
        {
            foreach (IntSettingsItem set in state.MatchSettings.LocalIntSettings)
            {
                if (set.name.EndsWith(setting.ToString()))
                {
                    return set;
                }
            }

            return null;
        }

        public void SetMatchSetting(Enums.MatchBoolSettings setting, bool value)
        {
            foreach (BoolSettingsItem set in state.MatchSettings.LocalBoolSettings)
            {
                if (set.name.Contains(setting.ToString()))
                {
                    set.Variable.Value = value;
                }
            }
        }

        public void SetMatchSetting(Enums.MatchFloatSettings setting, float value)
        {
            foreach (FloatSettingsItem set in state.MatchSettings.LocalFloatSettings)
            {
                if (set.name.Contains(setting.ToString()))
                {
                    set.Variable.Value = value;
                }
            }
        }

        public void SetMatchSetting(Enums.MatchIntSettings setting, int value)
        {
            foreach (IntSettingsItem set in state.MatchSettings.LocalIntSettings)
            {
                if (set.name.Contains(setting.ToString()))
                {
                    set.Variable.Value = value;
                }
            }
        }

        public BoolSettingsItem GetRoleSetting(Enums.RoleBoolSettings setting)
        {
            foreach (BoolSettingsItem set in GameObject.Find("RoleCustomizationSettings").GetComponent<MatchCustomizationSettings>().LocalBoolSettings)
            {
                if (set.name.EndsWith(setting.ToString()))
                {
                    return set;
                }
            }

            return null;
        }

        public FloatSettingsItem GetRoleSetting(Enums.RoleFloatSettings setting)
        {
            foreach (FloatSettingsItem set in GameObject.Find("RoleCustomizationSettings").GetComponent<MatchCustomizationSettings>().LocalFloatSettings)
            {
                if (set.name.EndsWith(setting.ToString()))
                {
                    return set;
                }
            }

            return null;
        }

        public IntSettingsItem GetRoleSetting(Enums.RoleIntSettings setting)
        {
            foreach (IntSettingsItem set in GameObject.Find("RoleCustomizationSettings").GetComponent<MatchCustomizationSettings>().LocalIntSettings)
            {
                if (set.name.EndsWith(setting.ToString()))
                {
                    return set;
                }
            }

            return null;
        }

        public void SetRoleSetting(Enums.RoleBoolSettings setting, bool value)
        {
            foreach (BoolSettingsItem set in GameObject.Find("RoleCustomizationSettings").GetComponent<MatchCustomizationSettings>().LocalBoolSettings)
            {
                if (set.name.Contains(setting.ToString()))
                {
                    set.Variable.Value = value;
                }
            }
        }

        public void SetRoleSetting(Enums.RoleFloatSettings setting, float value)
        {
            foreach (FloatSettingsItem set in GameObject.Find("RoleCustomizationSettings").GetComponent<MatchCustomizationSettings>().LocalFloatSettings)
            {
                if (set.name.Contains(setting.ToString()))
                {
                    set.Variable.Value = value;
                }
            }
        }

        public void SetRoleSetting(Enums.RoleIntSettings setting, int value)
        {
            foreach (IntSettingsItem set in GameObject.Find("RoleCustomizationSettings").GetComponent<MatchCustomizationSettings>().LocalIntSettings)
            {
                if (set.name.Contains(setting.ToString()))
                {
                    set.Variable.Value = value;
                }
            }
        }

        void Update()
        {
            if (CurrentMode.Name == "More Roles")
            {
                if (!state.InLobbyState())
                {
                    if (queuedWin != null)
                    {
                        if (queuedWin.RunInState == GameplayStates.NotSet || queuedWin.RunInState == state.GameModeStateValue.GameState)
                        {
                            state.GameEndReasonIndex = queuedWin.Reason;
                            state.EndGame(GameTeam.Jester);
                            queuedWin = null;
                        }
                    }
                }
                else
                {
                    state._preventMatchEnding.Value = false;
                }
            }
        }
    }
}
