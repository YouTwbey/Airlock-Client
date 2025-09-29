using AirlockClient.Handlers;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Network;
using Il2CppSG.Airlock.XR;
using UnityEngine;

namespace AirlockClient.Managers
{
    public class PetManager : MonoBehaviour
    {
        public static PetManager Instance;

        void Start()
        {
            if (Instance == null)
            {
                Instance = this;

                foreach (PlayerState state in FindObjectsOfType<PlayerState>())
                {
                    if (state != null)
                    {
                        if (state.IsSpawned)
                        {
                            if (state.IsConnected)
                            {
                                //AssignDebugPet(state.LocomotionPlayer);
                            }
                        }
                    }
                }
            }
            else
            {
                Destroy(this);
            }
        }

        public void AssignDebugPet(NetworkedLocomotionPlayer player)
        {
            GameObject debugPet = new GameObject("PET_Debug");
            debugPet.transform.parent = player.transform;
            debugPet.AddComponent<PetHandler>();

            if (player.TaskPlayer == FindObjectOfType<XRRig>().TaskPlayer)
            {
                debugPet.GetComponent<PetHandler>().isMine = true;
            }

            SpriteRenderer rend = debugPet.AddComponent<SpriteRenderer>();
            rend.useLightProbes = true;
            rend.sprite = StorageManager.ModStamp;
        }
    }
}
