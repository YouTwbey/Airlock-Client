using AirlockClient.Attributes;
using AirlockClient.Data.Roles.Infection.Imposter;
using AirlockClient.Data.Roles.Infection.Crewmate;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Customization;
using Il2CppSG.Airlock.Network;
using Il2CppSG.Airlock.Roles;
using AirlockClient.AC;

namespace AirlockClient.Managers.Gamemode
{
    public class InfectionManager : AirlockClientGamemode
    {
        public static InfectionManager Instance;
        public SpawnManager spawn;
        public CustomizationManager wardrobe;
        public int buritto = 15;
        public int chef = 15;

        void Start()
        {
            if (Instance == null)
            {
                spawn = FindObjectOfType<SpawnManager>();
                wardrobe = FindObjectOfType<CustomizationManager>();

                foreach (CustomizationElement element in wardrobe._elementCollection.AllCustomizationElements)
                {
                    if (element.name == "CE_AO_Chef")
                    {
                        chef = wardrobe.FindElementIndex(element);
                    }
                    else if (element.name.ToLower().Contains("burrito"))
                    {
                        buritto = wardrobe.FindElementIndex(element);
                    }
                }

                Instance = this;
            }
            else
            {
                Destroy(this);
            }
        }

        void Update()
        {
            if (State.InTaskState())
            {
                foreach (PlayerState state in spawn.PlayerStates)
                {
                    if (!state.IsSpectating)
                    {
                        state.ActivePowerUps = PowerUps.None;

                        if (state.GetComponent<Chef>())
                        {
                            if (state.HatId != chef)
                            {
                                AntiCheat.ChangeHatWithAntiCheat(state, chef);
                            }
                        }
                        else
                        {
                            if (state.HatId != buritto)
                            {
                                AntiCheat.ChangeHatWithAntiCheat(state, buritto);
                            }
                        }
                    }
                }
            }
        }

        public override void OnAfterAssignRoles()
        {
            foreach (SubRole role in SubRole.All)
            {
                Destroy(role);
            }

            foreach (PlayerState player in FindObjectsOfType<PlayerState>())
            {
                if (GetTrueRole(player) == GameRole.Crewmember)
                {
                    player.gameObject.AddComponent<Chef>();
                }
                else
                {
                    player.gameObject.AddComponent<Zomburitto>();
                }
            }
        }
    }
}
