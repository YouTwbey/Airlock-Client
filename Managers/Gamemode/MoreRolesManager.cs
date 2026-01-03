using AirlockAPI.Attributes;
using AirlockAPI.Data;
using AirlockClient.AC;
using AirlockClient.Attributes;
using AirlockClient.Core;
using AirlockClient.Data.Roles.MoreRoles.Broken;
using AirlockClient.Data.Roles.MoreRoles.Crewmate;
using AirlockClient.Data.Roles.MoreRoles.Imposter;
using Il2CppInterop.Runtime;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Roles;
using Il2CppTMPro;
using System.Collections.Generic;
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
        public static SubRoleData MyRole;
        public static Dictionary<string, SubRoleData> SubRoleToData;
        public static Dictionary<string, System.Type> SubRoleToType;

        static string Color32ToHex(Color32 c) { return $"#{c.r:X2}{c.g:X2}{c.b:X2}{c.a:X2}"; }
        public static void OnRoleReveal(TextMeshPro yourRole, TextMeshPro role, TextMeshPro desc)
        {
            if (MyRole != null)
            {
                string hex = Color32ToHex(MyRole.AC_Color);
                yourRole.text = $"<color={hex}>Your role is";
                role.text = $"<color={hex}>{MyRole.Name}";
                desc.text = $"<color={hex}>{MyRole.AC_Description}";
            }
        }
        
        public override bool OnGameEnd(ref GameTeam teamThatWon)
        {
            RPC_SendSubRole(-1, "null");
            return true;
        }

        public static void FetchRoles()
        {
            SubRoleToData = new Dictionary<string, SubRoleData>();
            SubRoleToType = new Dictionary<string, System.Type>();

            string[] namespaceTargets =
            {
            "AirlockClient.Data.Roles.MoreRoles.Crewmate",
            "AirlockClient.Data.Roles.MoreRoles.Imposter",
            "AirlockClient.Data.Roles.MoreRoles.Neutral"
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
        }

        public static void RPC_SendSubRole(int playerId, string role)
        {
            SendRpc("SendSubRole", playerId, role);
        }

        [AirlockRpc("SendSubRole", RpcTarget.AllInclusive, RpcCaller.Host)]
        public static void SendSubRole(string role)
        {
            if (SubRoleToData.ContainsKey(role))
            {
                SubRoleData roleData = SubRoleToData[role];
                MyRole = roleData;
            }
            else
            {
                MyRole = null;
            }
        }

        GameObject UI;
        void Start()
        {
            //UI = Instantiate(StorageManager.AirlockClient_UI);
            //SetupUI();
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
            ProximityTargetedAction targetAction = (ProximityTargetedAction)action;
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
                                if (targetAction == ProximityTargetedAction.Kill) targetSubRole.OnPlayerDied(killer);
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
                                if (targetAction == ProximityTargetedAction.Kill) targetSubRole.OnPlayerDied(killer);
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
                        if (targetAction == ProximityTargetedAction.Kill) targetSubRole.OnPlayerDied(killer);
                    }

                    role.OnPlayerKilled(victim);
                    role.OnPlayerAction(action);
                }
                else if (role.PlayerWithRole.PlayerId == victim.PlayerId)
                {
                    Armorer armorer = victim.GetComponent<Armorer>();
                    if (armorer)
                    {
                        if (armorer.HasTakenHit)
                        {
                            return true;
                        }
                        else
                        {
                            armorer.HasTakenHit = true;
                            return false;
                        }
                    }
                }
            }

            if (targetSubRole != null)
            {
                if (targetAction == ProximityTargetedAction.Kill) targetSubRole.OnPlayerDied(killer);
            }

            return true;
        }

        public List<PlayerState> Crewmates = new List<PlayerState>();
        public List<PlayerState> Imposters = new List<PlayerState>();
        public override void OnAfterAssignRoles()
        {
            AssignedSubRoles.Clear();
            Crewmates.Clear();
            Imposters.Clear();

            foreach (SubRole role in SubRole.All)
            {
                Destroy(role);
            }

            foreach (PlayerState player in FindObjectsOfType<PlayerState>())
            {
                if (GetTrueRole(player) == GameRole.Crewmember)
                {
                    Crewmates.Add(player);
                }
                if (GetTrueRole(player) == GameRole.Impostor)
                {
                    Imposters.Add(player);
                }
            }

            System.Random rng1 = new System.Random();
            Crewmates = Crewmates.OrderBy(_ => rng1.Next()).ToList();
            System.Random rng2 = new System.Random();
            Imposters = Imposters.OrderBy(_ => rng2.Next()).ToList();

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
                                if (data.Name == "Wraith")
                                {
                                    if (Imposters.Count != 1)
                                    {
                                        roleAssignments.Add(() => AssignRole(SubRoleToType[type], Imposters, data));
                                    }
                                }
                                else
                                {
                                    roleAssignments.Add(() => AssignRole(SubRoleToType[type], Imposters, data));
                                }
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

        void OnGUI()
        {
            if (State.InLobbyState())
            {
                GUILayout.BeginHorizontal();

                GUILayout.BeginVertical("More Roles - Amount");
                foreach (string role in SubRoleToData.Keys)
                {
                    SubRoleData data = SubRoleToData[role];

                    if (data.RoleType == "Crewmate")
                    {
                        GUI.color = Color.cyan;
                    }
                    else if (data.RoleType == "Imposter")
                    {
                        GUI.color = Color.red;
                    }
                    else
                    {
                        GUI.color = Color.gray;
                    }

                    if (data != null)
                    {
                        if (data.AC_Description.Contains("OTHER_ROLE")) continue;

                        if (GUILayout.Button(role.ToString() + ": " + data.Amount))
                        {
                            data.Amount += 1;

                            if (data.Amount == 11)
                            {
                                data.Amount = 0;
                            }
                        }
                    }
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical("More Roles - Chance");
                foreach (string role in SubRoleToData.Keys)
                {
                    SubRoleData data = SubRoleToData[role];

                    if (data.RoleType == "Crewmate")
                    {
                        GUI.color = Color.cyan;
                    }
                    else if (data.RoleType == "Imposter")
                    {
                        GUI.color = Color.red;
                    }
                    else
                    {
                        GUI.color = Color.gray;
                    }

                    if (data != null)
                    {
                        if (data.AC_Description.Contains("OTHER_ROLE")) continue;

                        if (GUILayout.Button("Chance: " + data.Chance))
                        {
                            data.Chance += 5;

                            if (data.Chance == 105)
                            {
                                data.Chance = 5;
                            }
                        }
                    }
                }
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
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

        public static System.Collections.IEnumerator DisplayRoleInfo(PlayerState Player, SubRole Role, SubRoleData Data, string additional = "", GameRole roleToChange = GameRole.NotSet, bool displayRoleInstant = false)
        {
            RPC_SendSubRole(Player.PlayerId, Data.Name);

            if (Player != null && Role != null && Data != null && CurrentMode.Name != "Sandbox")
            {
                if (roleToChange != GameRole.NotSet && displayRoleInstant)
                {
                    Current.Role.AlterPlayerRole(roleToChange, Player.PlayerId);
                }
                Role.IsDisplayingRole = true;
                string ogName = Player.NetworkName.Value;

                yield return new WaitForSeconds(1);

                if (!AirlockClientUsers.Contains(Player.PlayerId)) Player.NetworkName = Data.Name;

                yield return new WaitForSeconds(3);

                if (roleToChange != GameRole.NotSet && !displayRoleInstant)
                {
                    Current.Role.AlterPlayerRole(roleToChange, Player.PlayerId);
                }

                yield return new WaitForSeconds(5);

                Player.NetworkName = ogName;
                Role.IsDisplayingRole = false;

                yield return new WaitForSeconds(3);
                if (roleToChange != GameRole.NotSet)
                {
                    Player.LocomotionPlayer.TaskPlayer._minigameManager.AssignTasks(Player.LocomotionPlayer.TaskPlayer);
                }
            }
        }
    }
}
