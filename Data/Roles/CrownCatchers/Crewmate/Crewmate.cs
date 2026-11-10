using AirlockClient.Attributes;
using AirlockClient.Managers.Gamemode;
using SG.Airlock;
using SG.Airlock.Roles;

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
