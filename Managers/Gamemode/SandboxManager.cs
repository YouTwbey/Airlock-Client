using Il2CppSG.Airlock.Network;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Roles;
using AirlockClient.Data.Roles.MoreRoles.Crewmate;
using AirlockClient.Data.Roles.MoreRoles.Imposter;
using AirlockClient.Data.Roles.MoreRoles.Neutral;
using AirlockClient.Managers.Debug;
using AirlockClient.Attributes;
using Il2CppSG.Airlock.Minigames;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AirlockClient.Managers.Gamemode
{
    public class SandboxManager : ModdedGamemode
    {
        public bool playerDidSpawn;
        public MinigameManager Tasks;

        void Start()
        {
            Tasks = FindObjectOfType<MinigameManager>();
            ModdedGameStateManager.Instance.SetMatchSetting(Data.Enums.MatchBoolSettings.ReportBodies, false);
        }

        void Update()
        {
            if (Keyboard.current.leftAltKey.wasPressedThisFrame)
            {
                Enabled = !Enabled;
            }

            if (State.InLobbyState() && playerDidSpawn == true)
            {
                State.StartGame();
            }

            if (State._preventMatchEnding.Value == false)
            {
                State._preventMatchEnding.Value = true;
            }

            if (FindObjectOfType<EmergencyButton>())
            {
                State.Runner.Despawn(FindObjectOfType<EmergencyButton>().Object, true);
            }
        }

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
            Lover
        }

        public void AssignPowerUp(PlayerState player, PowerUps powerUp)
        {
            if (player)
            {
                player.ActivePowerUps = powerUp;
                return;
            }

            Logging.Warn("Player was null in AssignPowerUp(PlayerState, PowerUps) [SandboxManager].");
        }

        public void AssignRole(PlayerState player, GameRole role)
        {
            if (player)
            {
                State.RoleManager.AlterPlayerRole(role, player.PlayerId);
                player.RPC_KnownGameRole(role);
                return;
            }

            Logging.Warn("Player was null in AssignRole(PlayerState, GameRole) [SandboxManager].");
        }

        public void AssignNewTasks(PlayerState player, int amount)
        {
            if (player)
            {
                player.LocomotionPlayer.LocalMinigamePlayer.AssignedTasks.Clear();
                Tasks.AssignTaskSlots(amount, player.LocomotionPlayer.LocalMinigamePlayer);
                return;
            }

            Logging.Warn("Player was null in AssignNewTasks(PlayerState, int) [SandboxManager].");
        }

        public void AssignRole(PlayerState player, SubGameRole role)
        {
            if (player)
            {
                switch (role)
                {
                    case SubGameRole.Bait:
                        player.gameObject.AddComponent<Bait>();
                        break;

                    case SubGameRole.GuardianAngel:
                        player.gameObject.AddComponent<GuardianAngel>();
                        break;

                    case SubGameRole.Magician:
                        player.gameObject.AddComponent<Magician>();
                        break;

                    case SubGameRole.Mayor:
                        player.gameObject.AddComponent<Mayor>();
                        break;

                    case SubGameRole.Sheriff:
                        player.gameObject.AddComponent<Sheriff>();
                        break;

                    case SubGameRole.Silencer:
                        player.gameObject.AddComponent<Silencer>();
                        break;

                    case SubGameRole.Yapper:
                        player.gameObject.AddComponent<Yapper>();
                        break;

                    case SubGameRole.Bomber:
                        player.gameObject.AddComponent<Bomber>();
                        break;

                    case SubGameRole.Janitor:
                        player.gameObject.AddComponent<Janitor>();
                        break;

                    case SubGameRole.Vampire:
                        player.gameObject.AddComponent<Vampire>();
                        break;

                    case SubGameRole.Witch:
                        player.gameObject.AddComponent<Witch>();
                        break;

                    case SubGameRole.Executioner:
                        player.gameObject.AddComponent<Executioner>();
                        break;

                    case SubGameRole.Jester:
                        player.gameObject.AddComponent<Jester>();
                        break;

                    case SubGameRole.Lover:
                        player.gameObject.AddComponent<Lover>();
                        break;

                    default:
                        Logging.Warn("Unknown role for type: " + role.ToString());
                        break;
                }

                return;
            }

            Logging.Warn("Player was null in AssignRole(PlayerState, SubGameRole) [SandboxManager].");
        }

        PlayerState GetPlayerStateById(int id)
        {
            GameObject obj = GameObject.Find("PlayerState (" + id + ")");

            if (obj == null)
            {
                Logging.Warn("Id was invalid in GetPlayerStateById(int). Using host value. [SandboxManager].");
                return GetPlayerStateById(9);
            }

            PlayerState state = obj.GetComponent<PlayerState>();

            if (!state.IsConnected)
            {
                Logging.Warn("Id was invalid in GetPlayerStateById(int). Using host value. [SandboxManager].");
                return GetPlayerStateById(9);
            }

            return state;
        }

        // Generated by ChatGPT, too lazy to create the proper UI yet lol.
        private int playerIdInput = 0;
        private int taskAmount = 1;

        private SandboxManager.SubGameRole selectedSubRole;
        private GameRole selectedGameRole;
        private PowerUps selectedPowerUp;

        // Track selected enum indices
        private int selectedSubRoleIndex = 0;
        private int selectedGameRoleIndex = 0;
        private int selectedPowerUpIndex = 0;

        private int DrawEnumSelectionGrid<T>(int selectedIndex) where T : System.Enum
        {
            string[] names = System.Enum.GetNames(typeof(T));
            for (int i = 0; i < names.Length; i++)
            {
                bool isSelected = (i == selectedIndex);
                bool toggled = GUILayout.Toggle(isSelected, names[i], GUI.skin.button);
                if (toggled && !isSelected)
                {
                    selectedIndex = i;
                }
            }
            return selectedIndex;
        }

        bool Enabled;
        Vector2 scrollPosition = Vector2.zero;

        void OnGUI()
        {
            if (!State.InTaskState() || !Enabled) return;

            GUILayout.BeginArea(new Rect(10, 10, 350, 800), "Sandbox Menu", GUI.skin.window);
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(390), GUILayout.Height(680));

            // Player ID Input
            GUILayout.Label("Enter Player ID:");
            string playerIdStr = GUILayout.TextField(playerIdInput.ToString(), GUILayout.Width(100));
            int.TryParse(playerIdStr, out playerIdInput);

            GUILayout.Space(10);

            GUILayout.Label("Assign SubRole:");
            selectedSubRoleIndex = DrawEnumSelectionGrid<SandboxManager.SubGameRole>(selectedSubRoleIndex);
            selectedSubRole = (SandboxManager.SubGameRole)selectedSubRoleIndex;
            if (GUILayout.Button("Assign SubRole"))
            {
                AssignRole(GetPlayerStateById(playerIdInput), selectedSubRole);
            }
            if (GUILayout.Button("Remove All Subroles"))
            {
                foreach (SubRole role in GetPlayerStateById(playerIdInput).GetComponents<SubRole>())
                {
                    Destroy(role);
                }
            }

            GUILayout.Space(10);

            GUILayout.Label("Assign GameRole:");
            selectedGameRoleIndex = DrawEnumSelectionGrid<GameRole>(selectedGameRoleIndex);
            selectedGameRole = (GameRole)selectedGameRoleIndex;
            if (GUILayout.Button("Assign GameRole"))
            {
                AssignRole(GetPlayerStateById(playerIdInput), selectedGameRole);
            }

            GUILayout.Space(10);

            GUILayout.Label("Assign PowerUp:");
            selectedPowerUpIndex = DrawEnumSelectionGrid<PowerUps>(selectedPowerUpIndex);
            selectedPowerUp = (PowerUps)selectedPowerUpIndex;
            if (GUILayout.Button("Assign PowerUp"))
            {
                AssignPowerUp(GetPlayerStateById(playerIdInput), selectedPowerUp);
            }

            GUILayout.Space(10);

            GUILayout.Label("Assign Tasks:");
            GUILayout.BeginHorizontal();
            GUILayout.Label("Amount:", GUILayout.Width(60));
            string taskStr = GUILayout.TextField(taskAmount.ToString(), GUILayout.Width(40));
            int.TryParse(taskStr, out taskAmount);
            if (GUILayout.Button("Assign Tasks"))
            {
                AssignNewTasks(GetPlayerStateById(playerIdInput), taskAmount);
            }
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Revive Player"))
            {
                GetPlayerStateById(playerIdInput).IsAlive = true;
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
    }
}
