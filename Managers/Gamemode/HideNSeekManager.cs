using AirlockClient.Attributes;
using Il2CppSG.Airlock.Roles;
using Il2CppSG.Airlock;
using UnityEngine;
using static AirlockClient.Data.Enums;
using AirlockClient.Data.Roles.HideNSeek.Imposter;
using AirlockClient.Data.Roles.HideNSeek.Crewmate;
using Il2CppSG.Airlock.Sabotage;
using UnityEngine.Audio;
using Il2CppSG.Airlock.Network;

namespace AirlockClient.Managers.Gamemode
{
    public class HideNSeekManager : ModdedGamemode
    {
        public AudioSource SeekerMusic;
        public static Seeker seeker;
        public bool AllowSabotagesToBeTurnedOff;
        bool GameStarted;
        bool InLobby;
        bool FinalHide;

        // Gamemode Settings
        public float Timer = 180;
        public bool ShowNametags = true;
        public float FinalHideSpeedBoost = 1.3f;

        void Start()
        {
            SeekerMusic = new GameObject("SeekerMusic").AddComponent<AudioSource>();
            SeekerMusic.loop = true;
            SeekerMusic.clip = StorageManager.SeekerMusic;
            SeekerMusic.volume = 0.2f;

            foreach (AudioMixerGroup group in Resources.FindObjectsOfTypeAll<AudioMixerGroup>())
            {
                if (group.name == "Music")
                {
                    SeekerMusic.outputAudioMixerGroup = group;
                }
            }
        }

        public override void OnGameStart()
        {
            ModdedGameStateManager.Instance.SetMatchSetting(MatchIntSettings.MaxInfected, 1);
            ModdedGameStateManager.Instance.SetMatchSetting(MatchIntSettings.NumImposters, 1);
            ModdedGameStateManager.Instance.SetMatchSetting(MatchIntSettings.TagCooldown, 10);
            ModdedGameStateManager.Instance.SetMatchSetting(MatchIntSettings.TagTotalTasks, 200);

            State._gamemodeTimerCurrent = 0;
            State._gamemodeTimerRunning = false;
            State._preventMatchEnding.Value = true;

            foreach (SubRole role in SubRole.All)
            {
                if (role.PlayerWithRole.IsSpawned)
                {
                    role.PlayerWithRole.ActivePowerUps = PowerUps.None;
                }
            }

            GameStarted = false;
            FinalHide = false;
            AllowSabotagesToBeTurnedOff = false;
        }

        public override void OnGameEnd(GameTeam teamThatWon)
        {
            State._gamemodeTimerCurrent = 0;
            State._gamemodeTimerRunning = false;
            GameStarted = false;
            FinalHide = false;
            AllowSabotagesToBeTurnedOff = true;

            DangerMeterManager.DeInit();
            SeekerMusic.Stop();
            FindObjectOfType<SabotageManager>().RPC_EndSabotage(false);

            foreach (SubRole role in SubRole.All)
            {
                Listener.Send("HideNSeek_GameEnd", role.PlayerWithRole.PlayerId);
                if (role.PlayerWithRole.IsSpawned)
                {
                    role.PlayerWithRole.ActivePowerUps = PowerUps.None;
                }
            }

            if (ShowNametags == false)
            {
                foreach (SubRole player in SubRole.All)
                {
                    NetworkedLocomotionPlayer loco = player.PlayerWithRole.LocomotionPlayer;

                    loco.RPC_ToggleExternalUINames(true);
                }
            }
        }

