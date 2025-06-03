using AirlockClient.Managers.Debug;
using MelonLoader.Utils;
using System;
using System.IO;
using UnityEngine;

namespace AirlockClient.Managers
{
    public class StorageManager : MonoBehaviour
    {
        // MainData
        public static Sprite Logo;
        public static Sprite ModStamp;

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
            string modStamp = "MainData\\ModStamp.png";
            string logo = "MainData\\Logo.png";
            string moreRolesIcon = "Icons\\MoreRoles.png";
            string hideNSeekIcon = "Icons\\HideNSeek.png";
            string freeRoamIcon = "Icons\\FreeRoam.png";
            string lightsOutIcon = "Icons\\LightsOut.png";
            string infectedIcon = "Icons\\Infected.png";
            string containmentIcon = "Icons\\Containment.png";
            string sheriffIcon = "Icons\\Sheriff.png";
            string dangerMeter0 = "Gamemodes\\HideNSeek\\DangerMeter\\0.png";
            string dangerMeter1 = "Gamemodes\\HideNSeek\\DangerMeter\\1.png";
            string dangerMeter2 = "Gamemodes\\HideNSeek\\DangerMeter\\2.png";
            string dangerMeter3 = "Gamemodes\\HideNSeek\\DangerMeter\\3.png";
            string dangerMeter4 = "Gamemodes\\HideNSeek\\DangerMeter\\4.png";
            string dangerMeter5 = "Gamemodes\\HideNSeek\\DangerMeter\\5.png";
            string dangerMusic0 = "Gamemodes\\HideNSeek\\DangerMeter\\0.wav";
            string dangerMusic1 = "Gamemodes\\HideNSeek\\DangerMeter\\1.wav";
            string dangerMusic2 = "Gamemodes\\HideNSeek\\DangerMeter\\2.wav";
            string dangerMusic3 = "Gamemodes\\HideNSeek\\DangerMeter\\3.wav";
            string dangerMusic4 = "Gamemodes\\HideNSeek\\DangerMeter\\4.wav";
            string dangerMusic5 = "Gamemodes\\HideNSeek\\DangerMeter\\5.wav";
            string seekerMusic = "Gamemodes\\HideNSeek\\Seeker.wav";

            if (ModStamp == null) ModStamp = LoadSprite(modStamp, "ModStamp");
            if (Logo == null) Logo = LoadSprite(logo, "Logo");
            if (MoreRolesIcon == null) MoreRolesIcon = LoadSprite(moreRolesIcon, "MoreRolesIcon");
            if (HideNSeekIcon == null) HideNSeekIcon = LoadSprite(hideNSeekIcon, "HideNSeekIcon");
            if (LightsOutIcon == null) LightsOutIcon = LoadSprite(lightsOutIcon, "LightsOutIcon");
            if (InfectedIcon == null) InfectedIcon = LoadSprite(infectedIcon, "InfectedIcon");
            if (ContainmentIcon == null) ContainmentIcon = LoadSprite(containmentIcon, "ContainmentIcon");
            if (FreeRoamIcon == null) FreeRoamIcon = LoadSprite(freeRoamIcon, "FreeRoamIcon");
            if (SheriffIcon == null) SheriffIcon = LoadSprite(sheriffIcon, "SheriffIcon");
            if (DangerMeter0 == null) DangerMeter0 = LoadSprite(dangerMeter0, "DangerMeter0");
            if (DangerMeter1 == null) DangerMeter1 = LoadSprite(dangerMeter1, "DangerMeter1");
            if (DangerMeter2 == null) DangerMeter2 = LoadSprite(dangerMeter2, "DangerMeter2");
            if (DangerMeter3 == null) DangerMeter3 = LoadSprite(dangerMeter3, "DangerMeter3");
            if (DangerMeter4 == null) DangerMeter4 = LoadSprite(dangerMeter4, "DangerMeter4");
            if (DangerMeter5 == null) DangerMeter5 = LoadSprite(dangerMeter5, "DangerMeter5");

            if (DangerMusic0 == null) DangerMusic0 = LoadAudio(dangerMusic0, "DangerMusic0");
            if (DangerMusic1 == null) DangerMusic1 = LoadAudio(dangerMusic1, "DangerMusic1");
            if (DangerMusic2 == null) DangerMusic2 = LoadAudio(dangerMusic2, "DangerMusic2");
            if (DangerMusic3 == null) DangerMusic3 = LoadAudio(dangerMusic3, "DangerMusic3");
            if (DangerMusic4 == null) DangerMusic4 = LoadAudio(dangerMusic4, "DangerMusic4");
            if (DangerMusic5 == null) DangerMusic5 = LoadAudio(dangerMusic5, "DangerMusic5");
            if (SeekerMusic == null) SeekerMusic = LoadAudio(seekerMusic, "SeekerMusic");
        }

        static Sprite LoadSprite(string location, string name)
        {
            try
            {
                string userdata = MelonEnvironment.UserDataDirectory + "\\AirlockClient\\";
                location = userdata + location;

                Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!texture.LoadImage(File.ReadAllBytes(location)))
                {
                    Logging.Error("Failed to load " + location);
                    return null;
                }
                else
                {
                    Sprite sprite = Sprite.Create(
                        texture,
                        new Rect(0, 0, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f));
                    sprite.name = name;
                    return sprite;
                }
            }
            catch
            {
                return ModStamp;
            }
        }

        static AudioClip LoadAudio(string location, string name)
        {
            string userdata = MelonEnvironment.UserDataDirectory + "\\AirlockClient\\";
            location = userdata + location;

            byte[] fileBytes = File.ReadAllBytes(location);
            AudioClip clip = ToAudioClip(fileBytes, Path.GetFileNameWithoutExtension(location));
            clip.name = name;
            return clip;
        }

        static AudioClip ToAudioClip(byte[] wavFile, string clipName = "wav")
        {
            int channels = BitConverter.ToInt16(wavFile, 22);
            int sampleRate = BitConverter.ToInt32(wavFile, 24);
            int byteRate = BitConverter.ToInt32(wavFile, 28);
            int dataStartIndex = FindDataChunk(wavFile);
            int samples = (wavFile.Length - dataStartIndex) / 2;

            float[] audioData = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                short sample = BitConverter.ToInt16(wavFile, dataStartIndex + i * 2);
                audioData[i] = sample / 32768f;
            }

            AudioClip audioClip = AudioClip.Create(clipName, samples / channels, channels, sampleRate, false);
            audioClip.SetData(audioData, 0);
            return audioClip;
        }

        static int FindDataChunk(byte[] file)
        {
            for (int i = 12; i < file.Length - 4; i++)
            {
                if (file[i] == 'd' && file[i + 1] == 'a' && file[i + 2] == 't' && file[i + 3] == 'a')
                    return i + 8;
            }
            throw new Exception("WAV file missing data chunk.");
        }
    }
}
