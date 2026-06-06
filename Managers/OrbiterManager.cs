using UnityEngine;
using System.Collections.Generic;
namespace AirlockClient.Managers
{
    public class OrbiterManager : MonoBehaviour
    {
        private Vector3 startPosition;
        private Vector3 endPosition;
        private float speed = 0.1f;
        public float t = 0f;
        public bool isActive = false;
        public float yPosition = -5.32f;

        void Start()
        {
            startPosition = new Vector3(-10.1474f, yPosition, 7.8073f);
            endPosition = new Vector3(11.7326f, yPosition, 7.8073f);
            transform.position = startPosition;
        }

        void Update()
        {
            if (!isActive) return;
            t += Time.deltaTime * speed;
            if (t >= 1f)
            {
                t = 0f;
                isActive = false;
                transform.position = startPosition;

                OrbiterManager[] all = FindObjectsOfType<OrbiterManager>();
                List<OrbiterManager> others = new List<OrbiterManager>();
                foreach (OrbiterManager o in all)
                {
                    if (o != this) others.Add(o);
                }
                if (others.Count > 0)
                {
                    OrbiterManager next = others[Random.Range(0, others.Count)];
                    next.t = 0f;
                    next.isActive = true;
                }
            }

            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            transform.position = Vector3.Lerp(startPosition, endPosition, smoothT);
        }
    }
}