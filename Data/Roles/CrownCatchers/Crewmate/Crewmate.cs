using AirlockClient.Attributes;
using AirlockClient.Managers.Gamemode;
using SG.Airlock;
using SG.Airlock.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirlockClient.Data.Roles.CrownCatchers.Crewmate
{
    public class Catcher : SubRole
    {
        SubRoleData Data = new SubRoleData
        {
            Name = "Catcher",
            Amount = 9,
            Team = GameTeam.Crewmember
        
        };

        public override void OnPlayerRecievedRole()
        {
            PlayerWithRole.ActivePowerUps = PowerUps.Guard;
        }

        void Start()
        {
            CrownRunnersManager.Kill.AlterRole(GameRole.Engineer, PlayerWithRole.PlayerId);
            PlayerWithRole.ActivePowerUps = PowerUps.Guard;
        }
    }
}
