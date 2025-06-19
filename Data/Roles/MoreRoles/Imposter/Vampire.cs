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
    /// Imposter Role
    /// A vampire can only kill in the dark.
    /// </summary>
    public class Vampire : SubRole
    {
        public static SubRoleData Data = new SubRoleData
        {
            Name = "Vampire",
            Description = "10 Sec Delay",
            AC_Description = "Your kills are delayed by 10 seconds.",
            Team = GameTeam.Imposter,
            Amount = 1
        };

        public void DelayedKill(PlayerState target, int action)
        {
            MelonCoroutines.Start(DoDelayedKill(target, action));
        }

        IEnumerator DoDelayedKill(PlayerState target, int action)
        {
            yield return new WaitForSeconds(10);
            AntiCheat.KillPlayerWithAntiCheat(PlayerWithRole, target);
            if (target.GetComponent<SubRole>() != null)
            {
                target.GetComponent<SubRole>().OnPlayerDied(PlayerWithRole);
            }

            OnPlayerKilled(target);
            OnPlayerAction(action);
        }

        void Start()
        {
            MelonCoroutines.Start(MoreRolesManager.DisplayRoleInfo(PlayerWithRole, this, Data));
        }
    }
}
