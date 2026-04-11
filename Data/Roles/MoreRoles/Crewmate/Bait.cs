using AirlockClient.Attributes;
using AirlockClient.Data.Roles.MoreRoles.Neutral;
using AirlockClient.Managers.Debug;
using AirlockClient.Managers.Gamemode;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Roles;
using MelonLoader;
using static UnityEngine.Object;

namespace AirlockClient.Data.Roles.MoreRoles.Crewmate
{
    /// <summary>
    /// A crewmate role that forces the imposter to report your body once you die.
    /// </summary>
    public class Bait : SubRole
    {
        public Arsonist arsonist = null;
        public static SubRoleData Data = new SubRoleData
        {
            Name = "Bait",
            RoleType = "Crewmate",
            Description = "Auto Report Body",
            AC_Description = "When an imposter kills you, they will automatically report your body.",
            Team = GameTeam.Crewmember,
            Amount = 0
        };

        void Start()
        {
            if (arsonist == null)
            {
                arsonist = FindObjectOfType<Arsonist>();
            }
            MoreRolesManager.QueueRoleDisplay(PlayerWithRole, this, Data);
        }

        public override void OnPlayerDied(PlayerState killer)
        {
            if (arsonist != null && arsonist.PlayerWithRole != null)
            {
                if (killer.KnownGameRole != GameRole.GuardianAngel && killer.KnownGameRole != GameRole.Tracker && killer != arsonist.PlayerWithRole && killer.KnownGameRole != GameRole.Revenger && killer.KnownGameRole != GameRole.Sheriff && killer.KnownGameRole != GameRole.VIP)
                {
                    if (FindObjectOfType<VoteManager>())
                    {
                        FindObjectOfType<VoteManager>().RPC_CallVote(PlayerWithRole.PlayerId, killer.PlayerId, true);
                        Logging.Log($"Arsonist isnt null and somebody has the role. calling meeting because killer didnt have any of the specified roles: {killer.KnownGameRole.ToString()}");
                        return;
                    }
                    else
                    {
                        Logging.Log($"'killer' had one of the specified gameroles: {killer.KnownGameRole.ToString()} or {arsonist.PlayerWithRole.PlayerModerationUsername ?? "nobody"} is arsonist");
                    }
                }
            }
            else 
            {
                if (killer.KnownGameRole != GameRole.GuardianAngel && killer.KnownGameRole != GameRole.Tracker && killer.KnownGameRole != GameRole.Revenger && killer.KnownGameRole != GameRole.Sheriff && killer.KnownGameRole != GameRole.VIP)
                {
                    if (FindObjectOfType<VoteManager>())
                    {
                        FindObjectOfType<VoteManager>().RPC_CallVote(PlayerWithRole.PlayerId, killer.PlayerId, true);
                        Logging.Log($"Arsonist is null or nobody has the role. calling meeting because killer didnt have any of the specified roles: {killer.KnownGameRole.ToString()}");
                        return;
                    }
                }
                else
                {
                    Logging.Log($"'killer' had one of the specified gameroles: {killer.KnownGameRole.ToString()}");
                } 
                    
            }
            Logging.Error("VoteManager happens to be null in OnPlayerDied.");
        }
    }
}
