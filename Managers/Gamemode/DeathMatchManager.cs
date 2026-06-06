using AirlockClient.AC;
using AirlockClient.Attributes;
using AirlockClient.Data.Roles.CrownCatchers.Crewmate;
using AirlockClient.Data.Roles.CrownCatchers.Impostor;
using AirlockClient.Data.Roles.DeathMatch.Crewmates;
using AirlockClient.Data.Roles.DeathMatch.Impostors;
using AirlockClient.Managers.Debug;
using Il2CppFusion;
using Il2CppFusion.CodeGen;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Minigames;
using Il2CppSG.Airlock.Network;
using Il2CppSG.Airlock.Roles;
using Il2CppSG.Airlock.Sabotage;
using MelonLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

namespace AirlockClient.Managers.Gamemode
{
    public class DeathMatchManager : AirlockClientGamemode
    {
        public SabotageManager sabotage;
        public DoorsSabotage doors;
        public MinigameManager tasks;
        public static SpawnManager Spawn;
        public static NetworkedKillBehaviour Kill;
        public static RoleManager Rolemanager;
        public static List<int> validPlayerids = new List<int>();
        public static Dictionary<int, string> playerNames = new Dictionary<int, string>();
        public static Dictionary<int, int> PlayerHats = new Dictionary<int, int>();
        public static Dictionary<int, int> PlayerSkins = new Dictionary<int, int>();
        public static Dictionary<int, int> PlayerHands = new Dictionary<int, int>();
        public static int VigilanteTeamPoints = 0;
        public static int ImpostorTeamPoints = 0;
        public static bool KilledAllPlayers = false;

        void Start()
        {
            sabotage = FindObjectOfType<SabotageManager>();
            doors = FindObjectOfType<DoorsSabotage>();
            tasks = FindObjectOfType<MinigameManager>();
            Spawn = FindObjectOfType<SpawnManager>();
            Kill = FindObjectOfType<NetworkedKillBehaviour>();
            Rolemanager = FindObjectOfType<RoleManager>();
        }

        public override bool OnGameStart()
        {

            if (ModdedGameStateManager.Instance == null)
            {
                Logging.Warn("ModdedGameStateManager Is null please report this error");
                return true;
            }
            else
            {
                ModdedGameStateManager.Instance.SetRoleSetting(Data.Enums.RoleIntSettings.SabotageCooldown, -1);
                ModdedGameStateManager.Instance.SetRoleSetting(Data.Enums.RoleBoolSettings.AllowDoorSabotage, false);
                ModdedGameStateManager.Instance.SetMatchSetting(Data.Enums.MatchBoolSettings.ReportBodies, false);
                ModdedGameStateManager.Instance.SetMatchSetting(Data.Enums.MatchIntSettings.LongTasks, 0);
                ModdedGameStateManager.Instance.SetMatchSetting(Data.Enums.MatchIntSettings.NumEmergencyMeetings, 0);
                ModdedGameStateManager.Instance.SetRoleSetting(Data.Enums.RoleIntSettings.NumImposters, 1);
                ModdedGameStateManager.Instance.SetMatchSetting(Data.Enums.MatchIntSettings.MaxInfected, 0);
                ModdedGameStateManager.Instance.SetMatchSetting(Data.Enums.MatchIntSettings.TagCooldown, 999);
                ModdedGameStateManager.Instance.SetMatchSetting(Data.Enums.MatchIntSettings.ShortTasks, 0);
                ModdedGameStateManager.Instance.SetRoleSetting(Data.Enums.RoleIntSettings.MaxVigilantes, 1);
                ModdedGameStateManager.Instance.SetRoleSetting(Data.Enums.RoleIntSettings.VigilanteKillCooldown, 0);
                ModdedGameStateManager.Instance.SetRoleSetting(Data.Enums.RoleIntSettings.VigilanteNumOfKills, 50);
                ModdedGameStateManager.Instance.SetRoleSetting(Data.Enums.RoleIntSettings.KillCooldown, 0);
                ModdedGameStateManager.Instance.SetMatchSetting(Data.Enums.MatchIntSettings.TagNumTasksAssigned, 0);
                ModdedGameStateManager.Instance.SetMatchSetting(Data.Enums.MatchIntSettings.TagTotalTasks, 9999);
                if (State._preventMatchEnding.Value == false)
                {
                    State._preventMatchEnding.Value = true;
                }
                return true;
            }
        }

        public override bool OnTargetedAction(ref PlayerState killer, ref PlayerState victim, ref int action)
        {
            if (GetTrueRoleDM(killer) == GameRole.Vigilante && GetTrueRoleDM(victim) == GameRole.Vigilante)
            {
                return false;
            }

            else return true;
        }

        public override void OnAfterAssignRoles()
        {
            RandomlyAssign10Roles();
        }

