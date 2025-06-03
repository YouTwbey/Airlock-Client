using AirlockClient.Managers.Debug;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Network;
using UnityEngine;

namespace AirlockClient.Handlers
{
    public class PetHandler : MonoBehaviour
    {
        public bool isMine;
        public Transform player;
        public PlayerState PState;
        public float moveSpeed = 6.5f;
        public float startFollowDistance = 2f;
        public float yAxisOffset = 0.5f;

        void Start()
        {
            if (GetComponentInParent<NetworkedLocomotionPlayer>())
            {
                player = GetComponentInParent<NetworkedLocomotionPlayer>().TaskPlayer.transform;
                PState = GetComponentInParent<NetworkedLocomotionPlayer>().PState;
            }
            else
            {
                Destroy(this);
            }
        }

        void Update()
        {
            if (!PState.IsConnected) Destroy(gameObject);

            if (isMine)
            {
                HandleMovement(Camera.main.transform);
            }
            else
            {
                if (player == null)
                {
                    Logging.Warn("Player Transform not assigned on " + gameObject.name);
                    return;
                }

                HandleMovement(player);
            }
        }

        void HandleMovement(Transform referenceTransform)
        {
            if (PState.IsAlive)
            {
                Vector3 petPos = new Vector3(transform.position.x, 0, transform.position.z);
                Vector3 targetPos = new Vector3(referenceTransform.position.x, 0, referenceTransform.position.z);

                float distance = Vector3.Distance(petPos, targetPos);

                if (PState.InVent || PState.IsSpectating)
                {
                    targetPos = new Vector3(1000, 1000, 1000);
                    transform.position = targetPos;
                    return;
                }

                if (distance > startFollowDistance)
                {
                    Vector3 newPos = Vector3.MoveTowards(petPos, targetPos, moveSpeed * Time.deltaTime);
                    transform.position = new Vector3(newPos.x, yAxisOffset, newPos.z);
                }

                if (distance > 5 && !PState.InVent && !PState.IsSpectating)
                {
                    transform.position = targetPos + new Vector3(0, yAxisOffset, 0);
                }

                HandleCamera();
            }
        }

        void HandleCamera()
        {
            if (PState == null) return;
            if (PState.LocomotionPlayer == null) return;
            if (Camera.main == null) return;
            if (transform == null) return;

            Vector3 lookAtPos = Vector3.zero;

            if (isMine)
            {
                lookAtPos = Camera.main.transform.position;
            }
            else
            {
                lookAtPos = PState.LocomotionPlayer.RigidbodyPosition;
            }

            lookAtPos.y = transform.position.y;
            transform.LookAt(lookAtPos);
        }
    }
}