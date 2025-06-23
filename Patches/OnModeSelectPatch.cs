using AirlockAPI.Data;
using AirlockClient.Managers;
using HarmonyLib;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.UI.TitleScreen;
using Il2CppSG.LightUI;
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
            if (modeSelect.ModeInfo.ModeName.StartsWith("<size=0>MODDED</size><color=yellow>"))
            {
                if (modeSelect.ModeIcon.gameObject.transform.Find("Icon(Clone)") == null)
                {
                    GameObject modeIcon = Instantiate(modeSelect.ModeIcon.gameObject, modeSelect.ModeIcon.transform.parent);
                    modeSelect.ModeIcon.transform.position = new Vector3(1000, 1000, 1000);

                    Destroy(modeIcon.GetComponent<Renderer>());
                    Destroy(modeIcon.GetComponent<LUITile>());
                    Destroy(modeIcon.GetComponent<MeshFilter>());

                    Image rend = modeIcon.AddComponent<Image>();

                    if (CurrentMode.Name == "More Roles")
                    {
                        rend.sprite = StorageManager.MoreRolesIcon;
                    }
                    else if (CurrentMode.Name == "Hide N Seek")
                    {
                        rend.sprite = StorageManager.HideNSeekIcon;
                    }
                    else if (CurrentMode.Name == "Sandbox")
                    {
                        rend.sprite = StorageManager.FreeRoamIcon;
                    }
                    else if (CurrentMode.Name == "Lights Out")
                    {
                        rend.sprite = StorageManager.LightsOutIcon;
                    }
                    else if (CurrentMode.Name == "Infection")
                    {
                        rend.sprite = StorageManager.InfectedIcon;
                    }
                    else if (CurrentMode.Name == "Containment")
                    {
                        rend.sprite = StorageManager.ContainmentIcon;
                    }
                    else if (CurrentMode.Name == "Round Up")
                    {
                        rend.sprite = StorageManager.SheriffIcon;
                    }
                    else
                    {
                        rend.sprite = StorageManager.ModStamp;
                    }
                }
            }
        }
    }
}
