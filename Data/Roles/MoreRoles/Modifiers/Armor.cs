using AirlockClient.Attributes;
using AirlockClient.Managers;
using AirlockClient.Managers.Gamemode;
using Il2CppSG.Airlock.Roles;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirlockClient.Data.Roles.MoreRoles.Modifiers
{
    public class Armor : Modifier
    {
        public static ModifierData Data = new ModifierData
        {
            Name = "Armor",
            ModifierType = "Crewmate",
            Description = "Take 2 Hits",
            AC_Description = "You are able to withstand 2 kills from an imposter.",
            Team = GameTeam.Crewmember,
            Amount = 0
        };
        public bool HasTakenHit;
        void Start()
        {
            MoreRolesManager.QueueModifierDisplay(PlayerWithModifier, Data);
        }
    }
}
