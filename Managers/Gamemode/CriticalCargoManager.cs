using AirlockClient.Attributes;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Customization;
using Il2CppSG.Airlock.Network;
using Il2CppSG.Airlock.Roles;
using System.Collections.Generic;
using System.Linq;

namespace AirlockClient.Managers.Gamemode
{
    public class CriticalCargoManager : AirlockClientGamemode
    {
        public static CriticalCargoManager Instance;
        SpawnManager Spawn;
        CustomizationManager Wardrobe;
        NetworkedKillBehaviour Kill;
        int critHatId;
        int goldCritHatId;

        void Start()
        {
            if (Instance == null)
            {
                Spawn = FindObjectOfType<SpawnManager>();
                Wardrobe = FindObjectOfType<CustomizationManager>();
                Kill = FindObjectOfType<NetworkedKillBehaviour>();

                foreach (CustomizationElement element in Wardrobe._elementCollection.AllCustomizationElements)
                {
                    if (element.name.Contains("ScanalyzerHat"))
                    {
                        critHatId = Wardrobe.FindElementIndex(element);
                    }
                    else if (element.name.Contains("ScanalyzerDeluxeHat"))
                    {
                        goldCritHatId = Wardrobe.FindElementIndex(element);
                    }
                }

                Instance = this;
            }
            else
            {
                Destroy(this);
            }
        }

        public override bool OnGameStart()
        {
            foreach (PlayerState player in Spawn.ActivePlayerStates)
            {
                if (player.HatId != goldCritHatId)
                {
                    player.HatId = critHatId;
                }
            }

            return true;
        }

        List<PlayerState> Crewmates = new List<PlayerState>();
        List<PlayerState> Imposters = new List<PlayerState>();
        public override void OnAfterAssignRoles()
        {
            Crewmates.Clear();
            Imposters.Clear();

            foreach (PlayerState player in FindObjectsOfType<PlayerState>())
            {
                if (GetTrueRole(player) == GameRole.Impostor)
                {
                    Imposters.Add(player);
                }
                else
                {
                    Crewmates.Add(player);
                }
            }

            System.Random rng1 = new System.Random();
            Crewmates = Crewmates.OrderBy(_ => rng1.Next()).ToList();
            Current.Role.AlterPlayerRole(GameRole.VIP, Crewmates[0].PlayerId);
            Current.Role.AlterPlayerRole(GameRole.VIP, Crewmates[1].PlayerId);
            
            foreach (PlayerState imposter in Imposters)
            {
                Kill.RPC_Scan(imposter.PlayerId, Crewmates[0].PlayerId, 0);
                Kill.RPC_Scan(imposter.PlayerId, Crewmates[1].PlayerId, 0);
                Kill.RPC_Scan(imposter.PlayerId, Crewmates[2].PlayerId, 0);
            }
        }
    }
}
