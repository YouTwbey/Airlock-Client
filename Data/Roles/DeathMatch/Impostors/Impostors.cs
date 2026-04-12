using AirlockClient.AC;
using AirlockClient.Attributes;
using AirlockClient.Managers.Debug;
using AirlockClient.Managers.Gamemode;
using Il2CppSG;
using Il2CppSG.Airlock;
using MelonLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AirlockClient.Data.Roles.DeathMatch.Impostors
{
    public class Impostors : SubRole
    {
        public bool _isReviving = false;

        public override void OnPlayerDied(PlayerState killer)
        {
            if (_isReviving) return;
            _isReviving = true;

            if (killer != PlayerWithRole)
            {
                DeathMatchManager.VigilanteTeamPoints += 1;
            }
            MelonCoroutines.Start(DelayReviveDeadPlayers(15, PlayerWithRole));
        }

        public IEnumerator DelayReviveDeadPlayers(int delay, PlayerState dead)
        {
            Logging.Log($"Player Has Died waiting {delay} to revive them");

            yield return new WaitForSeconds(delay);

            dead.IsAlive = true;
            _isReviving = false;

            AntiCheat.PlayShieldBreakWithAntiCheat(dead, dead);

            Logging.Log("Player Has Been Revived");
        }

        public override void OnPlayerKilled(PlayerState playerKilled)
        {
            
        }
    }
}
