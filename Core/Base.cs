using AirlockClient.AC;
using AirlockClient.Managers;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using static AirlockClient.Data.Info;
using static UnityEngine.Object;

namespace AirlockClient.Core
{
    [BepInPlugin(GUID, Name, Version)]
    public class Base : BasePlugin
    {
        Harmony harmony;
        public static Base Instance;

        public override void Load()
        {
            Instance = this;
            harmony = new Harmony(GUID);
            harmony.PatchAll();
        }

        public static void OnInit()
        {
            ClassInjector.RegisterTypeInIl2Cpp(typeof(AirlockClientManager));
            ClassInjector.RegisterTypeInIl2Cpp(typeof(StorageManager));
            GameObject airlockClient = new GameObject("AirlockClient");
            GameObject storage = new GameObject("StorageManager");
            DontDestroyOnLoad(airlockClient);
            airlockClient.AddComponent<AirlockClientManager>();
            storage.AddComponent<StorageManager>();
        }
    }
}