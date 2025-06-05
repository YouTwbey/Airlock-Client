using AirlockClient.Data;
using AirlockClient.Managers;
using Il2CppInterop.Runtime.Injection;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.UI.TitleScreen;
using Il2CppSG.Airlock.XR;
using MelonLoader;
using UnityEngine;
using static UnityEngine.Object;
using static AirlockClient.Data.Info;
using AirlockClient.Managers.Gamemode;
using AirlockClient.Managers.Dev;
using Il2CppInterop.Runtime;
using AirlockClient.AC;
using Il2CppSG.Airlock.UI;

namespace AirlockClient.Core
{
    public class Base : MelonMod
    {
        public static bool InGame;
        bool bonusMapsAdded = false;
        public static GameObject SceneStorage;
        public static string SceneName = "";

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
                if (GameObject.Find("Host Private Menu") && CurrentMode.IsHosting == false)
                {
                    CurrentMode.IsHosting = GameObject.Find("Host Private Menu");
                }

                if (!titleScreenFormat)
                {
                    if (FindObjectOfType<GamemodeSelectionMenu>())
                    {
                        FindObjectOfType<GamemodeSelectionMenu>()._modeLayout.ChildHeight = 30;
                        FindObjectOfType<GamemodeSelectionMenu>()._modeLayout.SetHeight(125);
                        titleScreenFormat = true;
                    }
                }

                if (FindObjectOfType<TitleMenu>() && !bonusMapsAdded)
                {
                    titleScreenFormat = false;
                    if (GameObject.Find("UI/MainMenuVR /Menus/TitleMenu/Title"))
                    {
                        GameObject.Find("UI/MainMenuVR /Menus/TitleMenu/Title").GetComponent<SpriteRenderer>().sprite = StorageManager.Logo;
                        GameObject.Find("UI/MainMenuVR /Menus/TitleMenu/Title").transform.localScale = new Vector3(0.75f, 1, 1);
                    }
                    else if (GameObject.Find("UI/MainMenu3D/Menus/TitleMenu/Title"))
                    {
                        GameObject.Find("UI/MainMenu3D/Menus/TitleMenu/Title").GetComponent<SpriteRenderer>().sprite = StorageManager.Logo;
                        GameObject.Find("UI/MainMenu3D/Menus/TitleMenu/Title").transform.localScale = new Vector3(0.75f, 1, 1);
                    }

                    //FindObjectOfType<GamemodeSelectionMenu>(true)._mapInfoCollection._activeModesAndMaps.Maps.Add("Mess Hall"); devs removed mess hall
                    CustomModeManager.Instance.CreateMode("More Roles", "Social deduction gameplay, with tons of extra roles");
                    CustomModeManager.Instance.CreateMode("Hide N Seek", "Survive the time limit before the imposter takes everyone out", GameModes.Infection);
                    CustomModeManager.Instance.CreateMode("Sandbox", "Practice killing, doing tasks or just have fun");
                    CustomModeManager.Instance.CreateMode("Lights Out", "Imposters loose, vents accessible, complete darkness", GameModes.LightsOut);
                    CustomModeManager.Instance.CreateMode("Infection", "The zomburritos have returned to spread the infection", GameModes.Infection);
                    //CustomModeManager.Instance.CreateMode("Containment", "Sabotages triggering, doors locking, imposters wandering", GameModes.Containment);
                    //CustomModeManager.Instance.CreateMode("Dum Justice", "The vigilante has returned to restore justice", GameModes.Vigilante);
                    //CustomModeManager.Instance.CreateMode("Round Up", "The deputy has returned to lasso imposters", GameModes.Sheriff);

                    if (!WelcomeMessageShown)
                    {
                        FindObjectOfType<MenuManager>().ShowRetrySignInPopup(WelcomeMessage, "CLOSE");
                        WelcomeMessageShown = true;
                    }

                    bonusMapsAdded = true;
                }
            }

            if (InGame)
            {
                if (CurrentMode.Modded && CurrentMode.IsHosting)
                {
                    if (GameObject.Find("MakePublic_Button"))
                    {
                        GameObject.Find("MakePublic_Button").SetActive(false);
                    }
                }
            }
        }
    }
}