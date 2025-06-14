﻿using AirlockClient.Attributes;
using Il2CppSG.Airlock.Roles;
using Il2CppSG.Airlock;
using System.Collections.Generic;

namespace AirlockClient.Managers.Gamemode
{
    public class LightsOutManager : ModdedGamemode
    {
        public override void OnGameStart()
        {
            ModdedGameStateManager.Instance.SetMatchSetting(Data.Enums.MatchFloatSettings.CrewmateVisionDistance, 0.25f);
            ModdedGameStateManager.Instance.SetMatchSetting(Data.Enums.MatchFloatSettings.ImpostorVisionDistance, 0.25f);

        }

        public override void OnAssignRoles()
        {
            List<PlayerState> normalCrewmates = new List<PlayerState>();
            List<PlayerState> imposters = new List<PlayerState>();
            foreach (PlayerState player in FindObjectsOfType<PlayerState>())
            {
                if (GetTrueRole(player) == GameRole.Crewmember)
                {
                    normalCrewmates.Add(player);
                }
                if (GetTrueRole(player) == GameRole.Imposter)
                {
                    imposters.Add(player);
                }
            }

            foreach (PlayerState crewmate in normalCrewmates)
            {
                if (crewmate != null)
                {
                    Role.AlterPlayerRole(GameRole.Engineer, crewmate.PlayerId);
                }
            }
        }
    }
}
