using AirlockAPI.Managers;
using AirlockClient.Handlers;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static AirlockClient.Data.Info;


namespace AirlockClient.Managers
{
    public static class ModStamp
    {
        private static GameObject uiCameraObject;
        private static TextMeshProUGUI watermark;
        private static Camera uiCamera;
        private static GameObject canvasObject;

        private static void ApplyWatermark()
        {
            if (watermark != null || IsVR) return;
            
            watermark = new GameObject("Watermark").AddComponent<TextMeshProUGUI>();
            watermark.transform.SetParent(GameObject.Find("3DHUD_Canvas").transform, false);
            watermark.transform.localPosition = new Vector3(504.873f, 511.091f, 0);
            watermark.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
            watermark.enableWordWrapping = false;
            watermark.alignment = TextAlignmentOptions.Top;
            watermark.text = $"<color=yellow>AIRLOCK CLIENT (V{Version})</color>\nMod by <color=red>YouTubey</color>\nPing: {(int)NetworkManager.GetPing()} | FPS: {(int)(1 / Time.deltaTime)}";

            CoroutineHandler.Start(UpdateWatermark());
        }

        private static IEnumerator UpdateWatermark()
        {
            while (watermark != null)
            {
                watermark.text = $"<color=yellow>AIRLOCK CLIENT (V{Version})</color>\nMod by <color=red>YouTubey</color>\nPing: {(int)NetworkManager.GetPing()} | FPS: {(int)(1 / Time.deltaTime)}";
                yield return new WaitForSeconds(1);
            }
        }

        public static void CreateModStamp()
        {
            StorageManager.Instance.ModStamp = StorageManager.Instance.LoadModStamp("AirlockClient.Data.Sprite.ModStamp.png");
            var uiLayer = LayerMask.NameToLayer("UI");

            if (IsVR)
            {
                uiCameraObject = new GameObject("ModUICamera")
                {
                    transform =
                    {
                        parent = AirlockClientManager.SceneStorage.transform
                    }
                };
                uiCamera = uiCameraObject.AddComponent<Camera>();
                uiCamera.clearFlags = CameraClearFlags.Nothing;
                uiCamera.cullingMask = 1 << uiLayer;
                uiCamera.depth = 1;
                uiCamera.orthographic = true;
                uiCamera.orthographicSize = 5;
                uiCamera.nearClipPlane = 0.1f;
                uiCamera.farClipPlane = 1.1f;

                var listener = uiCameraObject.GetComponent<AudioListener>();
                if (listener != null) Object.Destroy(listener);

                canvasObject = new GameObject("ModUICanvas")
                {
                    transform =
                    {
                        parent = AirlockClientManager.SceneStorage.transform
                    },
                    layer = uiLayer
                };

                var canvas = canvasObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = uiCamera;
                canvas.planeDistance = 1f;

                canvasObject.AddComponent<CanvasScaler>();
                canvasObject.AddComponent<GraphicRaycaster>();
            }

            var stampObj = new GameObject("ModStamp")
            {
                layer = uiLayer
            };

            if (IsVR)
            {
                stampObj.transform.SetParent(canvasObject.transform, false);
                stampObj.transform.localPosition = new Vector3(32.4f, 32.4f, 0);
                stampObj.transform.localScale = new Vector3(5, 5, 5);

                if (AirlockClientManager.InGame)
                {
                    ApplyWatermark();
                }
            }
            else
            {
                if (AirlockClientManager.InGame)
                {
                    ApplyWatermark();
                    stampObj.transform.SetParent(GameObject.Find("3DHUD_Canvas").transform, false);
                    stampObj.transform.localPosition = new Vector3(869.7786f, 356.62f, 0);
                    stampObj.transform.localScale = new Vector3(100, 100, 100);
                }
                else
                {
                    stampObj.transform.SetParent(GameObject.Find("UI").transform, false);
                    stampObj.transform.localPosition = new Vector3(572.7276f, 303.1816f, 0);
                    stampObj.transform.localScale = new Vector3(60, 60, 60);
                }
            }

            SpriteRenderer modStamp = stampObj.AddComponent<SpriteRenderer>();
            modStamp.sprite = StorageManager.Instance.ModStamp;
            modStamp.color = new Color(1, 1, 1, 0.5f);
        }
    }
}
