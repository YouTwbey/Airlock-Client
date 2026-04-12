using AirlockAPI.Attributes;
using AirlockAPI.Data;
using AirlockClient.AC;
using AirlockClient.Attributes;
using AirlockClient.Core;
using AirlockClient.Data.Roles.MoreRoles.Crewmate;
using AirlockClient.Data.Roles.MoreRoles.Imposter;
using AirlockClient.Data.Roles.MoreRoles.Modifiers;
using AirlockClient.Data.Roles.MoreRoles.Neutral;
using AirlockClient.Managers.Debug;
using Il2CppInterop.Runtime;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Network;
using Il2CppSG.Airlock.Roles;
using Il2CppSystem.Runtime.Serialization;
using Il2CppTMPro;
using MelonLoader;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static AirlockAPI.Managers.NetworkManager;

namespace AirlockClient.Managers.Gamemode
{
    public class MoreRolesManager : AirlockClientGamemode
    {
        public static Dictionary<string, SubRoleData> SubRoleToData;
        public static Dictionary<string, System.Type> SubRoleToType;
        public static Dictionary<string, ModifierData> ModifierToData;
        public static Dictionary<string, System.Type> ModifierToType;
        public static int MaxBodiesEatenCount = 3;
        public static int BomberCooldown = 0;
        public static int TasksAssignedCount = 4;
        public static RoleManager Rolemanager;
        public static SpawnManager Spawn;
        public static NetworkedKillBehaviour Killing;

        public static void FetchRoles()
        {
            SubRoleToData = new Dictionary<string, SubRoleData>();
            SubRoleToType = new Dictionary<string, System.Type>();
            ModifierToData = new Dictionary<string, ModifierData>();
            ModifierToType = new Dictionary<string, System.Type>();

            string[] namespaceTargets =
            {
                "AirlockClient.Data.Roles.MoreRoles.Crewmate",
                "AirlockClient.Data.Roles.MoreRoles.Imposter",
                "AirlockClient.Data.Roles.MoreRoles.Neutral",
            };

            string[] modifiers =
            {
                "AirlockClient.Data.Roles.MoreRoles.Modifiers"
            };

            List<System.Type> roleTypes = Assembly.GetAssembly(typeof(Base)).GetTypes().Where(t => namespaceTargets.Contains(t.Namespace)).Where(t => t.IsClass).ToList();

            foreach (System.Type type in roleTypes)
            {
                FieldInfo field = type.GetField("Data", BindingFlags.Public | BindingFlags.Static);
                if (field == null) continue;

                SubRoleData data = (SubRoleData)field.GetValue(null);
                if (data != null)
                {
                    SubRoleToData[type.Name] = data;
                    SubRoleToType[type.FullName] = type;
                }
            }

            List<System.Type> modifierTypes = Assembly.GetAssembly(typeof(Base)).GetTypes().Where(t => modifiers.Contains(t.Namespace)).Where(t => t.IsClass).ToList();

            foreach (System.Type type in modifierTypes)
            {
                FieldInfo field = type.GetField("Data", BindingFlags.Public | BindingFlags.Static);
                if (field == null) continue;

                ModifierData data = (ModifierData)field.GetValue(null);
                if (data != null)
                {
                    ModifierToData[type.Name] = data;
                    ModifierToType[type.FullName] = type;
                }
            }
        }

        public override bool OnGameStart()
        {
            if (ModdedGameStateManager.Instance == null)
            {
                Logging.Warn("ModdedGameStateManager.Instance is null");
                return true;
            }
            return true;
        }
        public static void RPC_SendSubRole(int playerId, string role)
        {
            SendRpc("SendSubRole", playerId, role);
        }

        [AirlockRpc("SendSubRole", RpcTarget.All, RpcCaller.Host)]
        public static void SendSubRole(string role)
        {
            if (System.Type.GetType("AirlockClient.Data.Roles.MoreRoles." + role) != null)
            {
                SubRoleData roleData = (SubRoleData)System.Type.GetType("AirlockClient.Data.Roles.MoreRoles." + role).GetField("Data").GetValue(null);
                Logging.Log("ROLE: " + roleData.Name + " | " + roleData.AC_Description);
                Logging.Debug_Log("ROLE: " + roleData.Name + " | " + roleData.AC_Description);
            }
        }

