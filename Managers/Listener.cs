using AirlockClient.Attributes;
using AirlockClient.Core;
using AirlockClient.Data;
using AirlockClient.Managers.Debug;
using AirlockClient.Managers.Dev;
using AirlockClient.Managers.Gamemode;
using AirlockClient.Patches;
using Il2CppFusion;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSG.Airlock.Network;
using Il2CppSystem.IO;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

namespace AirlockClient.Managers
{
    public class Listener : MonoBehaviour
    {
        public static Listener Instance;
        public static AudioSource Listener_SeekerMusic;

        void Start()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }
        }

        public static void OnMessageRecieved(string message, PlayerRef sender)
        {
            if (CurrentMode.IsHosting)
            {
                Send(message);
            }

            if (message.Contains("HideNSeek"))
            {
                if (message.Contains("PlayerIsHider"))
                {
                    if (GameObject.Find("NetworkedLocomotionPlayer (" + message.Replace("HideNSeek_PlayerIsHider_", "") + ")"))
                    {
                        DangerMeterManager.Init(GameObject.Find("NetworkedLocomotionPlayer (" + message.Replace("HideNSeek_PlayerIsHider_", "") + ")").GetComponent<NetworkedLocomotionPlayer>().TaskPlayer.transform);
                    }
                }

                if (message.Contains("PlayerIsSeeker"))
                {
                    if (Listener_SeekerMusic == null)
                    {
                        Listener_SeekerMusic = new GameObject("LISTENER_SeekerMusic").AddComponent<AudioSource>();
                        Listener_SeekerMusic.clip = StorageManager.SeekerMusic;
                        Listener_SeekerMusic.volume = 0.2f;
                        Listener_SeekerMusic.loop = true;

                        foreach (AudioMixerGroup group in Resources.FindObjectsOfTypeAll<AudioMixerGroup>())
                        {
                            if (group.name == "Music")
                            {
                                Listener_SeekerMusic.outputAudioMixerGroup = group;
                            }
                        }
                    }

                    Listener_SeekerMusic.Play();
                }

                if (message.Contains("GameEnd"))
                {
                    DangerMeterManager.DeInit();

                    if (Listener_SeekerMusic)
                    {
                        Listener_SeekerMusic.Stop();
                    }
                }
            }
            if (message.Contains("MoreRoles"))
            {
                if (message.Contains("RecievedRole"))
                {
                    string roleRecieved = message.Replace("MoreRoles_RecievedRole_", "");
                    
                    if (Type.GetType("AirlockClient.Data.Roles.MoreRoles." + roleRecieved) != null)
                    {
                        SubRoleData roleData = (SubRoleData)Type.GetType("AirlockClient.Data.Roles.MoreRoles." + roleRecieved).GetField("Data").GetValue(null);
                        Logging.Debug_Log("ROLE: " + roleData.Name + " | " + roleData.AC_Description);
                    }
                }
            }

            if (message.Contains("JoinedModdedLobby"))
            {
                Base.SceneStorage.AddComponent<PetManager>();
                Base.SceneStorage.AddComponent<CommandManager>();
            }

            if (message.Contains("COMMAND"))
            {
                if (CurrentMode.IsHosting)
                {
                    Send(message);
                }

                CommandManager.Instance.HandleCommand(message, sender);
            }

            if (message.Contains("UpdateNameTagList"))
            {
                CommandManager.QueuedCommands.Add(message);
            }
        }

        public static void Send(string message, PlayerRef client)
        {
            Il2CppStructArray<byte> data = ReliableDataPatch.StringToData(message);

            if (CurrentMode.IsHosting)
            {
                if (FindObjectOfType<AirlockNetworkRunner>())
                {
                    FindObjectOfType<AirlockNetworkRunner>().SendReliableDataToPlayer(client, data);
                }
            }
            else
            {
                FindObjectOfType<AirlockNetworkRunner>().SendReliableDataToServer(data);
            }
        }

        public static void Send(string message, bool includeSelf = false)
        {
            foreach (var player in FindObjectOfType<AirlockNetworkRunner>().ActivePlayers.ToArray())
            {
                if (player.PlayerId != 9)
                {
                    Send(message, player);
                }
                else
                {
                    if (includeSelf)
                    {
                        Send(message, player);
                    }
                }
            }
        }
    }
}
