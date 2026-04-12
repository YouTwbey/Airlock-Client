using AirlockClient.Attributes;
using AirlockClient.Data.Roles.CrownCatchers.Crewmate;
using AirlockClient.Data.Roles.CrownCatchers.Impostor;
using Il2CppFusion;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Network;
using Il2CppSG.Airlock.Roles;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace AirlockClient.Managers.Gamemode
{
    public class CrownRunnersManager : AirlockClientGamemode
    {
        public static SpawnManager Spawn;
        public static RoleManager Rolemanager;
        public static NetworkedKillBehaviour Kill;
        public static GameStateManager state;
        public static Crowned Impostor = null;
        public const int Crown = 246;
        public const int No_Hat = 12;
        public static PlayerState StartingImpostor;
        public static List<int> validplayerIDs = new List<int>();
        public static Dictionary<int, int> savedHatIDs = new Dictionary<int, int>();

        void Start()
        {
            Spawn = FindObjectOfType<SpawnManager>();
            Rolemanager = FindObjectOfType<RoleManager>();
            Kill = FindObjectOfType<NetworkedKillBehaviour>();
            state = FindObjectOfType<GameStateManager>();
        }
        public override bool OnGameStart()
        {
            ModdedGameStateManager.Instance.SetRoleSetting(Data.Enums.RoleIntSettings.SabotageCooldown, 0);
            ModdedGameStateManager.Instance.SetRoleSetting(Data.Enums.RoleBoolSettings.AllowDoorSabotage, false);
            ModdedGameStateManager.Instance.SetMatchSetting(Data.Enums.MatchBoolSettings.ReportBodies, false);
            ModdedGameStateManager.Instance.SetMatchSetting(Data.Enums.MatchIntSettings.LongTasks, 0);
            ModdedGameStateManager.Instance.SetMatchSetting(Data.Enums.MatchIntSettings.NumEmergencyMeetings, 0);
            ModdedGameStateManager.Instance.SetRoleSetting(Data.Enums.RoleIntSettings.NumImposters, 0);
            ModdedGameStateManager.Instance.SetMatchSetting(Data.Enums.MatchIntSettings.ShortTasks, 10);
            ModdedGameStateManager.Instance.SetRoleSetting(Data.Enums.RoleIntSettings.MaxVigilantes, 0);
            ModdedGameStateManager.Instance.SetRoleSetting(Data.Enums.RoleIntSettings.VigilanteNumOfKills, 0);
            ModdedGameStateManager.Instance.SetRoleSetting(Data.Enums.RoleIntSettings.KillCooldown, 0);
            ModdedGameStateManager.Instance.SetRoleSetting(Data.Enums.RoleIntSettings.MaxSheriff, 1);
            ModdedGameStateManager.Instance.SetRoleSetting(Data.Enums.RoleFloatSettings.SheriffSpeedMultiplier, 1.25f);
            ModdedGameStateManager.Instance.SetRoleSetting(Data.Enums.RoleIntSettings.MaxEngineers, 10);
            ModdedGameStateManager.Instance.SetRoleSetting(Data.Enums.RoleIntSettings.MaxTimeInVentsEngineer, 3);
            ModdedGameStateManager.Instance.SetRoleSetting(Data.Enums.RoleIntSettings.VentUseCooldownEngineer, 0);
            foreach (PlayerState player in Spawn.ActivePlayerStates)
            {
                if (!player.IsConnected)
                    continue;

                if (savedHatIDs.ContainsKey(player.PlayerId))
                    continue;

                savedHatIDs.Add(player.PlayerId, player.HatId);
            }
            if (State._preventMatchEnding.Value == false)
            {
                State._preventMatchEnding.Value = true;
            }
            return true;
        }

        public override void OnAfterAssignRoles()
        {
            foreach (SubRole role in SubRole.All)
            {
                Destroy(role);
            }

            foreach (PlayerState player in FindObjectsOfType<PlayerState>())
            {
                if (GetTrueRole(player) == GameRole.Impostor)
                {
                    continue;
                }
                else
                {
                    player.gameObject.AddComponent<Catcher>();
                }
            }

            AlterRandomPersonsRole();
        }

        public static GameTeam GetTrueTeam(PlayerState player)
        {
            foreach (Il2CppSystem.Collections.Generic.KeyValuePair<GameRole, Il2CppSystem.Collections.Generic.List<int>> roleEntry in Rolemanager.gameRoleToPlayerIds)
            {
                foreach (int id in roleEntry.Value)
                {
                    if (player.IsConnected)
                    {
                        if (id == player.PlayerId)
                        {
                            return Rolemanager.GetTeam(roleEntry.Key);
                        }
                    }
                }
            }

            return GameTeam.None;
        }

        public static void AlterRandomPersonsRole()
        {
            foreach (PlayerState player in Spawn.ActivePlayerStates)
            {
                if (!player.IsConnected)
                    continue;

                if (validplayerIDs.Contains(player.PlayerId)) 
                    continue;

                validplayerIDs.Add(player.PlayerId);
            }

            StartingImpostor = GameObject.Find("PlayerState (" + validplayerIDs[Random.Range(0, validplayerIDs.Count)].ToString() + ")").GetComponent<PlayerState>();

            Kill.AlterRole(GameRole.Impostor, StartingImpostor.PlayerId);
            StartingImpostor.gameObject.AddComponent<Crowned>();
            Impostor = StartingImpostor.gameObject.GetComponent<Crowned>();

            if (StartingImpostor.gameObject.GetComponent<Catcher>() != null)
            {
                Destroy(StartingImpostor.gameObject.GetComponent<Catcher>());
            }
        }

        public static GameRole GetTrueRoleCC(PlayerState player)
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

        public override bool OnTargetedAction(ref PlayerState killer, ref PlayerState victim, ref int action)
        {
            if (killer == Impostor.PlayerWithRole && action == (int)ProximityTargetedAction.Vote)
                return false;

            if (victim == Impostor.PlayerWithRole && action == (int)ProximityTargetedAction.Guard)
            {
                Crowned currentHolder = FindObjectOfType<Crowned>();
                if (currentHolder != null)
                {
                    PlayerState oldHolder = currentHolder.GetComponent<PlayerState>();
                    if (oldHolder != null) oldHolder.HatId = savedHatIDs[oldHolder.PlayerId];
                    Destroy(currentHolder);
                }

                var oldKillerRole = killer.GetComponent<Catcher>();
                if (oldKillerRole != null) Destroy(oldKillerRole);

                Kill.AlterRole(GameRole.Sheriff, killer.PlayerId);
                killer.gameObject.AddComponent<Crowned>();

                Kill.AlterRole(GameRole.Engineer, victim.PlayerId);
                victim.gameObject.AddComponent<Catcher>();

                killer.HatId = Crown;
                victim.HatId = savedHatIDs[victim.PlayerId];

                Impostor = killer.GetComponent<Crowned>();

                return false;
            }

            if (killer != Impostor.PlayerWithRole && killer.KnownGameRole == GameRole.Sheriff && (action == (int)ProximityTargetedAction.Guard || action == (int)ProximityTargetedAction.Vote))
            {
                FixKillerBeingDep(killer);
            }

            return false;
        }

        public static void FixKillerBeingDep(PlayerState player)
        {
            Destroy(player.gameObject.GetComponent<Crowned>());

            var catcher = player.gameObject.GetComponent<Catcher>();
            if (catcher == null) player.gameObject.AddComponent<Catcher>();

            Kill.AlterRole(GameRole.Engineer, player.PlayerId);
            player.ActivePowerUps = PowerUps.Guard;
            player.HatId = savedHatIDs[player.PlayerId];
        }

        private bool showDebug = false;

        void Update()
        {
            if (Keyboard.current.numpad0Key.wasPressedThisFrame)
                showDebug = !showDebug;
        }

        public override bool OnGameEnd(ref GameTeam teamThatWon)
        {
            Crowned.playerCrownTimes.Clear();
            Crowned.All.Clear();
            Catcher.All.Clear();
            foreach (PlayerState player in Spawn.ActivePlayerStates)
            {
                player.HatId = savedHatIDs[player.PlayerId];
            }
            return true;
        }

        void OnGUI()
        {
            if (!showDebug) return;

            if (Keyboard.current.numpad1Key.wasPressedThisFrame)
                Impostor.PlayerWithRole.SetNetworkName("Mechanic");

            if (Impostor == null)
            {
                GUI.Label(new Rect(10, 10, 300, 20), "[CrownRunners] No crown holder");
                return;
            }

            GUI.Label(new Rect(10, 10, 300, 150), $"Crown Holder: {Impostor.PlayerWithRole.DisplayName.ToString()} (ID: null)");

            GUI.Label(new Rect(10, 50, 300, 150), $"Crown Holders current Time: {Impostor.PlayersTime}");

            GUI.Label(new Rect(10, 90, 300, 150), $"Crown Holders Amount: {Crowned.All.Count}");
        }
    }
}