        GameObject UI;
        void Start()
        {
            //UI = Instantiate(StorageManager.AirlockClient_UI);
            //SetupUI();
            Rolemanager = FindObjectOfType<RoleManager>();
            Spawn = FindObjectOfType<SpawnManager>();
            Killing = FindObjectOfType<NetworkedKillBehaviour>();
        }

        void Update()
        {
            if (UI)
            {
                if (!State.InLobbyState())
                {
                    if (UI.activeSelf)
                    {
                        UI.SetActive(false);
                    }
                    return;
                }

                if (Keyboard.current.leftAltKey.wasPressedThisFrame)
                {
                    UI.SetActive(!UI.activeSelf);
                }
            }
        }

        void SetupUI()
        {
            GameObject Template = UI.transform.Find("MoreRoles").Find("BG").Find("Roles").Find("ROLE_TEMPLATE").gameObject;

            foreach (string subrole in SubRoleToData.Keys)
            {
                GameObject RoleSetting = Instantiate(Template, Template.transform.parent);
                RoleSetting.name = "ROLE_" + subrole.ToString();

                Button decrease = RoleSetting.transform.Find("Decrease").gameObject.GetComponent<Button>();
                Button increase = RoleSetting.transform.Find("Increase").gameObject.GetComponent<Button>();
                TextMeshProUGUI roleName = RoleSetting.transform.Find("RoleName").gameObject.GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI amount = RoleSetting.transform.Find("Amount").gameObject.GetComponent<TextMeshProUGUI>();

                roleName.text = subrole.ToString();
                amount.text = "(1)";

                decrease.onClick.AddListener((UnityAction)(() =>
                {
                    ChangeRoleAmount(amount, subrole, -1);
                }));

                increase.onClick.AddListener((UnityAction)(() =>
                {
                    ChangeRoleAmount(amount, subrole, 1);
                }));
            }
            GameObject template = UI.transform.Find("MoreRoles").Find("BG").Find("Roles").Find("ROLE_TEMPLATE").gameObject;
            GameObject bodiesEatenSetting = Instantiate(template, template.transform.parent);
            bodiesEatenSetting.name = "BODIES_EATEN";

            Button decreaseBE = bodiesEatenSetting.transform.Find("Decrease").GetComponent<Button>();
            Button increaseBE = bodiesEatenSetting.transform.Find("Increase").GetComponent<Button>();
            TextMeshProUGUI roleNameBE = bodiesEatenSetting.transform.Find("RoleName").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI amountBE = bodiesEatenSetting.transform.Find("Amount").GetComponent<TextMeshProUGUI>();

            roleNameBE.text = "Bodies Eaten";
            amountBE.text = "(" + (MaxBodiesEatenCount == 0 ? 1 : MaxBodiesEatenCount).ToString() + ")";

            decreaseBE.onClick.AddListener((UnityAction)(() =>
            {
                ChangeMaxBodiesEatenAmount(amountBE, -1);
            }));
            increaseBE.onClick.AddListener((UnityAction)(() =>
            {
                ChangeMaxBodiesEatenAmount(amountBE, 1);
            }));

            GameObject bomberCooldownSetting = Instantiate(template, template.transform.parent);
            bomberCooldownSetting.name = "BOMBER_COOLDOWN";

            Button decreaseBC = bomberCooldownSetting.transform.Find("Decrease").GetComponent<Button>();
            Button increaseBC = bomberCooldownSetting.transform.Find("Increase").GetComponent<Button>();
            TextMeshProUGUI roleNameBC = bomberCooldownSetting.transform.Find("RoleName").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI amountBC = bomberCooldownSetting.transform.Find("Amount").GetComponent<TextMeshProUGUI>();

            roleNameBC.text = "Bomber Coolown";
            amountBC.text = "(" + BomberCooldown.ToString() + ")";

            decreaseBC.onClick.AddListener((UnityAction)(() =>
            {
                ChangeBomberCooldown(amountBC, -5);
            }));
            increaseBC.onClick.AddListener((UnityAction)(() =>
            {
                ChangeBomberCooldown(amountBC, 5);
            }));

            GameObject ViperDisolveTimeSetting = Instantiate(template, template.transform.parent);
            ViperDisolveTimeSetting.name = "Viper Disolve Time";
            Button decreaseVDC = ViperDisolveTimeSetting.transform.Find("Decrease").GetComponent<Button>();
            Button increaseVDC = ViperDisolveTimeSetting.transform.Find("Increase").GetComponent<Button>();
            TextMeshProUGUI rolenameVDC = ViperDisolveTimeSetting.transform.Find("RoleName").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI amountVDC = ViperDisolveTimeSetting.transform.Find("Amount").GetComponent<TextMeshProUGUI>();

            rolenameVDC.text = "Disolve Time";
            amountVDC.text = "(" + ViperDisolveTimeSetting.ToString() + ")";

            decreaseVDC.onClick.AddListener((UnityAction)(() =>
            {
                ChangeDissolveTime(amountVDC, -5);
            }));
            increaseVDC.onClick.AddListener((UnityAction)(() =>
            {
                ChangeDissolveTime(amountVDC, 5);
            }));

            GameObject WorkerTasksAssigned = Instantiate(template, template.transform.parent);
            WorkerTasksAssigned.name = "Worker Tasks Assigned";
            Button decreaseWTA = WorkerTasksAssigned.transform.Find("Decrease").GetComponent<Button>();
            Button increaseWTA = WorkerTasksAssigned.transform.Find("Increase").GetComponent<Button>();
            TextMeshProUGUI rolenameWTA = WorkerTasksAssigned.transform.Find("RoleName").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI amountWTA = WorkerTasksAssigned.transform.Find("Amount").GetComponent<TextMeshProUGUI>();

            rolenameWTA.text = "Disolve Time";
            amountWTA.text = "(" + WorkerTasksAssigned.ToString() + ")";

            decreaseWTA.onClick.AddListener((UnityAction)(() =>
            {
                ChangeTotalAssignedTasks(amountWTA, -1);
            }));
            increaseWTA.onClick.AddListener((UnityAction)(() =>
            {
                ChangeTotalAssignedTasks(amountWTA, 1);
            }));
            Template.SetActive(false);
            UI.SetActive(false);
        }

