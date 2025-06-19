using AirlockClient.Managers.Debug;
using MelonLoader.Utils;
using System.IO;
using UnityEngine;
using static UnityEngine.Object;

namespace AirlockClient.Managers
{
    public static class StorageManager
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
            if (Bundle != null)
            {
                if (Logo == null)
                {
                    Logo = LoadSprite("airlockclient/main/logo.png");
                    ModStamp = LoadSprite("airlockclient/main/modstamp.png");
                    AirlockClient_UI = LoadGameObject("airlockclient/main/airlockclient_ui.prefab");

                    MoreRolesIcon = LoadSprite("airlockclient/icons/moreroles.png");
                    HideNSeekIcon = LoadSprite("airlockclient/icons/hidenseek.png");
                    FreeRoamIcon = LoadSprite("airlockclient/icons/freeroam.png");
                    LightsOutIcon = LoadSprite("airlockclient/icons/lightsout.png");
                    InfectedIcon = LoadSprite("airlockclient/icons/infected.png");
                    ContainmentIcon = LoadSprite("airlockclient/icons/containment.png");
                    SheriffIcon = LoadSprite("airlockclient/icons/sheriff.png");

                    DangerMeter0 = LoadSprite("airlockclient/gamemodes/hidenseek/dangermeter/0.png");
                    DangerMeter1 = LoadSprite("airlockclient/gamemodes/hidenseek/dangermeter/1.png");
                    DangerMeter2 = LoadSprite("airlockclient/gamemodes/hidenseek/dangermeter/2.png");
                    DangerMeter3 = LoadSprite("airlockclient/gamemodes/hidenseek/dangermeter/3.png");
                    DangerMeter4 = LoadSprite("airlockclient/gamemodes/hidenseek/dangermeter/4.png");
                    DangerMeter5 = LoadSprite("airlockclient/gamemodes/hidenseek/dangermeter/5.png");
                    DangerMusic0 = LoadAudio("airlockclient/gamemodes/hidenseek/dangermeter/0.wav");
                    DangerMusic1 = LoadAudio("airlockclient/gamemodes/hidenseek/dangermeter/1.wav");
                    DangerMusic2 = LoadAudio("airlockclient/gamemodes/hidenseek/dangermeter/2.wav");
                    DangerMusic3 = LoadAudio("airlockclient/gamemodes/hidenseek/dangermeter/3.wav");
                    DangerMusic4 = LoadAudio("airlockclient/gamemodes/hidenseek/dangermeter/4.wav");
                    DangerMusic5 = LoadAudio("airlockclient/gamemodes/hidenseek/dangermeter/5.wav");
                    SeekerMusic = LoadAudio("airlockclient/gamemodes/hidenseek/seeker.mp3");
                }
                return;
            }

            string bundleLocation = MelonEnvironment.UserDataDirectory + "\\airlockclient";

            if (File.Exists(bundleLocation))
            {
                Bundle = AssetBundle.LoadFromFile(bundleLocation);

                if (Bundle != null)
                {
                    Logo = LoadSprite("airlockclient/main/logo.png");
                    ModStamp = LoadSprite("airlockclient/main/modstamp.png");
                    AirlockClient_UI = LoadGameObject("airlockclient/main/airlockclient_ui.prefab");

                    MoreRolesIcon = LoadSprite("airlockclient/icons/moreroles.png");
                    HideNSeekIcon = LoadSprite("airlockclient/icons/hidenseek.png");
                    FreeRoamIcon = LoadSprite("airlockclient/icons/freeroam.png");
                    LightsOutIcon = LoadSprite("airlockclient/icons/lightsout.png");
                    InfectedIcon = LoadSprite("airlockclient/icons/infected.png");
                    ContainmentIcon = LoadSprite("airlockclient/icons/containment.png");
                    SheriffIcon = LoadSprite("airlockclient/icons/sheriff.png");

                    DangerMeter0 = LoadSprite("airlockclient/gamemodes/hidenseek/dangermeter/0.png");
                    DangerMeter1 = LoadSprite("airlockclient/gamemodes/hidenseek/dangermeter/1.png");
                    DangerMeter2 = LoadSprite("airlockclient/gamemodes/hidenseek/dangermeter/2.png");
                    DangerMeter3 = LoadSprite("airlockclient/gamemodes/hidenseek/dangermeter/3.png");
                    DangerMeter4 = LoadSprite("airlockclient/gamemodes/hidenseek/dangermeter/4.png");
                    DangerMeter5 = LoadSprite("airlockclient/gamemodes/hidenseek/dangermeter/5.png");
                    DangerMusic0 = LoadAudio("airlockclient/gamemodes/hidenseek/dangermeter/0.wav");
                    DangerMusic1 = LoadAudio("airlockclient/gamemodes/hidenseek/dangermeter/1.wav");
                    DangerMusic2 = LoadAudio("airlockclient/gamemodes/hidenseek/dangermeter/2.wav");
                    DangerMusic3 = LoadAudio("airlockclient/gamemodes/hidenseek/dangermeter/3.wav");
                    DangerMusic4 = LoadAudio("airlockclient/gamemodes/hidenseek/dangermeter/4.wav");
                    DangerMusic5 = LoadAudio("airlockclient/gamemodes/hidenseek/dangermeter/5.wav");
                    SeekerMusic = LoadAudio("airlockclient/gamemodes/hidenseek/seeker.mp3");
                }
                else
                {
                    Logging.Error("Airlock Client Assets failed to load!", true);
                }
            }
            else
            {
                Logging.Error("Airlock Client Assets is missing from this location: " + bundleLocation, true);
            }
        }

        static Sprite LoadSprite(string name)
        {
            return Load<Sprite>(name);
        }

        static GameObject LoadGameObject(string name)
        {
            return Load<GameObject>(name);
        }

        static AudioClip LoadAudio(string name)
        {
            return Load<AudioClip>(name);
        }

        static T Load<T>(string name) where T : Object
        {
            return Bundle.LoadAsset<T>("assets/" + name);
        }
    }
}
