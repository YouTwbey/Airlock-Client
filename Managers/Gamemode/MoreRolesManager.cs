using AirlockClient.Attributes;
using AirlockClient.Data.Roles.MoreRoles.Crewmate;
using AirlockClient.Data.Roles.MoreRoles.Imposter;
using AirlockClient.Data.Roles.MoreRoles.Neutral;
using AirlockClient.Managers.Debug;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Roles;
using Il2CppTMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using AirlockAPI.Attributes;
using static AirlockAPI.Managers.NetworkManager;
using AirlockAPI.Data;

namespace AirlockClient.Managers.Gamemode
{
    public class MoreRolesManager : ModdedGamemode
    {
        public enum SubGameRole
        {
            Bait,
            GuardianAngel,
            Magician,
            Mayor,
            Sheriff,
            Silencer,
            Yapper,
            Bomber,
            Janitor,
            Vampire,
            Witch,
            Executioner,
            Jester,
            Lover,
            Armorer,
            Assassin,
            Poisoner,
            Troll
        }

        static readonly Dictionary<SubGameRole, SubRoleData> SubRoleToData = new Dictionary<SubGameRole, SubRoleData>
        {
            { SubGameRole.Bait, Bait.Data },
            { SubGameRole.GuardianAngel, GuardianAngel.Data },
            { SubGameRole.Magician, Magician.Data },
            { SubGameRole.Mayor, Mayor.Data },
            { SubGameRole.Sheriff , Sheriff.Data },
            { SubGameRole.Silencer, Silencer.Data },
            { SubGameRole.Yapper, Yapper.Data },
            { SubGameRole.Bomber, Bomber.Data },
            { SubGameRole.Janitor, Janitor.Data },
            { SubGameRole.Vampire, Vampire.Data },
            { SubGameRole.Witch, Witch.Data },
            { SubGameRole.Executioner, Executioner.Data },
            { SubGameRole.Jester, Jester.Data },
            { SubGameRole.Lover, Lover.Data },
            { SubGameRole.Armorer, Armorer.Data },
            { SubGameRole.Assassin, Assassin.Data },
            { SubGameRole.Poisoner, Poisoner.Data },
            { SubGameRole.Troll, Troll.Data },
        };

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
            }
        }

        GameObject UI;
        void Start()
        {
            UI = Instantiate(StorageManager.AirlockClient_UI);
            //SetupUI();
        }

        void Update()
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

        void SetupUI()
        {
            GameObject Template = UI.transform.Find("MoreRoles").Find("BG").Find("Roles").Find("ROLE_TEMPLATE").gameObject;

            foreach (SubGameRole subrole in System.Enum.GetValues(typeof(SubGameRole)))
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

        void ChangeRoleAmount(TextMeshProUGUI subRoleAmount, SubGameRole role, int changeBy)
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

        public List<PlayerState> Crewmates = new List<PlayerState>();
        public List<PlayerState> Imposters = new List<PlayerState>();
        public override void OnAssignRoles()
        {
            AssignedRoles.Clear();
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
                if (GetTrueRole(player) == GameRole.Imposter)
                {
                    Imposters.Add(player);
                }
            }

            System.Random rng1 = new System.Random();
            Crewmates = Crewmates.OrderBy(_ => rng1.Next()).ToList();
            System.Random rng2 = new System.Random();
            Imposters = Imposters.OrderBy(_ => rng2.Next()).ToList();

            List<System.Action> roleAssignments = new List<System.Action>();

            if (Bait.Data.Amount > 0) roleAssignments.Add(() => AssignRole<Bait>(Crewmates, Bait.Data.Amount));
            if (Magician.Data.Amount > 0) roleAssignments.Add(() => AssignRole<Magician>(Crewmates, Magician.Data.Amount));
            if (Mayor.Data.Amount > 0) roleAssignments.Add(() => AssignRole<Mayor>(Crewmates, Mayor.Data.Amount));
            if (Silencer.Data.Amount > 0) roleAssignments.Add(() => AssignRole<Silencer>(Crewmates, Silencer.Data.Amount));
            if (Yapper.Data.Amount > 0) roleAssignments.Add(() => AssignRole<Yapper>(Crewmates, Yapper.Data.Amount));
            if (GuardianAngel.Data.Amount > 0) roleAssignments.Add(() => AssignRole<GuardianAngel>(Crewmates, GuardianAngel.Data.Amount));
            if (Sheriff.Data.Amount > 0) roleAssignments.Add(() => AssignRole<Sheriff>(Crewmates, Sheriff.Data.Amount));
            if (Armorer.Data.Amount > 0) roleAssignments.Add(() => AssignRole<Armorer>(Crewmates, Armorer.Data.Amount));

            if (Bomber.Data.Amount > 0) roleAssignments.Add(() => AssignRole<Bomber>(Imposters, Bomber.Data.Amount));
            if (Janitor.Data.Amount > 0) roleAssignments.Add(() => AssignRole<Janitor>(Imposters, Janitor.Data.Amount));
            if (Vampire.Data.Amount > 0) roleAssignments.Add(() => AssignRole<Vampire>(Imposters, Vampire.Data.Amount));
            if (Witch.Data.Amount > 0) roleAssignments.Add(() => AssignRole<Witch>(Imposters, Witch.Data.Amount));
            if (Poisoner.Data.Amount > 0) roleAssignments.Add(() => AssignRole<Poisoner>(Imposters, Poisoner.Data.Amount));
            if (Assassin.Data.Amount > 0) roleAssignments.Add(() => AssignRole<Assassin>(Imposters, Assassin.Data.Amount));

            if (Executioner.Data.Amount > 0) roleAssignments.Add(() => AssignRole<Executioner>(Crewmates, Executioner.Data.Amount));
            if (Jester.Data.Amount > 0) roleAssignments.Add(() => AssignRole<Jester>(Crewmates, Jester.Data.Amount));
            if (Lover.Data.Amount > 0) roleAssignments.Add(() => AssignRole<Lover>(Crewmates, Lover.Data.Amount));
            if (Troll.Data.Amount > 0) roleAssignments.Add(() => AssignRole<Troll>(Crewmates, Troll.Data.Amount));

            System.Random rng3 = new System.Random();
            roleAssignments = roleAssignments.OrderBy(_ => rng3.Next()).ToList();

            foreach (var assignment in roleAssignments)
            {
                assignment.Invoke();
            }
        }

        public void AssignRole<T>(List<PlayerState> candidates, int amount) where T : Component
        {
            for (int i = 0; i < amount && candidates.Count > 0; i++)
            {
                candidates[0].gameObject.AddComponent<T>();
                candidates.RemoveAt(0);
            }
        }

        void OnGUI()
        {
            GUILayout.BeginVertical("More Roles");
            foreach (SubGameRole role in SubRoleToData.Keys)
            {
                SubRoleData data = SubRoleToData[role];

                if (data != null)
                {
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
        }

        public static System.Collections.IEnumerator DisplayRoleInfo(PlayerState Player, SubRole Role, SubRoleData Data, string additional = "", GameRole roleToChange = GameRole.NotSet)
        {
            if (Player != null && Role != null && Data != null && CurrentMode.Name != "Sandbox")
            {
                Role.IsDisplayingRole = true;
                RPC_SendSubRole(Player.PlayerId, Data.Name);
                string ogName = Player.NetworkName.Value;
                yield return new WaitForSeconds(1);
                Player.NetworkName = "WMWMWMWMWMWMWMWM";
                yield return new WaitForSeconds(3);
                Player.NetworkName = Data.Name;
                if (roleToChange != GameRole.NotSet)
                {
                    Current.Role.AlterPlayerRole(roleToChange, Player.PlayerId);
                    Player.LocomotionPlayer.TaskPlayer._minigameManager.AssignTasks(Player.LocomotionPlayer.TaskPlayer);
                }
                yield return new WaitForSeconds(2);
                if (string.IsNullOrEmpty(additional))
                {
                    Player.NetworkName = Data.Description;
                    yield return new WaitForSeconds(3);
                }
                else
                {
                    Player.NetworkName = Data.Description;
                    yield return new WaitForSeconds(1.5f);
                    Player.NetworkName = additional;
                    yield return new WaitForSeconds(1.5f);
                }
                Player.NetworkName = ogName;
                Role.IsDisplayingRole = false;
            }
        }
    }
}