        void ChangeRoleAmount(TextMeshProUGUI subRoleAmount, string role, int changeBy)
        {
            SubRoleData data = SubRoleToData[role];
            data.Amount += changeBy;

            if (data.Amount == -1)
            {
                data.Amount = 10;
            }
            else if (data.Amount == 11)
            {
                data.Amount = 0;
            }

            subRoleAmount.text = "(" + data.Amount.ToString() + ")";
        }

        void ChangeMaxBodiesEatenAmount(TextMeshProUGUI MaxBodiesEaten, int changeby)
        {
            MaxBodiesEatenCount += changeby;
            if (MaxBodiesEatenCount < 1)
            {
                MaxBodiesEatenCount = 5;
            }
            else if (MaxBodiesEatenCount > 5)
            {
                MaxBodiesEatenCount = 1;
            }

            MaxBodiesEaten.text = "(" + MaxBodiesEatenCount.ToString() + ")";
        }
        void ChangeBomberCooldown(TextMeshProUGUI BomberTime, int changeby)
        {
            BomberCooldown += changeby;
            if (BomberCooldown < 0)
            {
                BomberCooldown = 60;
            }
            else if (BomberCooldown > 60)
            {
                BomberCooldown = 0;
            }

            BomberTime.text = "(" + BomberCooldown.ToString() + ")";
        }
        void ChangeDissolveTime(TextMeshProUGUI DisolveTime, int changeby)
        {
            Viper.SecondsUntilDisolve += changeby;
            if (Viper.SecondsUntilDisolve < 5)
            {
                Viper.SecondsUntilDisolve = 60;
            }
            if (Viper.SecondsUntilDisolve > 60)
            {
                Viper.SecondsUntilDisolve = 5;
            }

            DisolveTime.text = "(" + Viper.SecondsUntilDisolve.ToString() + ")";
        }
        void ChangeTotalAssignedTasks(TextMeshProUGUI TasksAssigned, int changeby)
        {
            TasksAssignedCount += changeby;
            if (TasksAssignedCount < 3)
            {
                TasksAssignedCount = 15;
            }
            else if (TasksAssignedCount > 15)
            {
                TasksAssignedCount = 3;
            }

            TasksAssigned.text = "(" + TasksAssignedCount.ToString() + ")";
        }
        public override bool OnPlayerVoted(ref PlayerState voter, ref PlayerState voted)
        {
            foreach (SubRole role in SubRole.All)
            {
                if (role.PlayerWithRole == voter)
                {
                    role.OnPlayerVoted(voted);
                }
            }

            return true;
        }

