using AirlockAPI.Data;
using AirlockAPI.Managers;
using AirlockClient.AC;
using AirlockClient.Handlers;
using AirlockClient.Managers;
using AirlockClient.Managers.Dev;
using AirlockClient.Managers.Gamemode;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Cutscenes;
using Il2CppSG.Airlock.Localization;
using Il2CppSG.Airlock.UI;
using Il2CppSG.Airlock.UI.TitleScreen;
using Il2CppSG.LightUI;
using Il2CppTMPro;
using MelonLoader;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
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

        static List<HostPrivateMenu> hostingMenu = new List<HostPrivateMenu>();
        static List<GamemodeSelectionMenu> selectModeMenus = new List<GamemodeSelectionMenu>();
        static List<TitleMenu> titleMenus = new List<TitleMenu>();
        static List<MenuManager> menus = new List<MenuManager>();
        static GameObject makePublic;

        public static GameObject Orbiter1;
        public static GameObject Orbiter7;
        public static GameObject Orbiter2;
        public static GameObject Orbiter3;
        public static GameObject Orbiter4;
        public static GameObject Orbiter5;
        public static GameObject Orbiter8;
        public static GameObject Orbiter9;
        public static GameObject Orbiter10;
        public static GameObject Orbiter11;

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

            MoreRolesManager.FetchRoles();

            IsVR = Application.productName.Contains("VR");
        }

        string previousSceneName;
        public static bool titleScreenFormat;
        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            StorageManager.LoadAllAssets();
            SceneName = sceneName;
            InGame = SceneName != "Boot" && SceneName != "Title";
            SceneStorage = new GameObject("AirlockClient_" + SceneName);
            bonusMapsAdded = false;
            //LobbyBrowserHandler.firstTimeInBrowseMenu = false;
            //LobbyBrowserHandler.browseAdded = false;

            if (SceneName != "Boot")
            {
                ModStamp.CreateModStamp();
            }

            SceneStorage.transform.position = new Vector3(1000, 1000, 1000);

            if (SceneName == "Title")
            {
                CurrentMode.IsHosting = false;

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
                    polusScene._ejection.gameObject.SetActive(true);
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
                    HandleTitleOrbitor();
                }
            }

            if (InGame && previousSceneName == "Title")
            {
                if (CurrentMode.Modded)
                {
                    if (CurrentMode.IsHosting)
                    {
                        SceneStorage.AddComponent<PetManager>();
                        SceneStorage.AddComponent<ModdedGameStateManager>();
                        SceneStorage.AddComponent<CommandManager>();

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
                        //SceneStorage.AddComponent<AntiCheat>();
                    }
                }
            }

            previousSceneName = sceneName;
        }

        public override void OnGUI()
        {
            //LobbyBrowserHandler.OnGUI();
        }

        public override void OnUpdate()
        {
            if (SceneName == "Title")
            {
                if (CurrentMode.IsHosting == false)
                {
                    CurrentMode.IsHosting = GameObject.Find("Host Private Menu");
                }

                //if (!LobbyBrowserHandler.browseAdded)
                //{
                //    GameObject playOnline = GameObject.Find("PlayOnlineMenu");
                //    if (playOnline != null)
                //    {
                //        GameObject buttons = playOnline.transform.Find("MainLayout").Find("TopBar").Find("MatchButtons").gameObject;
                //        GameObject quickMatch = buttons.transform.Find("QuickMatch").gameObject;
                //        GameObject host = buttons.transform.Find("Host").gameObject;
                //        GameObject directJoin = buttons.transform.Find("DirectCode").gameObject;
                //        GameObject browse = Instantiate(quickMatch, quickMatch.transform.position, quickMatch.transform.rotation, buttons.transform);
                //        browse.name = "Browse";

                //        browse.transform.localPosition = new Vector3(-525, 0, 0);
                //        quickMatch.transform.localPosition = new Vector3(-175, 0, 0);
                //        host.transform.localPosition = new Vector3(175, 0, 0);
                //        directJoin.transform.localPosition = new Vector3(525, 0, 0);

                //        browse.transform.Find("LUI_Button_Frame").Find("Label (TMP)").GetComponent<TextMeshPro>().text = "<color=yellow>Browse";
                //        browse.transform.Find("LUI_Button_Frame").Find("Label (TMP)").GetComponent<UserStringComponent_TMP>().enabled = false;
                //        browse.transform.Find("LUI_Button_Frame").GetComponent<LUIButton>().OnPressed.RemoveAllListeners();
                //        browse.transform.Find("LUI_Button_Frame").GetComponent<LUIButton>().OnPressed.AddListener(DelegateSupport.ConvertDelegate<UnityAction>(LobbyBrowserHandler.OnEnteredBrowseLobbies));
                //        LobbyBrowserHandler.browseAdded = true;
                //    }
                //}

                //if (LobbyBrowserHandler.waitingForBrowseChange)
                //{
                //    GameObject menu = GameObject.Find("QuickMatchHostMenu");
                //    if (menu != null)
                //    {
                //        GameObject uiTitle = menu.transform.Find("Title").Find("QuickMatch").gameObject;
                //        LobbyBrowserHandler.quickMatchHud = menu.transform.Find("MainLayout").gameObject;
                //        uiTitle.transform.Find("QuickMatchTitle").GetComponent<TextMeshPro>().text = "<color=yellow>Browse";
                //        uiTitle.transform.Find("TitleText").GetComponent<TextMeshPro>().text = "Select A Lobby From The List";
                //        LobbyBrowserHandler.quickMatchHud.SetActive(false);
                //        LobbyBrowserHandler.waitingForBrowseChange = false;
                //        LobbyBrowserHandler.inBrowseMenu = true;
                //    }
                //}

                //if (LobbyBrowserHandler.inBrowseMenu)
                //{
                //    GameObject playOnline = GameObject.Find("PlayOnlineMenu");
                //    if (playOnline != null)
                //    {
                //        LobbyBrowserHandler.quickMatchHud.SetActive(true);
                //        LobbyBrowserHandler.inBrowseMenu = false;
                //    }
                //}

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
                                    if (menu.gameObject.activeSelf)
                                    {
                                        GamemodeManager.AddMode("More Roles", "Social deduction gameplay, with tons of extra roles");
                                        GamemodeManager.AddMode("Hide N Seek", "Survive the time limit before the imposter takes everyone out", GameModes.Infection);
                                        GamemodeManager.AddMode("Sandbox", "Practice killing, doing tasks or just have fun");
                                        //GamemodeManager.AddMode("Versus", "Be the first one to complete tasks and stop others");
                                        GamemodeManager.AddMode("Crown Runners", "Keep the crown for as long as possible");
                                        GamemodeManager.AddMode("DeathMatch", "2 Teams fight to the death first to 50 wins");
                                        GamemodeManager.AddMode("Lights Out", "Imposters loose, vents accessible, complete darkness", GameModes.LightsOut);
                                        GamemodeManager.AddMode("Infection", "The zomburritos have breached mess hall", GameModes.Infection);
                                        //GamemodeManager.AddMode("Containment", "Sabotages triggering, doors locking, imposters wandering", GameModes.Containment);

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
        public static void HandleTitleOrbitor()
        {
            Orbiter1 = GameObject.Find("Orbiter (1)");
            Orbiter7 = GameObject.Find("Orbiter (7)");
            Orbiter2 = GameObject.Find("Orbiter (2)");
            Orbiter3 = GameObject.Find("Orbiter (3)");
            Orbiter4 = GameObject.Find("Orbiter (4)");
            Orbiter5 = GameObject.Find("Orbiter (5)");
            Orbiter8 = GameObject.Find("Orbiter (8)");
            Orbiter9 = GameObject.Find("Orbiter (9)");
            Orbiter10 = GameObject.Find("Orbiter (10)");
            Orbiter11 = GameObject.Find("Orbiter (11)");

            GameObject[] orbiters = new GameObject[]
            {
                Orbiter1, Orbiter7, Orbiter2, Orbiter3, Orbiter4, Orbiter5,
                Orbiter8, Orbiter9, Orbiter10, Orbiter11
            };

            float[] yPositions = new float[]
            {
                -5.32f,    // Orbiter1
                -7.3309f,  // Orbiter7
                -9.32f,    // Orbiter2
                -11.2727f, // Orbiter3
                -12.8037f, // Orbiter4
                -14.4491f, // Orbiter5
                -15.7619f, // Orbiter8
                -17.0328f, // Orbiter9
                -18.1656f, // Orbiter10
                -19.4546f  // Orbiter11
            };

            for (int i = 0; i < orbiters.Length; i++)
            {
                Destroy(orbiters[i].GetComponent<Orbiter>());
                orbiters[i].AddComponent<OrbiterManager>();
                orbiters[i].transform.rotation = Quaternion.Euler(270, 180, 0);

                OrbiterManager manager = orbiters[i].GetComponent<OrbiterManager>();
                manager.yPosition = yPositions[i];
                manager.isActive = i == 0;
            }
        }
    }
}