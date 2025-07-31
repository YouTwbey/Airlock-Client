using AirlockClient.Managers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using static AirlockClient.Data.Info;

namespace AirlockClient.Handlers
{
    public class DangerMeterHandler : MonoBehaviour
    {
        public static DangerMeterHandler Instance;
        public Transform Dangerous;
        static Vector3 mainPos = new Vector3(0.1844f, 0.4168f, 2.1919f);
        public SpriteRenderer MeterRenderer;
        int currentLevel;
        public bool HasDied;

        public AudioSource DangerLevel0;
        public AudioSource DangerLevel1;
        public AudioSource DangerLevel2;
        public AudioSource DangerLevel3;
        public AudioSource DangerLevel4;
        public AudioSource DangerLevel5;
        public List<AudioSource> AllDangerLevels;

        float[] DistanceLevels = new float[] { 10f, 13f, 16f, 19f, 22f }; // Closest = Level 5

        public static void DeInit()
        {
            if (Instance == null) return;
            Destroy(Instance.gameObject);
        }

        public static void Init(Transform dangerous)
        {
            if (Instance) return;

            if (IsVR)
            {
                GameObject UI = GameObject.Find("ModUICanvas");
                if (UI != null)
                {
                    mainPos = new Vector3(0.1844f, 33, 2.1919f);
                    GameObject DangerMeter = new GameObject("UI_DangerMeter");
                    DangerMeter.layer = LayerMask.NameToLayer("UI");
                    SpriteRenderer rend = DangerMeter.AddComponent<SpriteRenderer>();
                    rend.sprite = StorageManager.DangerMeter0;
                    DangerMeter.transform.parent = UI.transform;
                    DangerMeter.transform.localPosition = mainPos;
                    DangerMeter.transform.localScale = new Vector3(7.5f, 7.5f, 7.5f);

                    DangerMeterHandler manager = DangerMeter.AddComponent<DangerMeterHandler>();
                    Instance = manager;
                    manager.MeterRenderer = rend;
                    manager.Dangerous = dangerous;
                    manager.DangerLevel0 = DangerMeter.AddComponent<AudioSource>();
                    manager.DangerLevel1 = DangerMeter.AddComponent<AudioSource>();
                    manager.DangerLevel2 = DangerMeter.AddComponent<AudioSource>();
                    manager.DangerLevel3 = DangerMeter.AddComponent<AudioSource>();
                    manager.DangerLevel4 = DangerMeter.AddComponent<AudioSource>();
                    manager.DangerLevel5 = DangerMeter.AddComponent<AudioSource>();

                    foreach (AudioMixerGroup group in Resources.FindObjectsOfTypeAll<AudioMixerGroup>())
                    {
                        if (group.name.Contains("Music"))
                        {
                            manager.DangerLevel0.outputAudioMixerGroup = group;
                            manager.DangerLevel1.outputAudioMixerGroup = group;
                            manager.DangerLevel2.outputAudioMixerGroup = group;
                            manager.DangerLevel3.outputAudioMixerGroup = group;
                            manager.DangerLevel4.outputAudioMixerGroup = group;
                            manager.DangerLevel5.outputAudioMixerGroup = group;
                        }
                    }

                    manager.AllDangerLevels = new List<AudioSource> { manager.DangerLevel0, manager.DangerLevel1, manager.DangerLevel2, manager.DangerLevel3, manager.DangerLevel4, manager.DangerLevel5 };

                    manager.DangerLevel0.clip = StorageManager.DangerMusic0;
                    manager.DangerLevel1.clip = StorageManager.DangerMusic1;
                    manager.DangerLevel2.clip = StorageManager.DangerMusic2;
                    manager.DangerLevel3.clip = StorageManager.DangerMusic3;
                    manager.DangerLevel4.clip = StorageManager.DangerMusic4;
                    manager.DangerLevel5.clip = StorageManager.DangerMusic5;

                    manager.DangerLevel1.volume = 0;
                    manager.DangerLevel2.volume = 0;
                    manager.DangerLevel3.volume = 0;
                    manager.DangerLevel4.volume = 0;
                    manager.DangerLevel5.volume = 0;

                    manager.DangerLevel0.loop = true;
                    manager.DangerLevel1.loop = true;
                    manager.DangerLevel2.loop = true;
                    manager.DangerLevel3.loop = true;
                    manager.DangerLevel4.loop = true;
                    manager.DangerLevel5.loop = true;

                    manager.DangerLevel0.Play();
                    manager.DangerLevel1.Play();
                    manager.DangerLevel2.Play();
                    manager.DangerLevel3.Play();
                    manager.DangerLevel4.Play();
                    manager.DangerLevel5.Play();
                }
            }
            else
            {
                GameObject UI = GameObject.Find("-------- VR MANAGEMENT --------/XRRig_Gameplay/UI/3DHUD_Canvas/3DHUD_Frame/LowerLeftParent");
                if (UI != null)
                {
                    GameObject DangerMeter = new GameObject("UI_DangerMeter");
                    DangerMeter.layer = LayerMask.NameToLayer("UI");
                    SpriteRenderer rend = DangerMeter.AddComponent<SpriteRenderer>();
                    rend.sprite = StorageManager.DangerMeter0;
                    DangerMeter.transform.parent = UI.transform;
                    DangerMeter.transform.localPosition = mainPos;
                    DangerMeter.transform.localScale = new Vector3(0.075f, 0.075f, 0.075f);

                    DangerMeterHandler manager = DangerMeter.AddComponent<DangerMeterHandler>();
                    Instance = manager;
                    manager.MeterRenderer = rend;
                    manager.Dangerous = dangerous;
                    manager.DangerLevel0 = DangerMeter.AddComponent<AudioSource>();
                    manager.DangerLevel1 = DangerMeter.AddComponent<AudioSource>();
                    manager.DangerLevel2 = DangerMeter.AddComponent<AudioSource>();
                    manager.DangerLevel3 = DangerMeter.AddComponent<AudioSource>();
                    manager.DangerLevel4 = DangerMeter.AddComponent<AudioSource>();
                    manager.DangerLevel5 = DangerMeter.AddComponent<AudioSource>();

                    foreach (AudioMixerGroup group in Resources.FindObjectsOfTypeAll<AudioMixerGroup>())
                    {
                        if (group.name.Contains("Music"))
                        {
                            manager.DangerLevel0.outputAudioMixerGroup = group;
                            manager.DangerLevel1.outputAudioMixerGroup = group;
                            manager.DangerLevel2.outputAudioMixerGroup = group;
                            manager.DangerLevel3.outputAudioMixerGroup = group;
                            manager.DangerLevel4.outputAudioMixerGroup = group;
                            manager.DangerLevel5.outputAudioMixerGroup = group;
                        }
                    }

                    manager.AllDangerLevels = new List<AudioSource> { manager.DangerLevel0, manager.DangerLevel1, manager.DangerLevel2, manager.DangerLevel3, manager.DangerLevel4, manager.DangerLevel5 };

                    manager.DangerLevel0.clip = StorageManager.DangerMusic0;
                    manager.DangerLevel1.clip = StorageManager.DangerMusic1;
                    manager.DangerLevel2.clip = StorageManager.DangerMusic2;
                    manager.DangerLevel3.clip = StorageManager.DangerMusic3;
                    manager.DangerLevel4.clip = StorageManager.DangerMusic4;
                    manager.DangerLevel5.clip = StorageManager.DangerMusic5;

                    manager.DangerLevel1.volume = 0;
                    manager.DangerLevel2.volume = 0;
                    manager.DangerLevel3.volume = 0;
                    manager.DangerLevel4.volume = 0;
                    manager.DangerLevel5.volume = 0;

                    manager.DangerLevel0.loop = true;
                    manager.DangerLevel1.loop = true;
                    manager.DangerLevel2.loop = true;
                    manager.DangerLevel3.loop = true;
                    manager.DangerLevel4.loop = true;
                    manager.DangerLevel5.loop = true;

                    manager.DangerLevel0.Play();
                    manager.DangerLevel1.Play();
                    manager.DangerLevel2.Play();
                    manager.DangerLevel3.Play();
                    manager.DangerLevel4.Play();
                    manager.DangerLevel5.Play();
                }
            }
        }