        public override bool OnTargetedAction(ref PlayerState killer, ref PlayerState victim, ref int action)
        {
            SubRole targetSubRole = victim.GetComponent<SubRole>();
            GameRole targetRole = GetTrueRole(victim);

            if (victim.Guarded)
            {
                AntiCheat.PlayShieldBreakWithAntiCheat(killer, victim);

                return false;
            }

            foreach (SubRole role in SubRole.All)
            {
                if (role == null) continue;
                if (role.PlayerWithRole == null) continue;

                if (role.PlayerWithRole.PlayerId == killer.PlayerId)
                {
                    if (killer.GetComponent<Sheriff>())
                    {
                        if (targetRole == GameRole.Impostor)
                        {
                            if (targetSubRole != null)
                            {
                                targetSubRole.OnPlayerDied(killer);
                            }
                            role.OnPlayerKilled(victim);
                            role.OnPlayerAction(action);
                            return true;
                        }
                        else
                        {
                            AntiCheat.KillPlayerWithAntiCheat(killer, killer);
                            return false;
                        }
                    }

                    if (killer.GetComponent<Witch>())
                    {
                        if (victim.GetComponent<Bait>() == null)
                        {
                            if (targetSubRole != null)
                            {
                                targetSubRole.OnPlayerDied(killer);
                            }
                        }
                        role.OnPlayerAction(action);

                        return false;
                    }

                    if (killer.GetComponent<Poisoner>())
                    {
                        role.OnPlayerKilled(victim);
                        role.OnPlayerAction(action);
                        return false;
                    }

                    if (targetSubRole != null)
                    {
                        targetSubRole.OnPlayerDied(killer);
                    }

                    role.OnPlayerKilled(victim);
                    role.OnPlayerAction(action);
                }
                else if (role.PlayerWithRole.PlayerId == victim.PlayerId)
                {
                    Armor armorer = victim.GetComponent<Armor>();
                    if (armorer)
                    {
                        if (armorer.HasTakenHit)
                        {
                            return true;
                        }
                        else if (!armorer.HasTakenHit && GetTrueRole(killer) != GameRole.Sheriff && GetTrueRole(killer) != GameRole.Tracker && GetTrueRole(killer) != GameRole.VIP && GetTrueRole(killer) != GameRole.GuardianAngel)
                        {
                            armorer.HasTakenHit = true;
                            return false;
                        }
                    }
                }
            }

            if (targetSubRole != null)
            {
                targetSubRole.OnPlayerDied(killer);
            }

            return true;
        }

