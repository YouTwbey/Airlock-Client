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
            if (modeSelect.ModeInfo.ModeName.StartsWith("<size=0>MODDED</size><color=yellow>"))
            {
                if (modeSelect.ModeIcon.gameObject.transform.Find("ModdedIcon") == null)
                {
                    GameObject modeIcon = Instantiate(modeSelect.ModeIcon.gameObject, modeSelect.ModeIcon.transform.parent);
                    modeSelect.ModeIcon.transform.position = new Vector3(1000, 1000, 1000);

                    Destroy(modeIcon.GetComponent<Renderer>());
                    Destroy(modeIcon.GetComponent<LUITile>());
                    Destroy(modeIcon.GetComponent<MeshFilter>());

                    Image rend = modeIcon.AddComponent<Image>();

                    if (CurrentMode.Name == "More Roles")
                    {
                        rend.sprite = StorageManager.Instance.MoreRolesIcon;
                        rend.preserveAspect = true;
                    }
                    else if (CurrentMode.Name == "Hide N Seek")
                    {
                        rend.sprite = StorageManager.Instance.HideNSeekIcon;
                        rend.preserveAspect = true;
                    }
                    else if (CurrentMode.Name == "Sandbox")
                    {
                        rend.sprite = StorageManager.Instance.FreeRoamIcon;
                        rend.preserveAspect = true;
                    }
                    else if (CurrentMode.Name == "Lights Out")
                    {
                        rend.sprite = StorageManager.Instance.LightsOutIcon;
                        rend.preserveAspect = true;
                    }
                    else if (CurrentMode.Name == "Infection")
                    {
                        rend.sprite = StorageManager.Instance.InfectedIcon;
                        rend.preserveAspect = true;
                    }
                    else if (CurrentMode.Name == "Containment")
                    {
                        rend.sprite = StorageManager.Instance.ContainmentIcon;
                        rend.preserveAspect = true;
                    }
                    else if (CurrentMode.Name == "Round Up")
                    {
                        rend.sprite = StorageManager.Instance.SheriffIcon;
                        rend.preserveAspect = true;
                    }
                    else if (CurrentMode.Name == "DeathMatch")
                    {
                        rend.sprite = StorageManager.Instance.DeathMatchIcon;
                        rend.preserveAspect = true;
                    }
                    else if (CurrentMode.Name == "Crown Runners")
                    {
                        rend.sprite = StorageManager.Instance.CrownRunnersIcon;
                        rend.preserveAspect = true;
                    }
                    else
                    {
                        rend.sprite = StorageManager.Instance.ModStamp;
                    }

                    modeIcon.name = "ModdedIcon";
                }
            }
        }
    }
}
