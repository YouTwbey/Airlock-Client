using Il2CppSG.Airlock.Roles;
using UnityEngine;

namespace AirlockClient.Attributes
{
    public class ModifierData
    {
        public int Amount;
        public int Chance = 50;
        public string Name;
        public string ModifierType;
        public string Description;
        public Color AC_Color = Color.white;
        public string AC_Description;
        public GameTeam Team;
    }
}
