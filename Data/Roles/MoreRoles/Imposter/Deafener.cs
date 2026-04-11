using AirlockClient.Attributes;
using AirlockClient.Managers.Gamemode;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Roles;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AirlockClient.Data.Roles.MoreRoles.Imposter
{
    public class Deafener : SubRole
    {
        public static SubRoleData Data = new SubRoleData
        {
            Name = "Deafener",
            RoleType = "Imposter",
            Description = "Mute others",
            AC_Description = "Mute one other person every meeting for the rest of the game",
            Team = GameTeam.Impostor,
            Amount = 0
        };

        void Start()
        {
            MoreRolesManager.QueueRoleDisplay(PlayerWithRole, this, Data);
        }

        public bool CanMutePlayer = true;
        public PlayerState PlayerToMute = null;
        public GameRole OriginalRole = GameRole.NotSet;

        public override void OnAllVotesCast()
        {
            if (PlayerToMute != null && PlayerToMute.IsSpectating == true && OriginalRole != GameRole.NotSet)
            {
                PlayerToMute.IsSpectating = false;
                MoreRolesManager.Killing.AlterRole(OriginalRole, PlayerToMute.PlayerId);
            }
        }

        public override void OnVotingBegan(PlayerState bodyReported, PlayerState reportingPlayer)
        {
            if (PlayerToMute != null)
            {
                MelonCoroutines.Start(DelaySpectator());
            }
        }

        public override void OnPlayerEjected(PlayerState ejectedPlayer, GameRole role)
        {

        }

        public System.Collections.IEnumerator DelaySpectator()
        {
            yield return new WaitForSeconds(2f);

            PlayerToMute.IsSpectating = true;
        }
    }
}
