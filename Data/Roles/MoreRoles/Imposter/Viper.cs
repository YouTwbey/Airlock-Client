using AirlockClient.Attributes;
using AirlockClient.Managers.Gamemode;
using SG.Airlock;
using SG.Airlock.Roles;

using System.Collections.Generic;
using AirlockClient.Managers.Lobby;
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

        public static List<AdvancedSettingDefinition> AdvancedSettings =
        [
            MoreRolesManager.DefaultAdvancedSettings[0],
            new AdvancedSettingDefinition(
                "Dissolve Time",
                data => SecondsUntilDisolve,
                (roleKey, delta) =>
                    MoreRolesManager.Instance.ChangeDissolveTime(delta),
                5
            )
        ];

        void Start()
        {
            MoreRolesManager.QueueRoleDisplay(PlayerWithRole, this, Data);
        }

        public static int SecondsUntilDisolve = 15;
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
