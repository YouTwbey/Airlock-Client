using AirlockClient.Core;
using UnityEngine.UI;
using UnityEngine;
using static AirlockClient.Data.Info;
using Il2CppTMPro;
using AirlockAPI.Managers;
using System.Collections;
using MelonLoader;

namespace AirlockClient.Managers
{
    public class ModStamp
    {
        static GameObject uiCameraObject;
        static TextMeshProUGUI watermark;
        static Camera uiCamera;
        static GameObject canvasObject;

        public static void ApplyWatermark()
        {
            if (watermark == null)
            {
                watermark = new GameObject("Watermark").AddComponent<TextMeshProUGUI>();
                watermark.transform.SetParent(GameObject.Find("3DHUD_Canvas").transform, false);
                watermark.transform.localPosition = new Vector3(504.873f, 511.091f, 0);
                watermark.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
                watermark.enableWordWrapping = false;
                watermark.alignment = TextAlignmentOptions.Top;
                watermark.text = $"<color=yellow>AIRLOCK CLIENT (V{Version})</color>\nMod by <color=red>YouTubey</color>\nPing: {(int)NetworkManager.GetPing()} | FPS: {(int)(1 / Time.deltaTime)}";

                MelonCoroutines.Start(UpdateWatermark());
            }
        }

        public static IEnumerator UpdateWatermark()
        {
            while (watermark != null)
            {
                watermark.text = $"<color=yellow>AIRLOCK CLIENT (V{Version})</color>\nMod by <color=red>YouTubey</color>\nPing: {(int)NetworkManager.GetPing()} | FPS: {(int)(1 / Time.deltaTime)}";
                yield return new WaitForSeconds(1);
            }
        }

        public static void CreateModStamp()
        {
            int uiLayer = LayerMask.NameToLayer("UI");

            if (IsVR)
            {
                uiCameraObject = new GameObject("ModUICamera");
                uiCameraObject.transform.parent = Base.SceneStorage.transform;
                uiCamera = uiCameraObject.AddComponent<Camera>();
                uiCamera.clearFlags = CameraClearFlags.Nothing;
                uiCamera.cullingMask = 1 << uiLayer;
                uiCamera.depth = 1;
                uiCamera.orthographic = true;
                uiCamera.orthographicSize = 5;
                uiCamera.nearClipPlane = 0.1f;
                uiCamera.farClipPlane = 1.1f;

                var listener = uiCameraObject.GetComponent<AudioListener>();
                if (listener != null) GameObject.Destroy(listener);

                canvasObject = new GameObject("ModUICanvas");
                canvasObject.transform.parent = Base.SceneStorage.transform;
                canvasObject.layer = uiLayer;

                Canvas canvas = canvasObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = uiCamera;
                canvas.planeDistance = 1f;

                canvasObject.AddComponent<CanvasScaler>();
                canvasObject.AddComponent<GraphicRaycaster>();
            }

            GameObject stampObj = new GameObject("ModStamp");
            stampObj.layer = uiLayer;

            if (IsVR)
            {
                stampObj.transform.SetParent(canvasObject.transform, false);
                stampObj.transform.localPosition = new Vector3(32.4f, 32.4f, 0);
                stampObj.transform.localScale = new Vector3(5, 5, 5);

                if (Base.InGame)
                {
                    ApplyWatermark();
                }
            }
            else
            {
                if (Base.InGame)
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
            modStamp.sprite = StorageManager.ModStamp;
            modStamp.color = new Color(1, 1, 1, 0.5f);
        }
    }
}
