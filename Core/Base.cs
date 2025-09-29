using AirlockAPI.Data;
using AirlockAPI.Managers;
using AirlockClient.AC;
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

            MoreRolesManager.FetchRoles();

            IsVR = Application.productName.Contains("VR");
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            StorageManager.LoadAllAssets();
            SceneName = sceneName;
            InGame = SceneName != "Boot" && SceneName != "Title";
            SceneStorage = new GameObject("AirlockClient_" + SceneName);
            bonusMapsAdded = false;

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
                                    if (menu.gameObject.activeSelf)
                                    {
                                        GamemodeManager.AddMode("More Roles", "Social deduction gameplay, with tons of extra roles");
                                        GamemodeManager.AddMode("Hide N Seek", "Survive the time limit before the imposter takes everyone out", GameModes.Infection);
                                        GamemodeManager.AddMode("Sandbox", "Practice killing, doing tasks or just have fun");
                                        GamemodeManager.AddMode("Lights Out", "Imposters loose, vents accessible, complete darkness", GameModes.LightsOut);
                                        GamemodeManager.AddMode("Infection", "The zomburritos have breached mess hall", GameModes.Infection);
                                        GamemodeManager.AddMode("Critical Cargo", "Protect the critical crewmates and scan anyone suspicious.");
                                        //GamemodeManager.AddMode("Versus", "Be the first one to complete tasks and stop others");
                                        //CustomModeManager.Instance.CreateMode("Containment", "Sabotages triggering, doors locking, imposters wandering", GameModes.Containment);
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