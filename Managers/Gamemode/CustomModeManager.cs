using Il2CppSG.Airlock;
using Il2CppSG.Airlock.UI.TitleScreen;
using UnityEngine;
using Il2CppSG.Airlock.Localization;
using System.Collections.Generic;
using AirlockClient.Managers.Debug;

namespace AirlockClient.Managers.Gamemode
{
    public class CustomModeManager : MonoBehaviour
    {
        public static CustomModeManager Instance;

        void Start()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }
        }

        public static List<string> RegisteredCustomModes = new List<string>();

        public void CreateMode(string name, string description, GameModes useGamemodeScript = GameModes.NotSet, Sprite image = null)
        {
            GamemodeSelectionMenu menu = FindObjectOfType<GamemodeSelectionMenu>(true);

            if (menu != null)
            {
                if (menu._modeInfoCollection._activeModesAndMaps.Modes.Count >= 4)
                {
                    foreach (GamemodeSelectionMenu selectMenu in FindObjectsOfType<GamemodeSelectionMenu>(true))
                    {
                        MapModeSelect[] modes = selectMenu._modeLayout.GetComponentsInChildren<MapModeSelect>(true);                      
                        int index = 0;

                        foreach (MapModeSelect mode in modes)
                        {
                            mode.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);

                            if (index == 0)
                            {
                                GameObject newSelect = Instantiate(mode.gameObject, mode.transform.parent);
                                newSelect.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
                            }

                            index++;
                        }

                        selectMenu._modeLayout._totalHeight = 30;
                        selectMenu._modeLayout.SetHeight(125);
                    }
                }


                if (RegisteredCustomModes.Contains(name + "_" + description))
                {
                    menu._modeInfoCollection._activeModesAndMaps.Modes.Add("<size=0>MODDED</size><color=yellow>" + name);
                    return;
                }

                ModeInfo newMode = new ModeInfo();

                if (useGamemodeScript == GameModes.NotSet)
                {
                    newMode.Mode = menu._modeInfoCollection.Modes[0].Mode;
                    newMode.Maps = menu._modeInfoCollection.Modes[0].Maps;
                }
                else
                {
                    ModeInfo modeScript = menu._modeInfoCollection.GetModeInfo(useGamemodeScript);
                    if (modeScript != null)
                    {
                        Logging.Debug_Log("Using " + useGamemodeScript.ToString() + " as a template...");
                        newMode.Mode = modeScript.Mode;
                        newMode.Maps = modeScript.Maps;
                    }
                    else
                    {
                        Logging.Debug_Warn(useGamemodeScript.ToString() + " returned a null ModeScript! Using default...");
                        newMode.Mode = menu._modeInfoCollection.Modes[0].Mode;
                        newMode.Maps = menu._modeInfoCollection.Modes[0].Maps;
                    }
                }

                newMode.ModeName = "<size=0>MODDED</size><color=yellow>" + name;
                newMode.ModeNameKey = new UserString();
                newMode.ModeNameKey.DefaultValue = newMode.ModeName;
                newMode.ModeNameKey.FormattedPreview = newMode.ModeName;

                newMode.ModeDescriptionKey = new UserString();
                newMode.ModeDescriptionKey.DefaultValue = description;
                newMode.ModeDescriptionKey.FormattedPreview = description;

                if (image)
                {
                    newMode.ModeSprite = image;
                }

                if (!menu._modeInfoCollection.Modes.Contains(newMode))
                {
                    menu._modeInfoCollection.Modes.Add(newMode);
                }

                menu._modeInfoCollection._activeModesAndMaps.Modes.Add(newMode.ModeName);
                RegisteredCustomModes.Add(name + "_" + description);
            }
        }
    }
}
