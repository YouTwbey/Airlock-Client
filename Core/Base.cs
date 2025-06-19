using AirlockClient.AC;
using AirlockClient.Data;
using AirlockClient.Managers;
using AirlockClient.Managers.Dev;
using AirlockClient.Managers.Gamemode;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Cutscenes;
using Il2CppSG.Airlock.UI;
using Il2CppSG.Airlock.UI.TitleScreen;
using MelonLoader;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static AirlockClient.Data.Info;
using static UnityEngine.Object;

namespace AirlockClient.Core
{
    public class Base : MelonMod
    {
        public static bool InGame;
        bool bonusMapsAdded = false;
        public static GameObject SceneStorage;
        public static string SceneName = "";

        // Optimizations
        static List<HostPrivateMenu> hostingMenu = new List<HostPrivateMenu>();
        static List<GamemodeSelectionMenu> selectModeMenus = new List<GamemodeSelectionMenu>();
        static List<TitleMenu> titleMenus = new List<TitleMenu>();
        static List<MenuManager> menus = new List<MenuManager>();
        static GameObject makePublic;

        public override void OnInitializeMelon()
        {
            foreach (System.Type type in System.Reflection.Assembly.GetExecutingAssembly().GetTypes())
            {
                try
                {
                    ClassInjector.RegisterTypeInIl2Cpp(type);
                }
                catch { }
            }

            IsVR = Application.productName.Contains("VR");
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            StorageManager.LoadAllAssets();
            SceneName = sceneName;
            InGame = SceneName != "Boot" && SceneName != "Title";
            SceneStorage = new GameObject("AirlockClient_" + SceneName);
            bonusMapsAdded = false;

            ModStamp.CreateModStamp();

            SceneStorage.transform.position = new Vector3(1000, 1000, 1000);
            SceneStorage.AddComponent<StorageManager>();

            if (SceneName == "Title")
            {
                CurrentMode.IsHosting = false;
                SceneStorage.AddComponent<CustomModeManager>();

                hostingMenu = FindObjectsOfType<HostPrivateMenu>(true).ToList();
                selectModeMenus = FindObjectsOfType<GamemodeSelectionMenu>(true).ToList();
                titleMenus = FindObjectsOfType<TitleMenu>(true).ToList();
                menus = FindObjectsOfType<MenuManager>(true).ToList();

                TransitionSpace polusScene = null;
                foreach (TransitionSpace space in Resources.FindObjectsOfTypeAll<TransitionSpace>())
                {
                    if (space.name.Contains("Polus"))
                    {
                        polusScene = Instantiate(space.gameObject).GetComponent<TransitionSpace>();
                    }
                }

                if (polusScene)
                {
                    polusScene._someoneWasEjected.gameObject.SetActive(true);
                    Destroy(GameObject.Find("OrbitingCrewmates"));

                    foreach (Transform obj in polusScene.GetComponentsInChildren<Transform>(true))
                    {
                        switch (obj.name)
                        {
                            case "TheaterSpace":
                                obj.gameObject.SetActive(false);
                                break;

                            case "P_EjectionScene_01":
                                obj.gameObject.SetActive(false);
                                break;

                            case "SM_EndGame_01":
                                obj.gameObject.SetActive(true);
                                break;
                        }
                    }
                }
            }

            if (InGame)
            {
                if (CurrentMode.Modded)
                {
                    if (CurrentMode.IsHosting)
                    {
                        SceneStorage.AddComponent<PetManager>();
                        SceneStorage.AddComponent<ModdedGameStateManager>();
                        SceneStorage.AddComponent<CommandManager>();

                        System.Type moddedGamemodeScript = System.Type.GetType("AirlockClient.Managers.Gamemode." + CurrentMode.Name.Replace(" ", "") + "Manager");
                        if (moddedGamemodeScript != null)
                        {
                            var il2cppType = Il2CppType.From(moddedGamemodeScript);
                            SceneStorage.AddComponent(il2cppType);
                        }

                        if (CurrentMode.Name == "Hide N Seek")
                        {
                            FindObjectOfType<UILobbyScreenHandler>().PrivateLobbyPlayerCount = 2;
                            FindObjectOfType<UILobbyScreenHandler>().PublicLobbyPlayerCount = 2;
                        }
                    }
                }
                else
                {
                    if (CurrentMode.IsHosting)
                    {
                        SceneStorage.AddComponent<AntiCheat>();
                    }
                }
            }
        }

