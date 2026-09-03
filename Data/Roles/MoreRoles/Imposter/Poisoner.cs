using AirlockClient.AC;
using AirlockClient.Attributes;
using AirlockClient.Handlers;
using AirlockClient.Managers.Gamemode;
using SG.Airlock;
using SG.Airlock.Roles;
using System.Collections;
using AirlockClient.Utils;
using UnityEngine;

namespace AirlockClient.Data.Roles.MoreRoles.Imposter
{
    /// <summary>
    /// An imposter role with delayed kills.
    /// </summary>
    public class Poisoner : SubRole
    {
        public static SubRoleData Data = new SubRoleData
        {
            Name = "Poisoner",
            RoleType = "Imposter",
            Description = "20 Sec Delay",
            AC_Description = "Kills are delayed by 20 seconds",
            AC_Color = new Color(150, 0, 150),
            Team = GameTeam.Impostor,
            Amount = 0
        };

        void Start()
        {
            MelonCoroutines.Start(MoreRolesManager.DisplayRoleInfo(PlayerWithRole, this, Data));
        }

        public override void OnPlayerKilled(PlayerState playerKilled)
        {
            CoroutineHandler.Start(DelayedKill(playerKilled));
        }

        IEnumerator DelayedKill(PlayerState target)
        {
            yield return new WaitForSeconds(20);

            if (AirlockClientGamemode.Current.State.InTaskState() && AirlockClientGamemode.Current.State.InPlayableGameState())
            {
                if (target.GetComponent<SubRole>())
                {
                    target.GetComponent<SubRole>().OnPlayerDied(PlayerWithRole);
                }

                PlayerWithRole.KillPlayerWithAntiCheat(target);
            }
        }
    }
}