        int GetDangerLevel(float distance)
        {
            if (distance <= DistanceLevels[0]) return 5;
            if (distance <= DistanceLevels[1]) return 4;
            if (distance <= DistanceLevels[2]) return 3;
            if (distance <= DistanceLevels[3]) return 2;
            if (distance <= DistanceLevels[4]) return 1;
            return 0;
        }

        void Update()
        {
            if (Dangerous)
            {
                if (Dangerous == null || Camera.main == null)
                    return;

                float distance = Vector3.Distance(Camera.main.transform.position, Dangerous.position);
                int level = GetDangerLevel(distance);

                if (level == 5)
                {
                    Vector3 shake = new Vector3(
                        Random.Range(-0.005f, 0.005f),
                        Random.Range(-0.005f, 0.005f),
                        0f
                    );
                    transform.localPosition = mainPos + shake;
                }
                else
                {
                    transform.localPosition = mainPos;
                }

                if (level != currentLevel)
                {
                    SetDangerLevel(level);
                }
            }
        }

        void SetDangerLevel(int level)
        {
            if (HasDied)
            {
                for (int i = 0; i < AllDangerLevels.Count; i++)
                {
                    if (i == 0)
                    {
                        AllDangerLevels[i].volume = 0.5f;

                        if (!AllDangerLevels[i].isPlaying)
                            AllDangerLevels[i].Play();
                    }
                    else
                    {
                        AllDangerLevels[i].volume = 0;
                    }
                }
            }
            else
            {
                for (int i = 0; i < AllDangerLevels.Count; i++)
                {
                    if (i == level)
                    {
                        AllDangerLevels[i].volume = 1;

                        if (!AllDangerLevels[i].isPlaying)
                            AllDangerLevels[i].Play();
                    }
                    else
                    {
                        AllDangerLevels[i].volume = 0;
                    }
                }
            }

            if (MeterRenderer != null)
            {
                switch (level)
                {
                    case 0: MeterRenderer.sprite = StorageManager.DangerMeter0; break;
                    case 1: MeterRenderer.sprite = StorageManager.DangerMeter1; break;
                    case 2: MeterRenderer.sprite = StorageManager.DangerMeter2; break;
                    case 3: MeterRenderer.sprite = StorageManager.DangerMeter3; break;
                    case 4: MeterRenderer.sprite = StorageManager.DangerMeter4; break;
                    case 5: MeterRenderer.sprite = StorageManager.DangerMeter5; break;
                }
            }

            currentLevel = level;
        }
    }
}
