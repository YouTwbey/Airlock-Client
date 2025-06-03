using AirlockClient.Attributes;
using Il2CppSG.Airlock.Roles;

namespace AirlockClient.Data.Roles.Infection.Imposter
{
    public class Zomburitto : SubRole
    {
        public static SubRoleData Data = new SubRoleData
        {
            Name = "Zomburitto",
            Description = "infect all",
            Team = GameTeam.Imposter,
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
