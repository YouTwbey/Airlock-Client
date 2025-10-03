using AirlockClient.Attributes;
using AirlockClient.Managers;
using AirlockClient.Managers.Gamemode;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Roles;
using MelonLoader;
using System.Collections;
using UnityEngine;

namespace AirlockClient.Data.Roles.HideNSeek.Imposter
{
    public class Seeker : SubRole
    {
        public static SubRoleData Data = new SubRoleData
        {
            Name = "Killer",
            Description = "Kill all crew",
            Team = GameTeam.Impostor,
            Amount = 1
        };

        void Start()
        {
            MelonCoroutines.Start(HideNSeekManager.DisplayRoleInfo(PlayerWithRole, this));
        }

        bool GameStart;
        void Update()
        {
            if (ModdedGameStateManager.Instance.state.InTaskState() && !GameStart)
            {
                MelonCoroutines.Start(StartTimer());
                GameStart = true;
            }
        }

        public override void OnPlayerRecievedRole()
        {
            PlayerWithRole.ActivePowerUps = PowerUps.None;
        }

        public IEnumerator StartTimer()
        {
            ModdedGameStateManager.Instance.SetMatchSetting(Enums.MatchFloatSettings.TaggedSpeedMultiplier, 0);
            yield return new WaitForSeconds(0.1f);
            ModdedGameStateManager.Instance.SetMatchSetting(Enums.MatchFloatSettings.TaggedSpeedMultiplier, 1.05f);
            ModdedGameStateManager.Instance.SetMatchSetting(Enums.MatchIntSettings.TagCooldown, 1);
        }
    }
}