        public static void RandomlyAssign10Roles()
        {
            validPlayerids.Clear();
            playerNames.Clear();
            PlayerHats.Clear();
            PlayerSkins.Clear();
            PlayerHands.Clear();

            foreach (PlayerState player in Spawn.ActivePlayerStates)
            {
                if (!player.IsConnected)
                    continue;
                validPlayerids.Add(player.PlayerId);

                playerNames[player.PlayerId] = player.NetworkName.ToString();
                PlayerHats[player.PlayerId] = player.HatId;
                PlayerSkins[player.PlayerId] = player.SkinId;
                PlayerHands[player.PlayerId] = player.HandsId;
            }

            List<int> shuffled = new List<int>(validPlayerids);
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
            }

            int playerCount = shuffled.Count;
            int impostorCount = playerCount / 2;
            int vigilanteCount = playerCount - impostorCount;

            List<GameRole> rolesToAssign = new List<GameRole>();
            for (int i = 0; i < vigilanteCount; i++) rolesToAssign.Add(GameRole.Vigilante);
            for (int i = 0; i < impostorCount; i++) rolesToAssign.Add(GameRole.Impostor);

            for (int i = rolesToAssign.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (rolesToAssign[i], rolesToAssign[j]) = (rolesToAssign[j], rolesToAssign[i]);
            }

            int count = Math.Min(shuffled.Count, rolesToAssign.Count);
            for (int i = 0; i < count; i++)
            {
                PlayerState target = null;
                foreach (PlayerState p in Spawn.ActivePlayerStates)
                {
                    if (p.PlayerId == shuffled[i])
                    {
                        target = p;
                        break;
                    }
                }
                if (target == null) continue;
                Kill.AlterRole(rolesToAssign[i], shuffled[i]);

                string rawName = target.NetworkName.ToString();
                if (rawName.StartsWith("<#") && rawName.Contains(">"))
                    rawName = rawName.Substring(rawName.IndexOf('>') + 1);

                if (rolesToAssign[i] == GameRole.Vigilante)
                {
                    target.gameObject.AddComponent<Vigilante>();
                    target.NetworkName = "<#0FF>" + rawName;
                    target.HatId = 74;
                    target.SkinId = 161;
                    target.HandsId = 128;
                }
                else if (rolesToAssign[i] == GameRole.Impostor)
                {
                    target.gameObject.AddComponent<Impostors>();
                    target.NetworkName = "<#F00>" + rawName;
                    target.HatId = 115;
                    target.SkinId = 161;
                    target.HandsId = 132;
                }
            }
        }

        public override bool OnGameEnd(ref GameTeam teamThatWon)
        {
            foreach (PlayerState player in Spawn.ActivePlayerStates)
            {
                if (!player.IsConnected) continue;

                if (playerNames.ContainsKey(player.PlayerId))
                    player.NetworkName = playerNames[player.PlayerId];
                if (PlayerHats.ContainsKey(player.PlayerId))
                    player.HatId = PlayerHats[player.PlayerId];
                if (PlayerSkins.ContainsKey(player.PlayerId))
                    player.SkinId = PlayerSkins[player.PlayerId];
                if (PlayerHands.ContainsKey(player.PlayerId))
                    player.HandsId = PlayerHands[player.PlayerId];
            }

            VigilanteTeamPoints = 0;
            ImpostorTeamPoints = 0;
            KilledAllPlayers = false;
            return true;
        }

        public static GameRole GetTrueRoleDM(PlayerState player)
        {
            foreach (Il2CppSystem.Collections.Generic.KeyValuePair<GameRole, Il2CppSystem.Collections.Generic.List<int>> roleEntry in Rolemanager.gameRoleToPlayerIds)
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

        public static bool showDebug;

        void Update()
        {
            if (Keyboard.current.numpad0Key.wasPressedThisFrame)
                showDebug = !showDebug;

            if (KilledAllPlayers == false && State.GameModeStateValue.GameState == GameplayStates.Task)
            {
                MelonCoroutines.Start(DelayedKillAllPlayers(1));
            }

            if (VigilanteTeamPoints >= 50)
            {
                State.EndGame(GameTeam.Crewmember);
            }
            if (ImpostorTeamPoints >= 50)
            {
                State.EndGame(GameTeam.Impostor);
            }
        }

        void OnGUI()
        {
            if (!showDebug) return;

            GUI.Label(new Rect(10, 10, 300, 150), $"Team Impostor Points: {ImpostorTeamPoints}");

            GUI.Label(new Rect(10, 50, 300, 150), $"Team Vigilante Points: {VigilanteTeamPoints}");
        }

        public IEnumerator DelayedKillAllPlayers(int delay)
        {
            KilledAllPlayers = true;

            yield return new WaitForSeconds(delay);

            foreach (PlayerState player in Spawn.ActivePlayerStates)
            {
                if (!player.IsConnected)
                    continue;
                if (GetTrueRoleDM(player) == GameRole.Impostor)
                {
                    AntiCheat.KillPlayerWithAntiCheat(player, player);
                }

                player.IsAlive = true;
            }
        }
    }
}