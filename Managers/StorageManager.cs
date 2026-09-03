using AirlockClient.Managers.Debug;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace AirlockClient.Managers
{
    public class StorageManager : MonoBehaviour
    {
        public static StorageManager Instance;

        // MainData
        public Sprite Logo;
        public Sprite ModStamp;

        // Icons
        public Sprite MoreRolesIcon;
        public Sprite HideNSeekIcon;
        public Sprite FreeRoamIcon;
        public Sprite LightsOutIcon;
        public Sprite InfectedIcon;
        public Sprite ContainmentIcon;
        public Sprite SheriffIcon;
        public Sprite DeathMatchIcon;
        public Sprite CrownRunnersIcon;

        // HideNSeek
        public Sprite DangerMeter0;
        public Sprite DangerMeter1;
        public Sprite DangerMeter2;
        public Sprite DangerMeter3;
        public Sprite DangerMeter4;
        public Sprite DangerMeter5;
        public AudioClip DangerMusic0;
        public AudioClip DangerMusic1;
        public AudioClip DangerMusic2;
        public AudioClip DangerMusic3;
        public AudioClip DangerMusic4;
        public AudioClip DangerMusic5;
        public AudioClip SeekerMusic;

        private void Start()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.add_sceneLoaded((UnityAction<Scene, LoadSceneMode>)OnSceneLoad);
        }

        public void OnSceneLoad(Scene scene, LoadSceneMode loadSceneMode)
        {
            LoadAllAssets();
        }

        public void LoadAllAssets()
        {
            if (Logo == null) Logo = LoadSprite("AirlockClient.Data.Sprite.Logo.png");
            if (ModStamp == null) ModStamp = LoadModStamp("AirlockClient.Data.Sprite.ModStamp.png");

            if (MoreRolesIcon == null) MoreRolesIcon = LoadSprite("AirlockClient.Data.Sprite.MoreRolesIcon.png");
            if (HideNSeekIcon == null) HideNSeekIcon = LoadSprite("AirlockClient.Data.Sprite.HideNSeekIcon.png");
            if (FreeRoamIcon == null) FreeRoamIcon = LoadSprite("AirlockClient.Data.Sprite.FreeRoamIcon.png");
            if (LightsOutIcon == null) LightsOutIcon = LoadSprite("AirlockClient.Data.Sprite.LightsOutIcon.png");
            if (InfectedIcon == null) InfectedIcon = LoadSprite("AirlockClient.Data.Sprite.InfectedIcon.png");
            if (DeathMatchIcon == null) DeathMatchIcon = LoadSprite("AirlockClient.Data.Sprite.DeathMatchIcon.png");
            if (CrownRunnersIcon == null) CrownRunnersIcon = LoadSprite("AirlockClient.Data.Sprite.CrownRunnersTemp.png");

            if (DangerMeter0 == null) DangerMeter0 = LoadSprite("AirlockClient.Data.Sprite.0.png");
            if (DangerMeter1 == null) DangerMeter1 = LoadSprite("AirlockClient.Data.Sprite.1.png");
            if (DangerMeter2 == null) DangerMeter2 = LoadSprite("AirlockClient.Data.Sprite.2.png");
            if (DangerMeter3 == null) DangerMeter3 = LoadSprite("AirlockClient.Data.Sprite.3.png");
            if (DangerMeter4 == null) DangerMeter4 = LoadSprite("AirlockClient.Data.Sprite.4.png");
            if (DangerMeter5 == null) DangerMeter5 = LoadSprite("AirlockClient.Data.Sprite.5.png");
            if (DangerMusic0 == null) DangerMusic0 = LoadAudio("AirlockClient.Data.AudioClip.0.wav");
            if (DangerMusic1 == null) DangerMusic1 = LoadAudio("AirlockClient.Data.AudioClip.1.wav");
            if (DangerMusic2 == null) DangerMusic2 = LoadAudio("AirlockClient.Data.AudioClip.2.wav");
            if (DangerMusic3 == null) DangerMusic3 = LoadAudio("AirlockClient.Data.AudioClip.3.wav");
            if (DangerMusic4 == null) DangerMusic4 = LoadAudio("AirlockClient.Data.AudioClip.4.wav");
            if (DangerMusic5 == null) DangerMusic5 = LoadAudio("AirlockClient.Data.AudioClip.5.wav");
            if (SeekerMusic == null) SeekerMusic = LoadAudio("AirlockClient.Data.AudioClip.Seeker.wav");
        }

        private Sprite LoadSprite(string resourcePath, float targetWidth = 80f)
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath);
            if (stream == null)
            {
                Logging.Warn($"Stream has failed to load at path: {resourcePath}");
            }

            MemoryStream ms = new MemoryStream();
            if (stream != null) stream.CopyTo(ms);
            byte[] bytes = ms.ToArray();

            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            var success = tex.LoadImage(bytes);

            Logging.Warn($"{resourcePath}: {success}");

            var ppu = tex.width / targetWidth;

            var spr = Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                ppu,
                0,
                SpriteMeshType.FullRect,
                Vector4.zero,
                false
            );

            spr.hideFlags |= HideFlags.HideAndDontSave;
            spr.name = resourcePath;
            return spr;
        }

        public Sprite LoadModStamp(string resourcePath)
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath);
            if (stream == null) return null;
            var ms = new MemoryStream();
            stream.CopyTo(ms);
            var bytes = ms.ToArray();
            var tex = new Texture2D(1, 1);
            tex.LoadImage(bytes);
            var spr = Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                100, 0,
                SpriteMeshType.FullRect,
                new Vector4(0, 0, 0, 0),
                false, null
            );
            spr.hideFlags |= HideFlags.HideAndDontSave;
            return spr;
        }

        private static AudioClip LoadAudio(string soundName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            var stream = assembly.GetManifestResourceStream(soundName);

            if (stream == null) return null;
            
            var data = new byte[stream.Length];
            // ReSharper disable once MustUseReturnValue
            stream.Read(data, 0, data.Length);

            using var ms = new MemoryStream(data);
            using var reader = new BinaryReader(ms);

            reader.ReadBytes(4); 
            reader.ReadInt32();
            reader.ReadBytes(4);

            reader.ReadBytes(4);
            var fmtSize = reader.ReadInt32();
            reader.ReadInt16();
            int channels = reader.ReadInt16();
            var sampleRate = reader.ReadInt32();
            reader.ReadInt32();
            reader.ReadInt16();
            int bitDepth = reader.ReadInt16();
            if (fmtSize > 16) reader.ReadBytes(fmtSize - 16);

            var chunkId = "";
            var chunkSize = 0;
            while (chunkId != "data")
            {
                chunkId = new string(reader.ReadChars(4));
                chunkSize = reader.ReadInt32();
                if (chunkId != "data") reader.ReadBytes(chunkSize);
            }

            var rawSamples = reader.ReadBytes(chunkSize);
            var bytesPerSample = bitDepth / 8;
            var sampleCount = rawSamples.Length / bytesPerSample;
            var samples = new float[sampleCount];

            for (var i = 0; i < sampleCount; i++)
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

            var clip = AudioClip.Create(soundName, sampleCount / channels, channels, sampleRate, false);
            clip.SetData(samples, 0);
            clip.hideFlags |= HideFlags.HideAndDontSave;
            return clip;
        }
    }
}