        public List<PlayerState> Crewmates = new List<PlayerState>();
        public List<PlayerState> Imposters = new List<PlayerState>();
        public List<PlayerState> Others = new List<PlayerState>();
        public override void OnAfterAssignRoles()
        {
            foreach (var role in SubRole.All.ToList())
            {
                if (role != null)
                {
                    DestroyImmediate(role);
                }
            }

            foreach (var modifier in Modifier.All.ToList())
            {
                if (modifier != null)
                {
                    DestroyImmediate(modifier);
                }
            }

            AssignedSubRoles.Clear();
            Crewmates.Clear();
            Imposters.Clear();
            Others.Clear();
            SubRole.All.Clear();
            AssignedSubRoles.Clear();
            AssignedModifiers.Clear();
            Modifier.All.Clear();


            foreach (PlayerState player in FindObjectOfType<SpawnManager>().ActivePlayerStates)
            {
                GameRole role = GetTrueRole(player);

                if (role == GameRole.Crewmember)
                {
                    Crewmates.Add(player);
                }
                else if (role == GameRole.Impostor)
                {
                    Imposters.Add(player);
                }

                Others.Add(player);
            }

            System.Random rng1 = new System.Random();
            Crewmates = Crewmates.OrderBy(_ => rng1.Next()).ToList();
            System.Random rng2 = new System.Random();
            Imposters = Imposters.OrderBy(_ => rng2.Next()).ToList();
            System.Random rng5 = new System.Random();
            Others = Others.OrderBy(_ => rng5.Next()).ToList();

            List<System.Action> roleAssignments = new List<System.Action>();

            foreach (string role in SubRoleToData.Keys)
            {
                SubRoleData data = SubRoleToData[role];

                if (data.Amount > 0)
                {
                    foreach (string type in SubRoleToType.Keys)
                    {
                        if (type.Contains(role))
                        {
                            if (data.Team == GameTeam.Crewmember)
                            {
                                roleAssignments.Add(() => AssignRole(SubRoleToType[type], Crewmates, data));
                            }
                            else if (data.Team == GameTeam.Impostor)
                            {
                                roleAssignments.Add(() => AssignRole(SubRoleToType[type], Imposters, data));

                            }
                            else if (data.Team == GameTeam.Other)
                            {
                                List<PlayerState> allPlayers = Crewmates.Concat(Imposters).OrderBy(_ => Random.value).ToList();
                                roleAssignments.Add(() =>
                                {
                                    AssignRole(SubRoleToType[type], allPlayers, data);
                                });
                            }
                            break;
                        }
                    }
                }
            }

            System.Random rng3 = new System.Random();
            roleAssignments = roleAssignments.OrderBy(_ => rng3.Next()).ToList();

            foreach (var assignment in roleAssignments)
            {
                assignment.Invoke();
            }


            List<System.Action> modifierAssignments = new List<System.Action>();
            List<PlayerState> modCrew = new List<PlayerState>();
            List<PlayerState> modImposters = new List<PlayerState>();
            List<PlayerState> modAll = new List<PlayerState>();

            foreach (PlayerState player in FindObjectOfType<SpawnManager>().ActivePlayerStates)
            {
                GameRole role = GetTrueRole(player);
                if (role == GameRole.Crewmember) modCrew.Add(player);
                else if (role == GameRole.Impostor) modImposters.Add(player);
                modAll.Add(player);
            }

            System.Random rngMod = new System.Random();
            modCrew = modCrew.OrderBy(_ => rngMod.Next()).ToList();
            modImposters = modImposters.OrderBy(_ => rngMod.Next()).ToList();
            modAll = modAll.OrderBy(_ => rngMod.Next()).ToList();

            foreach (string modifier in ModifierToData.Keys)
            {
                ModifierData data = ModifierToData[modifier];
                if (data.Amount > 0)
                {
                    foreach (string type in ModifierToType.Keys)
                    {
                        if (type.Contains(modifier))
                        {
                            if (data.Team == GameTeam.Crewmember)
                                modifierAssignments.Add(() => AssignModifier(ModifierToType[type], modCrew, data));
                            else if (data.Team == GameTeam.Impostor)
                                modifierAssignments.Add(() => AssignModifier(ModifierToType[type], modImposters, data));
                            else if (data.Team == GameTeam.Other)
                            {
                                string capturedType = type;
                                modifierAssignments.Add(() => AssignModifier(ModifierToType[capturedType], modAll, data));
                            }
                            break;
                        }
                    }
                }
            }

            System.Random rng4 = new System.Random();
            modifierAssignments = modifierAssignments.OrderBy(_ => rng4.Next()).ToList();

            foreach (var assignment in modifierAssignments)
            {
                assignment.Invoke();
            }
        }

        public void AssignRole(System.Type role, List<PlayerState> candidates, SubRoleData data)
        {
            for (int i = 0; i < data.Amount && candidates.Count > 0; i++)
            {
                int randomChance = Random.Range(1, 101);
                if (data.Chance >= randomChance && candidates[0].gameObject.GetComponent<SubRole>() == null)
                {
                    candidates[0].gameObject.AddComponent(Il2CppType.From(role));
                    candidates.RemoveAt(0);
                }
            }
        }