        bool titleScreenFormat;
        public override void OnUpdate()
        {
            if (SceneName == "Title")
            {
                if (CurrentMode.IsHosting == false)
                {
                    CurrentMode.IsHosting = GameObject.Find("Host Private Menu");
                }

                if (!titleScreenFormat)
                {
                    foreach (GamemodeSelectionMenu selectModeMenu in selectModeMenus)
                    {
                        if (selectModeMenu != null)
                        {
                            if (selectModeMenu.gameObject.active)
                            {
                                if (selectModeMenu._modeLayout != null)
                                {
                                    selectModeMenu._modeLayout.ChildHeight = 30;
                                    selectModeMenu._modeLayout.SetHeight(125);
                                    titleScreenFormat = true;
                                }
                            }
                        }
                    }
                }

                if (!bonusMapsAdded)
                {
                    titleScreenFormat = false;

                    foreach (TitleMenu title in titleMenus)
                    {
                        if (title != null)
                        {
                            if (title.gameObject.active && title.transform.Find("Title"))
                            {
                                title.transform.Find("Title").GetComponent<SpriteRenderer>().sprite = StorageManager.Logo;
                                title.transform.Find("Title").transform.localScale = new Vector3(0.75f, 1, 1);

                                foreach (MenuManager menu in menus)
                                {
                                    if (CustomModeManager.Instance && menu.gameObject.activeSelf)
                                    {
                                        //FindObjectOfType<GamemodeSelectionMenu>(true)._mapInfoCollection._activeModesAndMaps.Maps.Add("Mess Hall"); devs removed mess hall
                                        CustomModeManager.QueuedCustomModes.Add("More Roles", new Dictionary<string, GameModes> { { "Social deduction gameplay, with tons of extra roles", GameModes.NotSet } });
                                        CustomModeManager.QueuedCustomModes.Add("Hide N Seek", new Dictionary<string, GameModes> { { "Survive the time limit before the imposter takes everyone out", GameModes.Infection } });
                                        CustomModeManager.QueuedCustomModes.Add("Sandbox", new Dictionary<string, GameModes> { { "Practice killing, doing tasks or just have fun", GameModes.NotSet } });
                                        CustomModeManager.QueuedCustomModes.Add("Lights Out", new Dictionary<string, GameModes> { { "Imposters loose, vents accessible, complete darkness", GameModes.LightsOut } });
                                        CustomModeManager.QueuedCustomModes.Add("Infection", new Dictionary<string, GameModes> { { "The zomburritos have returned to spread the infection", GameModes.Infection } });
                                        //CustomModeManager.Instance.CreateMode("Containment", "Sabotages triggering, doors locking, imposters wandering", GameModes.Containment);
                                        //CustomModeManager.Instance.CreateMode("Dum Justice", "The vigilante has returned to restore justice", GameModes.Vigilante);
                                        //CustomModeManager.Instance.CreateMode("Round Up", "The deputy has returned to lasso imposters", GameModes.Sheriff);

                                        bonusMapsAdded = true;
                                    }
                                }

                                if (!WelcomeMessageShown)
                                {
                                    FindObjectOfType<MenuManager>().ShowRetrySignInPopup(WelcomeMessage, "CLOSE");
                                    WelcomeMessageShown = true;
                                }

                                bonusMapsAdded = true;
                            }
                        }
                    }
                }
            }

            if (InGame)
            {
                if (CurrentMode.Modded && CurrentMode.IsHosting)
                {
                    if (makePublic)
                    {
                        if (makePublic.active)
                        {
                            makePublic.SetActive(false);
                        }
                    }
                    else
                    {
                        if (GameObject.Find("MakePublic_Button"))
                        {
                            makePublic = GameObject.Find("MakePublic_Button");
                        }
                    }
                }
            }
        }
    }
}