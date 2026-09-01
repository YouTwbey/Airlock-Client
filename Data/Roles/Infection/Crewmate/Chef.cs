using AirlockClient.Attributes;
using SG.Airlock.Roles;

namespace AirlockClient.Data.Roles.Infection.Crewmate
{
    public class Chef : SubRole
    {
        public static SubRoleData Data = new SubRoleData
        {
            Name = "Chef",
            Description = "do tasks",
            Team = GameTeam.Crewmember,
            Amount = 0
        };

        int defaultHat;

        public override void OnPlayerRecievedRole()
        {
            defaultHat = PlayerWithRole.HatId;
        }

        public override void OnGameEnd(GameTeam teamThatWon)
        {
            PlayerWithRole.HatId = defaultHat;
        }
    }
}
