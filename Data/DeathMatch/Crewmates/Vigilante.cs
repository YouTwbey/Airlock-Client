using AirlockClient.AC;
using AirlockClient.Attributes;
using AirlockClient.Managers.Debug;
using AirlockClient.Managers.Gamemode;
using Il2CppSG.Airlock;
using MelonLoader;
using System.Collections;
using UnityEngine;

namespace AirlockClient.Data.Roles.DeathMatch.Crewmates
{
    public class Vigilante : SubRole
    {
        private bool _isReviving = false;

        public override void OnPlayerDied(PlayerState killer)
        {
            if (_isReviving) return;
            _isReviving = true;

            if (killer != PlayerWithRole)
            {
                DeathMatchManager.ImpostorTeamPoints += 1;
            }
            MelonCoroutines.Start(DelayReviveDeadPlayers(15, PlayerWithRole));
        }

        public IEnumerator DelayReviveDeadPlayers(int delay, PlayerState dead)
        {
            Logging.Debug_Log($"Player Has Died waiting {delay} to revive them");

            yield return new WaitForSeconds(delay);

            dead.IsAlive = true;
            _isReviving = false;

            AntiCheat.PlayShieldBreakWithAntiCheat(dead, dead);

            Logging.Debug_Log("Player Has Been Revived");
        }
    }
}
