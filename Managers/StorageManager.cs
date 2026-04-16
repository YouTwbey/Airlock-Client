using AirlockClient.Managers.Debug;
using MelonLoader;
using MelonLoader.Utils;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
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
        public static Sprite DeathMatchIcon;
        public static Sprite CrownRunnersIcon;

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
            if (Bundle == null)
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                using (Stream stream = assembly.GetManifestResourceStream("AirlockClient.Data.airlockclientdata"))
                {
                    if (stream == null)
                    {
                        Logging.Error("AssetBundle resource not found: AirlockClient.Data.airlockclientdata");
                        return;
                    }

                    byte[] bundleBytes = new byte[stream.Length];
                    stream.Read(bundleBytes, 0, bundleBytes.Length);
                    Bundle = AssetBundle.LoadFromMemory(bundleBytes);

                    if (Bundle == null)
                    {
                        Logging.Error("Failed to load AssetBundle from embedded resource.");
                    }
                }
            }

            if (Bundle != null && Logo == null)
            {
                Logo = LoadSprite("AirlockClient.Data.Sprite.Logo.png");
                ModStamp = LoadModStamp("AirlockClient.Data.Sprite.ModStamp.png");
                AirlockClient_UI = LoadGameObject("airlockclient/main/airlockclient_ui.prefab");

                MoreRolesIcon = LoadSprite("AirlockClient.Data.Sprite.MoreRolesIcon.png");
                HideNSeekIcon = LoadSprite("AirlockClient.Data.Sprite.HideNSeekIcon.png");
                FreeRoamIcon = LoadSprite("AirlockClient.Data.Sprite.FreeRoamIcon.png");
                LightsOutIcon = LoadSprite("AirlockClient.Data.Sprite.LightsOutIcon.png");
                InfectedIcon = LoadSprite("AirlockClient.Data.Sprite.InfectedIcon.png");
                //ContainmentIcon = LoadSprite("AirlockClient.Data.Sprite.containment.png");
                //SheriffIcon = LoadSprite("AirlockClient.Data.Sprite.sheriff.png");
                DeathMatchIcon = LoadSprite("AirlockClient.Data.Sprite.DeathMatchIcon.png");
                CrownRunnersIcon = LoadSprite("AirlockClient.Data.Sprite.CrownRunnersTemp.png");

                DangerMeter0 = LoadSprite("AirlockClient.Data.Sprite.0.png");
                DangerMeter1 = LoadSprite("AirlockClient.Data.Sprite.1.png");
                DangerMeter2 = LoadSprite("AirlockClient.Data.Sprite.2.png");
                DangerMeter3 = LoadSprite("AirlockClient.Data.Sprite.3.png");
                DangerMeter4 = LoadSprite("AirlockClient.Data.Sprite.4.png");
                DangerMeter5 = LoadSprite("AirlockClient.Data.Sprite.5.png");
                DangerMusic0 = LoadAudio("AirlockClient.Data.AudioClip.0.wav");
                DangerMusic1 = LoadAudio("AirlockClient.Data.AudioClip.1.wav");
                DangerMusic2 = LoadAudio("AirlockClient.Data.AudioClip.2.wav");
                DangerMusic3 = LoadAudio("AirlockClient.Data.AudioClip.3.wav");
                DangerMusic4 = LoadAudio("AirlockClient.Data.AudioClip.4.wav");
                DangerMusic5 = LoadAudio("AirlockClient.Data.AudioClip.5.wav");
                SeekerMusic = LoadAudio("AirlockClient.Data.AudioClip.Seeker.wav");
            }
        }

        public static Sprite LoadSprite(string resourcePath, float targetWidth = 80f)
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath);
            if (stream == null) return null;

            MemoryStream ms = new MemoryStream();
            stream.CopyTo(ms);
            byte[] bytes = ms.ToArray();

            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            ImageConversion.LoadImage(tex, bytes);

            float ppu = tex.width / targetWidth;

            return Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                ppu,
                0,
                SpriteMeshType.FullRect,
                Vector4.zero,
                false
            );
        }

        public static Sprite LoadModStamp(string resourcePath)
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath);
            if (stream == null) return null;
            MemoryStream ms = new MemoryStream();
            stream.CopyTo(ms);
            byte[] bytes = ms.ToArray();
            Texture2D tex = new Texture2D(1, 1);
            ImageConversion.LoadImage(tex, bytes);
            return Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                100, 0,
                SpriteMeshType.FullRect,
                new Vector4(0, 0, 0, 0),
                false, null
            );
        }

        static GameObject LoadGameObject(string name)
        {
            return Load<GameObject>(name);
        }

        static AudioClip LoadAudio(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();

            var stream = assembly.GetManifestResourceStream(name);

            byte[] data = new byte[stream.Length];
            stream.Read(data, 0, data.Length);

            using var ms = new MemoryStream(data);
            using var reader = new BinaryReader(ms);

            reader.ReadBytes(4); 
            reader.ReadInt32();
            reader.ReadBytes(4);

            reader.ReadBytes(4);
            int fmtSize = reader.ReadInt32();
            reader.ReadInt16();
            int channels = reader.ReadInt16();
            int sampleRate = reader.ReadInt32();
            reader.ReadInt32();
            reader.ReadInt16();
            int bitDepth = reader.ReadInt16();
            if (fmtSize > 16) reader.ReadBytes(fmtSize - 16);

            string chunkId = "";
            int chunkSize = 0;
            while (chunkId != "data")
            {
                chunkId = new string(reader.ReadChars(4));
                chunkSize = reader.ReadInt32();
                if (chunkId != "data") reader.ReadBytes(chunkSize);
            }

            byte[] rawSamples = reader.ReadBytes(chunkSize);
            int bytesPerSample = bitDepth / 8;
            int sampleCount = rawSamples.Length / bytesPerSample;
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                samples[i] = bitDepth switch
                {
                    8 => (rawSamples[i] - 128) / 128f,
                    16 => BitConverter.ToInt16(rawSamples, i * 2) / 32768f,
                    24 => (rawSamples[i * 3] | (rawSamples[i * 3 + 1] << 8) | ((sbyte)rawSamples[i * 3 + 2] << 16)) / 8388608f,
                    32 => BitConverter.ToSingle(rawSamples, i * 4),
                    _ => throw new NotSupportedException($"Unsupported bit depth: {bitDepth}")
                };
            }

            AudioClip clip = AudioClip.Create(name, sampleCount / channels, channels, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        static T Load<T>(string name) where T : UnityEngine.Object
        {
            return Bundle.LoadAsset<T>("assets/" + name);
        }
    }
}
