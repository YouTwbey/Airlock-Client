using AirlockClient.Attributes;
using AirlockClient.Managers;
using Il2CppSG.Airlock.Roles;
using MelonLoader;
using AirlockClient.Managers.Gamemode;
using AirlockClient.AC;

namespace AirlockClient.Data.Roles.MoreRoles.Modifiers
{
    /// <summary>
    /// Crewmate Role
    /// Person can only speak quietly, speaking too loud will result in their death.
    /// </summary>
    public class Muted : Modifier
    {
        public static ModifierData Data = new ModifierData
        {
            Name = "Muted",
            ModifierType = "Neutral",
            Description = "Whisper only",
            AC_Description = "Talking too loud will result in your death.",
            Team = GameTeam.Other,
            Amount = 0
        };

        void Start()
        {
            MoreRolesManager.QueueModifierDisplay(PlayerWithModifier, Data);
        }

        void Update()
        {
            if (ModdedGameStateManager.Instance.state.InTaskState() || ModdedGameStateManager.Instance.state.InVotingState())
            {
                if (PlayerWithModifier.MicrophoneOutput >= 0.5f && PlayerWithModifier.IsAlive)
                {
                    AntiCheat.KillPlayerWithAntiCheat(PlayerWithModifier, PlayerWithModifier);
                    if (ModdedGameStateManager.Instance.state.InVotingState())
                    {
                        ModdedGameStateManager.Instance.state.VoteManager.RPC_Vote(PlayerWithModifier.PlayerId, PlayerWithModifier.PlayerId);
                    }
                }
            }
        }
    }
}