        void Update()
        {
            if (State.InTaskState())
            {
                if (GameStarted == false)
                {
                    State._gamemodeTimerCurrent = Timer;
                    State._gamemodeTimerRunning = true;

                    if (State._xrRig.PState == seeker.PlayerWithRole)
                    {
                        SeekerMusic.Play();
                    }
                    else
                    {
                        DangerMeterManager.Init(seeker.PlayerWithRole.LocomotionPlayer.TaskPlayer.transform);
                    }

                    foreach (SubRole player in SubRole.All)
                    {
                        if (GetTrueRole(player.PlayerWithRole) == GameRole.Infected)
                        {
                            Listener.Send("HideNSeek_PlayerIsSeeker_", player.PlayerWithRole.PlayerId);
                        }
                        else
                        {
                            Listener.Send("HideNSeek_PlayerIsHider_" + seeker.PlayerWithRole.PlayerId.ToString(), player.PlayerWithRole.PlayerId);
                        }

                        NetworkedLocomotionPlayer loco = player.PlayerWithRole.LocomotionPlayer;
                        loco.RPC_ToggleExternalUINames(ShowNametags);
                    }

                    GameStarted = true;
                    InLobby = false;
                }
            }
            else
            {
                GameStarted = false;

                if (!InLobby)
                {
                    InLobby = true;
                }
            }

            if (GameStarted)
            {
                if (!FinalHide)
                {
                    if (State._gamemodeTimerCurrent <= 0)
                    {
                        FindObjectOfType<SabotageManager>().BeginSabotageServer(2);
                        ModdedGameStateManager.Instance.SetMatchSetting(MatchFloatSettings.TaggedSpeedMultiplier, FinalHideSpeedBoost);
                        State._gamemodeTimerCurrent = 35;
                        State._gamemodeTimerRunning = true;

                        foreach (SubRole role in SubRole.All)
                        {
                            role.PlayerWithRole.LocomotionPlayer.TaskPlayer.AssignedTasks.Clear();
                        }

                        FinalHide = true;
                    }
                }
                else
                {
                    if (State._gamemodeTimerCurrent <= 0)
                    {
                        State.GameEndReasonIndex = 999999999;
                        State.EndGame(GameTeam.Crewmember);
                    }
                }

                int totalAlive = 0;

                foreach (SubRole role in SubRole.All)
                {
                    if (role.GetComponent<Hider>())
                    {
                        if (role.PlayerWithRole.IsAlive)
                        {
                            totalAlive = totalAlive + 1;
                        }
                    }
                }

                if (totalAlive == 0)
                {
                    State.GameEndReasonIndex = State.LowCrewmateCountWin;
                    State.EndGame(GameTeam.Imposter);
                }

                if (!seeker.PlayerWithRole.IsConnected)
                {
                    State.GameEndReasonIndex = State.NoImpostorsLeftWin;
                    State.EndGame(GameTeam.Crewmember);
                }
            }
        }

        public override void OnAssignRoles()
        {
            foreach (SubRole role in SubRole.All)
            {
                Destroy(role);
            }

            foreach (PlayerState player in FindObjectsOfType<PlayerState>())
            {
                if (GetTrueRole(player) == GameRole.Infected)
                {
                    player.gameObject.AddComponent<Seeker>();
                    seeker = player.GetComponent<Seeker>();
                }
                else
                {
                    player.gameObject.AddComponent<Hider>();
                }
            }
        }

        public static System.Collections.IEnumerator DisplayRoleInfo(PlayerState Player, SubRole Role)
        {
            if (Player != null && Role != null)
            {
                Role.IsDisplayingRole = true;
                string ogName = Player.NetworkName.Value;
                int ogHat = Player.HatId;
                int ogHands = Player.HandsId;
                int ogSkin = Player.SkinId;

                yield return new WaitForSeconds(1);
                Player.NetworkName = seeker.PlayerWithRole.NetworkName;
                Player.HatId = seeker.PlayerWithRole.HatId;
                Player.HandsId = seeker.PlayerWithRole.HandsId;
                Player.SkinId = seeker.PlayerWithRole.SkinId;
                Hider.ForceHiderColorId = seeker.PlayerWithRole.ColorId;
                yield return new WaitForSeconds(8);
                Hider.ForceHiderColorId = -1;
                Player.NetworkName = ogName;
                Player.HatId = ogHat;
                Player.HandsId = ogHands;
                Player.SkinId = ogSkin;
                Role.IsDisplayingRole = false;
            }
        }
    }
}
