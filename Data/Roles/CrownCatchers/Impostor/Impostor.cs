using AirlockAPI.Data;
using AirlockClient.Attributes;
using AirlockClient.Managers;
using AirlockClient.Managers.Gamemode;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Roles;
using MelonLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AirlockClient.Data.Roles.CrownCatchers.Impostor
{
    public class Crowned : SubRole
    {
        public static Dictionary<int, float> playerCrownTimes = new Dictionary<int, float>();
        public float PlayersTime;

        SubRoleData Data = new SubRoleData
        {
            Name = "Crowned",
            Amount = 1,
            Team = GameTeam.Impostor
        };

        void Start()
        {
            PlayerWithRole.ActivePowerUps = PowerUps.None;   
        }

        public override void OnPlayerRecievedRole()
        {
            PlayerWithRole.ActivePowerUps = PowerUps.None;
            PlayerWithRole.HatId = CrownRunnersManager.Crown;
            MelonCoroutines.Start(DelayedAlterRole());
        }

        public IEnumerator DelayedAlterRole()
        {
            yield return new WaitForSeconds(5f);

            CrownRunnersManager.Kill.AlterRole(GameRole.Sheriff, PlayerWithRole.PlayerId);
        }

        void Update()
        {
            if (PlayerWithRole != null && CrownRunnersManager.state.GameModeStateValue.GameState == GameplayStates.Task && CurrentMode.Name == "Crown Runners")
                PlayersTime += Time.deltaTime;

            float totalTime = PlayersTime;
            if (playerCrownTimes.ContainsKey(PlayerWithRole.PlayerId) && CrownRunnersManager.state.GameModeStateValue.GameState == GameplayStates.Task)
                totalTime += playerCrownTimes[PlayerWithRole.PlayerId];

            if (totalTime >= 45)
            {
                CrownRunnersManager.state.EndGame(GameTeam.Other);
            }
        }

        void OnDestroy()
        {
            if (PlayerWithRole == null) return;

            if (CrownRunnersManager.state.GameModeStateValue.GameState != GameplayStates.Task) return;

            int id = PlayerWithRole.PlayerId;

            if (playerCrownTimes.ContainsKey(id))
                playerCrownTimes[id] += PlayersTime;
            else
                playerCrownTimes.Add(id, PlayersTime);
        }

        public override void OnGameEnd(GameTeam teamThatWon)
        {
            PlayersTime = 0;
        }
    }
}
