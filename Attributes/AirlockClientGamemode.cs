using Il2CppSystem.Collections.Generic;
using Il2CppSG.Airlock;
using AirlockAPI.Handlers;

namespace AirlockClient.Attributes
{
    public class AirlockClientGamemode : CustomGameHandler
    {
        public Dictionary<PlayerState, SubRole> AssignedSubRoles = new Dictionary<PlayerState, SubRole>();

        public static AirlockClientGamemode Get()
        {
            return (AirlockClientGamemode)Current;
        }
    }
}
