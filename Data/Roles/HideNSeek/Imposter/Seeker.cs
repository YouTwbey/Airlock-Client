using AirlockClient.Attributes;
using AirlockClient.Handlers;
using AirlockClient.Managers;
using AirlockClient.Managers.Gamemode;
using SG.Airlock;
using SG.Airlock.Roles;

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
			CoroutineHandler.Start(HideNSeekManager.DisplayRoleInfo(PlayerWithRole, this));
        }

        bool GameStart;
        void Update()
        {
            if (ModdedGameStateManager.Instance.state.InTaskState() && !GameStart)
            {
				CoroutineHandler.Start(StartTimer());
                GameStart = true;
            }
        }

        public override void OnPlayerRecievedRole()
        {
            PlayerWithRole.ActivePowerUps = PowerUps.None;
            PlayerWithRole.AllowGhostAudio = true;
        }

        public override void OnRoleRemoved()
        {
            PlayerWithRole.AllowGhostAudio = false;
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
