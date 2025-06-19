using AirlockClient.Managers.Debug;
using Il2CppInterop.Runtime;
using MelonLoader.Utils;
using System.Collections;
using System.IO;
using UnityEngine;

namespace AirlockClient.Managers
{
    public class StorageManager : MonoBehaviour
    {
        // MainData
        public static AssetBundle Bundle;
        public static Sprite Logo;
        public static Sprite ModStamp;
        public static GameObject AirlockClient_UI;

        // Icons
        public static Sprite MoreRolesIcon;
        public static Sprite HideNSeekIcon;
        public static Sprite FreeRoamIcon;
        public static Sprite LightsOutIcon;
        public static Sprite InfectedIcon;
        public static Sprite ContainmentIcon;
        public static Sprite SheriffIcon;

        // HideNSeek
        public static Sprite DangerMeter0;
        public static Sprite DangerMeter1;
        public static Sprite DangerMeter2;
        public static Sprite DangerMeter3;
        public static Sprite DangerMeter4;
        public static Sprite DangerMeter5;
        public static AudioClip DangerMusic0;
        public static AudioClip DangerMusic1;
        public static AudioClip DangerMusic2;
        public static AudioClip DangerMusic3;
        public static AudioClip DangerMusic4;
        public static AudioClip DangerMusic5;
        public static AudioClip SeekerMusic;

        public static void LoadAllAssets()
        {
            string bundleLocation = MelonEnvironment.UserDataDirectory + "\\airlockclient";

            if (File.Exists(bundleLocation) && Bundle == null)
            {
                Bundle = AssetBundle.LoadFromFile(bundleLocation);

                if (Bundle != null)
                {
                    Logo = LoadSprite("Logo");
                    ModStamp = LoadSprite("ModStamp");
                    AirlockClient_UI = LoadGameObject("AirlockClient_UI");

                    MoreRolesIcon = LoadSprite("MoreRoles");
                    HideNSeekIcon = LoadSprite("HideNSeek");
                    FreeRoamIcon = LoadSprite("FreeRoam");
                    LightsOutIcon = LoadSprite("LightsOut");
                    InfectedIcon = LoadSprite("Infection");
                    ContainmentIcon = LoadSprite("Containment");
                    SheriffIcon = LoadSprite("Sheriff");

                    DangerMeter0 = LoadSprite("0");
                    DangerMeter1 = LoadSprite("1");
                    DangerMeter2 = LoadSprite("2");
                    DangerMeter3 = LoadSprite("3");
                    DangerMeter4 = LoadSprite("4");
                    DangerMeter5 = LoadSprite("5");
                    DangerMusic0 = LoadAudio("0");
                    DangerMusic1 = LoadAudio("1");
                    DangerMusic2 = LoadAudio("2");
                    DangerMusic3 = LoadAudio("3");
                    DangerMusic4 = LoadAudio("4");
                    DangerMusic5 = LoadAudio("5");
                    SeekerMusic = LoadAudio("Seeker");
                }
                else
                {
                    Logging.Error("Airlock Client Assets failed to load! Mod may not work as intended.");
                }
            }
        }

        static Sprite LoadSprite(string name)
        {
            return (Sprite)Bundle.Load(name, Il2CppType.Of<Sprite>());
        }

        static GameObject LoadGameObject(string name)
        {
            return (GameObject)Bundle.Load(name, Il2CppType.Of<GameObject>());
        }

        static AudioClip LoadAudio(string name)
        {
            return (AudioClip)Bundle.Load(name, Il2CppType.Of<AudioClip>());
        }
    }
}
