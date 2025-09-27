using AirlockClient.AC;
using AirlockClient.Attributes;
using AirlockClient.Managers.Gamemode;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Roles;
using MelonLoader;
using System.Collections;
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
            AC_Description = "A poisoner's kills are delayed by 20 seconds.",
            Team = GameTeam.Imposter,
            Amount = 0
        };

        void Start()
        {
            MelonCoroutines.Start(MoreRolesManager.DisplayRoleInfo(PlayerWithRole, this, Data));
        }

        public override void OnPlayerKilled(PlayerState playerKilled)
        {
            MelonCoroutines.Start(DelayedKill(playerKilled));
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

                AntiCheat.KillPlayerWithAntiCheat(PlayerWithRole, target);
            }
        }
    }
}
