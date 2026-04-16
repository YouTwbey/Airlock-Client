using AirlockClient.Attributes;
using AirlockClient.Managers;
using AirlockClient.Managers.Debug;
using AirlockClient.Managers.Gamemode;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirlockClient.Data.Roles.MoreRoles.Modifiers
{
    public class DSpUp : Modifier
    {
        public VoteManager voteMananger;

        public override void OnPlayerRecievedRole()
        {
            voteMananger = FindObjectOfType<VoteManager>();
            Logging.Debug_Log($"playerwithmodifier: {PlayerWithModifier.NetworkName.ToString()}");
        }

        public static ModifierData Data = new ModifierData
        {
            Name = "DSpUP", // shortened from DeputySpeedUp because of naming reasons
            ModifierType = "Neutral",
            Description = "Speed boost when Deputy",
            AC_Description = "You are 10% faster than other deputy's",
            Team = GameTeam.Other,
            Amount = 0
        };

        void Start()
        {
            MoreRolesManager.QueueModifierDisplay(PlayerWithModifier, Data);
        }
    }
}
