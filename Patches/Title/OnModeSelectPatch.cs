using AirlockAPI.Data;
using AirlockClient.Managers;
using HarmonyLib;
using SG.Airlock;
using SG.Airlock.UI.TitleScreen;
using SG.LightUI;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Object;

namespace AirlockClient.Patches
{
    [HarmonyPatch(typeof(GamemodeSelectionMenu), nameof(GamemodeSelectionMenu.OnModeSelect))]
    public class OnModeSelectPatch
    {
        public static void Prefix(GamemodeSelectionMenu __instance, MapModeSelect modeSelect)
        {
            if (!modeSelect.ModeInfo.ModeName.StartsWith("<size=0>MODDED</size><color=yellow>")) return;
            
            var modeIcon = Instantiate(modeSelect.ModeIcon.gameObject, modeSelect.ModeIcon.transform.parent);
            
            if (modeSelect.ModeIcon.gameObject.transform.Find("ModdedIcon") != null) return;
            modeSelect.ModeIcon.transform.position = new Vector3(1000, 1000, 1000);

            Destroy(modeIcon.GetComponent<Renderer>());
            Destroy(modeIcon.GetComponent<LUITile>());
            Destroy(modeIcon.GetComponent<MeshFilter>());

            var rend = modeIcon.AddComponent<Image>();

            switch (CurrentMode.Name)
            {
                case "More Roles":
                    rend.sprite = StorageManager.Instance.MoreRolesIcon;
                    rend.preserveAspect = true;
                    break;
                case "Hide N Seek":
                    rend.sprite = StorageManager.Instance.HideNSeekIcon;
                    rend.preserveAspect = true;
                    break;
                case "Sandbox":
                    rend.sprite = StorageManager.Instance.FreeRoamIcon;
                    rend.preserveAspect = true;
                    break;
                case "Lights Out":
                    rend.sprite = StorageManager.Instance.LightsOutIcon;
                    rend.preserveAspect = true;
                    break;
                case "Infection":
                    rend.sprite = StorageManager.Instance.InfectedIcon;
                    rend.preserveAspect = true;
                    break;
                case "Containment":
                    rend.sprite = StorageManager.Instance.ContainmentIcon;
                    rend.preserveAspect = true;
                    break;
                case "Round Up":
                    rend.sprite = StorageManager.Instance.SheriffIcon;
                    rend.preserveAspect = true;
                    break;
                case "DeathMatch":
                    rend.sprite = StorageManager.Instance.DeathMatchIcon;
                    rend.preserveAspect = true;
                    break;
                case "Crown Runners":
                    rend.sprite = StorageManager.Instance.CrownRunnersIcon;
                    rend.preserveAspect = true;
                    break;
                default:
                    rend.sprite = StorageManager.Instance.ModStamp;
                    break;
            }

            modeIcon.name = "ModdedIcon";
        }
    }
}