        public void AssignModifier(System.Type modifier, List<PlayerState> candidates, ModifierData data)
        {
            int assigned = 0;
            List<PlayerState> remaining = new List<PlayerState>(candidates);

            while (assigned < data.Amount && remaining.Count > 0)
            {
                PlayerState candidate = remaining[0];
                remaining.RemoveAt(0);

                if (candidate == null || !candidate.IsConnected) continue;
                if (candidate.gameObject.GetComponent<Modifier>() != null)
                {
                    Logging.Debug_Log($"[AssignModifier] Skipping {candidate.NetworkName.Value} — already has modifier");
                    continue;
                }

                int randomChance = Random.Range(1, 101);
                if (data.Chance < randomChance)
                {
                    Logging.Debug_Log($"[AssignModifier] Chance failed for {candidate.NetworkName.Value} — rolled {randomChance} vs {data.Chance}");
                    continue;
                }

                Logging.Debug_Log($"[AssignModifier] Assigning {modifier.Name} to {candidate.NetworkName.Value}");
                candidate.gameObject.AddComponent(Il2CppType.From(modifier));
                assigned++;
            }

            Logging.Debug_Log($"[AssignModifier] Done — assigned {assigned}/{data.Amount} of {modifier.Name}");
        }


        void OnGUI()
        {
            if (!State.InLobbyState()) return;

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("More Roles - Amount");
            foreach (string role in SubRoleToData.Keys)
            {
                SubRoleData data = SubRoleToData[role];
                if (data.AC_Description.Contains("OTHER_ROLE")) continue;

                GUI.color = data.RoleType switch
                {
                    "Crewmate" => Color.cyan,
                    "Imposter" => Color.red,
                    "Neutral" => Color.gray,
                    _ => Color.magenta
                };

                if (GUILayout.Button($"{role}: {data.Amount}"))
                {
                    data.Amount++;
                    if (data.Amount > 10) data.Amount = 0;
                }
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("More Roles - Chance");
            foreach (string role in SubRoleToData.Keys)
            {
                SubRoleData data = SubRoleToData[role];
                if (data.AC_Description.Contains("OTHER_ROLE")) continue;

                GUI.color = data.RoleType switch
                {
                    "Crewmate" => Color.cyan,
                    "Imposter" => Color.red,
                    "Neutral" => Color.gray,
                    _ => Color.gray,
                };

                if (GUILayout.Button("Chance: " + data.Chance))
                {
                    data.Chance += 5;
                    if (data.Chance > 100) data.Chance = 5;
                }
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Modifiers - Amount");

            foreach (string modifier in ModifierToData.Keys)
            {
                ModifierData data = ModifierToData[modifier];
                if (data.AC_Description.Contains("OTHER_MODIFIER")) continue;
                GUI.color = data.ModifierType switch
                {
                    "Crewmate" => Color.cyan,
                    "Imposter" => Color.red,
                    "Neutral" => Color.gray,
                    _ => Color.magenta
                };

                if (GUILayout.Button($"{modifier}: {data.Amount}"))
                {
                    data.Amount++;
                    if (data.Amount > 10) data.Amount = 0;
                }
            }

            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Modifiers - Chance");

            foreach (string modifier in ModifierToData.Keys)
            {
                ModifierData data = ModifierToData[modifier];
                if (data.AC_Description.Contains("OTHER_MODIFIER")) continue;

                GUI.color = data.ModifierType switch
                {
                    "Crewmate" => Color.cyan,
                    "Imposter" => Color.red,
                    "Neutral" => Color.gray,
                    _ => Color.magenta
                };

                if (GUILayout.Button("Chance: " + data.Chance))
                {
                    data.Chance += 5;
                    if (data.Chance > 100) data.Chance = 5;
                }
            }

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Role Settings");

            GUILayout.BeginHorizontal();
            GUILayout.Label($"[Vulture] Bodies Eaten To Win: {MaxBodiesEatenCount}", GUILayout.Width(160));
            if (GUILayout.Button("-", GUILayout.Width(25)))
            {
                MaxBodiesEatenCount--;
                if (MaxBodiesEatenCount < 1)
                {
                    MaxBodiesEatenCount = 5;
                }
            }
            if (GUILayout.Button("+", GUILayout.Width(25)))
            {
                MaxBodiesEatenCount++;
                if (MaxBodiesEatenCount > 5)
                {
                    MaxBodiesEatenCount = 1;
                }
            }
            GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal();
            //GUILayout.Label($"[Bomber] Explode Cooldown: {BomberCooldown}s", GUILayout.Width(160));
            //if (GUILayout.Button("-", GUILayout.Width(25)))
            //{
            //BomberCooldown -= 5;
            //if (BomberCooldown < 0)
            //{
            //BomberCooldown = 60;
            //}
            //}
            //if (GUILayout.Button("+", GUILayout.Width(25)))
            //{
            //BomberCooldown += 5;
            //if (BomberCooldown > 60)
            //{
            //BomberCooldown = 0;
            //}
            //}
            //GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"[Viper] Dissolve Time: {Viper.SecondsUntilDisolve}s", GUILayout.Width(160));
            if (GUILayout.Button("-", GUILayout.Width(25)))
            {
                Viper.SecondsUntilDisolve -= 5;
                if (Viper.SecondsUntilDisolve < 5)
                {
                    Viper.SecondsUntilDisolve = 60;
                }
            }
            if (GUILayout.Button("+", GUILayout.Width(25)))
            {
                Viper.SecondsUntilDisolve += 5;
                if (Viper.SecondsUntilDisolve > 60)
                {
                    Viper.SecondsUntilDisolve = 5;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"[Worker] Tasks Assigned at once: {TasksAssignedCount}", GUILayout.Width(160));
            if (GUILayout.Button("-", GUILayout.Width(25)))
            {
                TasksAssignedCount -= 1;
                if (TasksAssignedCount < 3)
                {
                    TasksAssignedCount = 15;
                }
            }
            if (GUILayout.Button("+", GUILayout.Width(25)))
            {
                TasksAssignedCount += 1;
                if (TasksAssignedCount > 15)
                {
                    TasksAssignedCount = 3;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }


        public override bool OnMeetingCalled(ref PlayerState reportingPlayer)
        {
            foreach (SubRole role in SubRole.All)
            {
                role.OnVotingBegan(null, reportingPlayer);

                if (role.PlayerWithRole.PlayerId == reportingPlayer.PlayerId)
                {
                    role.OnPlayerCalledMeeting();
                }
            }

            return true;
        }

        public override bool OnBodyReported(ref PlayerState bodyReported, ref PlayerState reportingPlayer)
        {
            foreach (SubRole role in SubRole.All)
            {
                role.OnVotingBegan(bodyReported, reportingPlayer);

                if (role.PlayerWithRole.PlayerId == reportingPlayer.PlayerId)
                {
                    role.OnPlayerReportedBody(bodyReported);
                }
            }

            return true;
        }

        public override bool OnPlayerVotedSkip(ref PlayerState voter)
        {
            foreach (SubRole role in SubRole.All)
            {
                if (role.PlayerWithRole == voter)
                {
                    role.OnPlayerVotedSkip();
                }
            }

            return true;
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


        public static GameRole GetTrueRoleMR(PlayerState player)
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

        private static Dictionary<int, (SubRoleData subRole, string additional, ModifierData modifier, GameRole roleToChange, bool displayRoleInstant)> _pendingDisplay = new();

        public static void QueueRoleDisplay(PlayerState player, SubRole role, SubRoleData data, string additional = "", GameRole roleToChange = GameRole.NotSet, bool displayRoleInstant = false)
        {
            Logging.Debug_Log($"[QueueRoleDisplay] Player: {player.NetworkName.Value}, Role: {data.Name}, Additional: {additional}");

            if (!_pendingDisplay.ContainsKey(player.PlayerId))
                _pendingDisplay[player.PlayerId] = (data, additional, null, roleToChange, displayRoleInstant);
            else
            {
                var existing = _pendingDisplay[player.PlayerId];
                _pendingDisplay[player.PlayerId] = (data, additional, existing.modifier, roleToChange, displayRoleInstant);
                Logging.Log($"[QueueRoleDisplay] Merged with existing modifier: {existing.modifier?.Name}");
            }

            MelonCoroutines.Start(WaitAndDisplay(player, role));
        }

        public static void QueueModifierDisplay(PlayerState player, ModifierData data)
        {
            if (data.AC_Description != null && data.AC_Description.Contains("OTHER_MODIFIER"))
            {
                Logging.Debug_Log($"[QueueModifierDisplay] Skipping OTHER_MODIFIER for {player.NetworkName.Value}");
                return;
            }

            Logging.Log($"[QueueModifierDisplay] Player: {player.NetworkName.Value}, Modifier: {data.Name}");

            if (!_pendingDisplay.ContainsKey(player.PlayerId))
            {
                _pendingDisplay[player.PlayerId] = (null, "", data, GameRole.NotSet, false);
                Logging.Debug_Log($"[QueueModifierDisplay] No existing role for {player.NetworkName.Value}, storing modifier only");
            }
            else
            {
                var existing = _pendingDisplay[player.PlayerId];
                _pendingDisplay[player.PlayerId] = (existing.subRole, existing.additional, data, existing.roleToChange, existing.displayRoleInstant);
                Logging.Debug_Log($"[QueueModifierDisplay] Merged modifier {data.Name} with existing role {existing.subRole?.Name}");
            }
        }

        private static System.Collections.IEnumerator WaitAndDisplay(PlayerState player, SubRole role)
        {
            Logging.Debug_Log($"[WaitAndDisplay] Waiting for player: {player.NetworkName.Value}");

            yield return new WaitForSeconds(0.5f);

            if (!_pendingDisplay.TryGetValue(player.PlayerId, out var pending))
            {
                Logging.Debug_Log($"[WaitAndDisplay] No pending display found for {player.NetworkName.Value}");
                yield break;
            }

            _pendingDisplay.Remove(player.PlayerId);

            Logging.Debug_Log($"[WaitAndDisplay] Displaying — Role: {pending.subRole?.Name}, Modifier: {pending.modifier?.Name}, Additional: {pending.additional}");

            SubRoleData subRoleData = pending.subRole;
            ModifierData modData = pending.modifier;
            string additional = pending.additional;

            if (subRoleData == null || player == null || CurrentMode.Name == "Sandbox")
            {
                Logging.Debug_Log($"[WaitAndDisplay] Bailing — subRoleData null: {subRoleData == null}, player null: {player == null}");
                yield break;
            }

            if (pending.roleToChange != GameRole.NotSet && pending.displayRoleInstant)
                Current.Role.AlterPlayerRole(pending.roleToChange, player.PlayerId);

            role.IsDisplayingRole = true;
            RPC_SendSubRole(player.PlayerId, subRoleData.Name);
            string ogName = player.NetworkName.Value;

            yield return new WaitForSeconds(1);

            string displayName = !string.IsNullOrEmpty(additional)
                ? subRoleData.Name[0] + ":" + additional
                : subRoleData.Name;

            if (modData != null)
                displayName += "+" + modData.Name;

            Logging.Debug_Log($"[WaitAndDisplay] Setting name to: {displayName}");
            player.NetworkName = displayName;

            yield return new WaitForSeconds(3);

            if (pending.roleToChange != GameRole.NotSet && !pending.displayRoleInstant)
                Current.Role.AlterPlayerRole(pending.roleToChange, player.PlayerId);

            yield return new WaitForSeconds(5);

            player.NetworkName = ogName;
            role.IsDisplayingRole = false;

            yield return new WaitForSeconds(3);

            if (pending.roleToChange != GameRole.NotSet)
                player.LocomotionPlayer.TaskPlayer._minigameManager.AssignTasks(player.LocomotionPlayer.TaskPlayer);
        }
        public static System.Collections.IEnumerator DisplayRoleInfo(PlayerState Player, SubRole Role, SubRoleData Data, string additional = "", GameRole roleToChange = GameRole.NotSet, bool displayRoleInstant = false)
        {
            QueueRoleDisplay(Player, Role, Data, additional, roleToChange, displayRoleInstant);
            yield break;
        }

        public static string BuildRoleDisplayName(PlayerState player, SubRoleData data, string additional = "", string modifierName = null)
        {
            string roleName = !string.IsNullOrEmpty(additional)
                ? data.Name[0] + ":" + additional
                : data.Name;

            if (!string.IsNullOrEmpty(modifierName))
                roleName += "+" + modifierName;

            return roleName;
        }
    }
}

