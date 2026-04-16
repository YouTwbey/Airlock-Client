using AirlockClient.Attributes;
using AirlockClient.Managers;
using Il2CppSG.Airlock.Roles;
using MelonLoader;
using System.Collections;
using UnityEngine;
using AirlockClient.Managers.Gamemode;
using AirlockClient.AC;

namespace AirlockClient.Data.Roles.MoreRoles.Modifiers
{
    /// <summary>
    /// Crewmate Role
    /// Person can not shut up, not speaking will result in their death.
    /// </summary>
    public class Yapper : Modifier
    {
        public static ModifierData Data = new ModifierData
        {
            Name = "Yapper",
            ModifierType = "Neutral",
            Description = "Keep talk or die",
            AC_Description = "If at any moment you stop talking, you will die.",
            Team = GameTeam.Other,
            Amount = 0
        };

        void Start()
        {
            MoreRolesManager.QueueModifierDisplay(PlayerWithModifier, Data);
        }


        bool isCheckInProgress;

        void Update()
        {
            if (ModdedGameStateManager.Instance.state.InTaskState() || ModdedGameStateManager.Instance.state.InVotingState())
            {
                if (PlayerWithModifier.MicrophoneOutput <= 0.1f && PlayerWithModifier.IsAlive && !isCheckInProgress)
                {
                    MelonCoroutines.Start(MicTimer());
                }
            }
        }

        IEnumerator MicTimer()
        {
            isCheckInProgress = true;

            yield return new WaitForSeconds(1);

            if (PlayerWithModifier && PlayerWithModifier.IsAlive && PlayerWithModifier.MicrophoneOutput <= 0.1f)
            {
                AntiCheat.KillPlayerWithAntiCheat(PlayerWithModifier, PlayerWithModifier);
            }

            isCheckInProgress = false;
        }
    }
}
