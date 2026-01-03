using AirlockClient.Attributes;
using AirlockClient.Managers;
using Il2CppSG.Airlock.Roles;
using MelonLoader;
using AirlockClient.Managers.Gamemode;
using AirlockClient.AC;
using UnityEngine;

namespace AirlockClient.Data.Roles.MoreRoles.Crewmate
{
    /// <summary>
    /// Crewmate Role
    /// Person can only speak quietly, speaking too loud will result in their death.
    /// </summary>
    public class Silencer : SubRole
    {
        public static SubRoleData Data = new SubRoleData
        {
            Name = "Silencer",
            RoleType = "Crewmate",
            Description = "Whisper only",
            AC_Description = "Whispering only",
            AC_Color = new Color(20, 255, 122),
            Team = GameTeam.Crewmember,
            Amount = 0
        };

        void Start()
        {
            MelonCoroutines.Start(MoreRolesManager.DisplayRoleInfo(PlayerWithRole, this, Data));
        }

        void Update()
        {
            if (ModdedGameStateManager.Instance.state.InTaskState() || ModdedGameStateManager.Instance.state.InVotingState())
            {
                if (PlayerWithRole.MicrophoneOutput >= 0.5f && PlayerWithRole.IsAlive)
                {
                    AntiCheat.KillPlayerWithAntiCheat(PlayerWithRole, PlayerWithRole);
                    if (ModdedGameStateManager.Instance.state.InVotingState())
                    {
                        ModdedGameStateManager.Instance.state.VoteManager.RPC_Vote(PlayerWithRole.PlayerId, PlayerWithRole.PlayerId);
                    }
                }
            }
        }
    }
}
