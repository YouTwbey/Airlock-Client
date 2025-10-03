using AirlockClient.Attributes;
using AirlockClient.Managers.Gamemode;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Roles;
using MelonLoader;
using System.Collections.Generic;
using UnityEngine;

namespace AirlockClient.Data.Roles.MoreRoles.Imposter
{
    /// <summary>
    /// A role that can disolve people's bodies when killed.
    /// </summary>
    public class Viper : SubRole
    {
        public static SubRoleData Data = new SubRoleData
        {
            Name = "Viper",
            RoleType = "Imposter",
            Description = "Disolve Bodies",
            AC_Description = "Anyone you kill, their bodies will be put on a timer before they fully disolve.",
            Team = GameTeam.Impostor,
            Amount = 0
        };

        void Start()
        {
            MelonCoroutines.Start(MoreRolesManager.DisplayRoleInfo(PlayerWithRole, this, Data));
        }

        public static float SecondsUntilDisolve = 30;
        List<NetworkedBody> bodiesToDisolve = new List<NetworkedBody>();
        public override void OnPlayerKilled(PlayerState playerKilled)
        {
            bodiesToDisolve.Add(GameObject.Find($"NetworkedBody ({playerKilled.PlayerId})").GetComponent<NetworkedBody>());
        }

        public override void OnVotingBegan(PlayerState bodyReported, PlayerState reportingPlayer)
        {
            bodiesToDisolve.Clear();
        }

        public override void OnGameEnd(GameTeam teamThatWon)
        {
            bodiesToDisolve.Clear();
        }

        void Update()
        {
            List<NetworkedBody> bodiesToRemove = new List<NetworkedBody>();

            foreach (NetworkedBody body in bodiesToDisolve)
            {
                body.transform.position += Vector3.down * (1 / SecondsUntilDisolve) * Time.deltaTime;

                if (body.transform.position.y <= -1)
                {
                    bodiesToRemove.Add(body);
                }
            }

            foreach (NetworkedBody body in bodiesToRemove)
            {
                body.RPC_ToggleBody(false);
                bodiesToDisolve.Remove(body);
            }
        }
    }
}
