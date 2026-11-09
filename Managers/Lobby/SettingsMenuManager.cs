using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AirlockAPI.Data;
using AirlockClient.Attributes;
using AirlockClient.Data.Roles.MoreRoles.Modifiers;
using AirlockClient.Managers.Debug;
using AirlockClient.Managers.Gamemode;
using SG.Airlock;
using SG.Airlock.Localization;
using SG.Airlock.Roles;
using SG.Airlock.Settings;
using SG.Airlock.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using AirlockClient.Utils;
using SG.Airlock.Minigames;
using SG.Airlock.XR;
using TMPro;
using UnityEngine.Events;
using IntVariable = SG.GlobalEvents.Variables.IntVariable;
using Object = UnityEngine.Object;

namespace AirlockClient.Managers.Lobby
{
    public partial class AdvancedSettingDefinition(string label, Func<SubRoleData, int> getValue, Action<string, int> changeValue, int step = 5)
    {
        public readonly string Label = label;
        public readonly Func<SubRoleData, int> GetValue = getValue;
        public readonly Action<string, int> ChangeValue = changeValue;
        public readonly int Step = Math.Abs(step);
    }

    public static class AdvancedSettingRegistry
    {
        public static readonly List<AdvancedSettingDefinition> DefaultDefinitions =
        [
            new AdvancedSettingDefinition(
                "Chance",
                data => data.Chance,
                (roleKey, delta) => MoreRolesManager.ChangeChance(roleKey, delta))
        ];

        private static readonly Dictionary<string, List<AdvancedSettingDefinition>> _perRoleDefinitions = new();
    }

    public sealed class ModifierAdvancedSettingDefinition(string label, Func<ModifierData, int> getValue, Action<string, int> changeValue, int step = 5)
    {
        public readonly string Label = label;
        public readonly Func<ModifierData, int> GetValue = getValue;
        public readonly Action<string, int> ChangeValue = changeValue;
        public readonly int Step = Math.Abs(step);
    }

    public static class ModifierAdvancedSettingRegistry
    {
        public static readonly List<ModifierAdvancedSettingDefinition> DefaultDefinitions =
        [
            new ModifierAdvancedSettingDefinition(
                "Chance",
                data => data.Chance,
                (modifierKey, delta) => MoreRolesManager.ChangeModifierChance(modifierKey, delta))
        ];

        private static readonly Dictionary<string, List<ModifierAdvancedSettingDefinition>> _definitions =
            new(StringComparer.OrdinalIgnoreCase);

        public static void Register(string modifierName, params ModifierAdvancedSettingDefinition[] definitions)
        {
            if (string.IsNullOrWhiteSpace(modifierName))
                throw new ArgumentException("A modifier name is required.", nameof(modifierName));

            _definitions[modifierName] = definitions?.Where(definition => definition != null).ToList() ?? new List<ModifierAdvancedSettingDefinition>();
        }

        public static bool TryGetDefinitions(string modifierName, out List<ModifierAdvancedSettingDefinition> definitions)
        {
            if (string.IsNullOrWhiteSpace(modifierName))
            {
                definitions = null;
                return false;
            }

            definitions = new List<ModifierAdvancedSettingDefinition>(DefaultDefinitions);

            if (_definitions.TryGetValue(modifierName, out var custom) && custom != null)
            {
                definitions.AddRange(custom.Where(definition =>
                    !DefaultDefinitions.Any(defaultDefinition =>
                        string.Equals(defaultDefinition.Label, definition.Label, StringComparison.OrdinalIgnoreCase))));
            }

            return true;
        }
    }

    public static class SettingsMenuManager
    {
        private const int SettingsPerPage = 5;
        private const int MaxRoleAmount = 10;
        private const int AdvancedSlotsPerRole = 4;
        
        private const int MaxModifierAmount = 10;
        private const int AdvancedSlotsPerModifier = 4;

        private const string CustomRolePagePrefix = "CustomRolePage_";

        private const string SkeldSettingsPath = "-------- UI OBJECTS --------/UI_LobbyScreen/LobbyScreenParent/Match Customization/UI_MatchCustomization";
        private const string PolusPointSettingsPath = "P_Office_01/PortalCulling_Exclude/UI_LobbyScreen/LobbyScreenParent/Match Customization/UI_MatchCustomization";
        private const string MessHallSettingsPath = "P_Dropship_ModMap02/PortalCulling_Exclude/SpawnRoomEssentials/UI_LobbyScreen/LobbyScreenParent/Match Customization/UI_MatchCustomization";

        private const string _3DSettingsPath =
            "-------- VR MANAGEMENT --------/XRRig_Gameplay/UI/3DHUD_Canvas/3DHUD_Frame/LowerCenterParent/UI_LobbyScreen_3D/LobbyScreenParent/Match Customization/UI_MatchCustomization";

        private static string SettingsRootPath
        {
            get
            {
                string sceneName = SceneManager.GetActiveScene().name;
                bool isVR = Data.Info.IsVR;

                if (!isVR)
                {
                    return sceneName switch
                    {
                        "Skeld" => _3DSettingsPath,
                        "PolusPoint" => _3DSettingsPath,
                        "MessHall" => _3DSettingsPath,
                        _ => null
                    };
                }

                return sceneName switch
                {
                    "Skeld" => SkeldSettingsPath,
                    "PolusPoint" => PolusPointSettingsPath,
                    "MessHall" => MessHallSettingsPath,
                    _ => null
                };
            }
        }

        private static string RolesSettingsPath => SettingsRootPath == null ? null : $"{SettingsRootPath}/Roles Settings";
        private static string PageOnePath => RolesSettingsPath == null ? null : $"{RolesSettingsPath}/Page One";
        private static string PageTwoPath => RolesSettingsPath == null ? null : $"{RolesSettingsPath}/Page Two";
        private static string AdvancedSettingsPath => SettingsRootPath == null ? null : $"{SettingsRootPath}/Roles Advanced";
        private static string PageSevenAdvancedPath => AdvancedSettingsPath == null ? null : $"{AdvancedSettingsPath}/Page Seven";
        public static string pathToEngineer => PageOnePath == null ? null : $"{PageOnePath}/UI_MatchOption_Engineer";
        public static string pathToVigi => PageOnePath == null ? null : $"{PageOnePath}/UI_MatchOption_Vigilante";
        public static string pathToTracker => PageOnePath == null ? null : $"{PageOnePath}/UI_MatchOption_Tracker";
        public static string pathToNumofVIPs => PageOnePath == null ? null : $"{PageOnePath}/UI_MatchOption_NumOfVIPs";
        public static string pathToGA => PageOnePath == null ? null : $"{PageOnePath}/UI_MatchOption_GuardianAngel";
        public static string pathToImpostor => PageTwoPath == null ? null : $"{PageTwoPath}/UI_MatchOption_Impostors";
        public static string pathToSheriff => PageTwoPath == null ? null : $"{PageTwoPath}/UI_MatchOption_Deputy";
        public static string pathToRevenger => PageTwoPath == null ? null : $"{PageTwoPath}/UI_MatchOption_Wraith";
        private static string pathToPage7 => PageSevenAdvancedPath;
        public static string pathToWraithAdvanced1 => PageSevenAdvancedPath == null ? null : $"{PageSevenAdvancedPath}/UI_MatchOption_RevengerSelfKillCooldown";
        public static string pathToWraithAdvanced2 => PageSevenAdvancedPath == null ? null : $"{PageSevenAdvancedPath}/UI_MatchOption_RevengerKillCooldown";
        public static string pathToWraithAdvanced3 => PageSevenAdvancedPath == null ? null : $"{PageSevenAdvancedPath}/UI_MatchOption_RevengerNumOfKills";
        public static string pathToWraithAdvanced4 => PageSevenAdvancedPath == null ? null : $"{PageSevenAdvancedPath}/UI_MatchOption_RevengerAudioDelay";

        private static readonly string[] TemplatePageNames = [
            "Page One",
            "Page Two",
            "Page Three",
            "Page Four",
            "Page Five",
            "Page Six",
            "Page Seven",
            "Page Eight"
        ];
        
        private static readonly List<GameObject> _spawnedPages = new();
        private static readonly List<GameObject> _spawnedModifierPages = new();
        private static readonly Dictionary<GameObject, SubRoleData> _slotRoleMap = new();
        private static readonly Dictionary<GameObject, ModifierData> _slotModifierMap = new();
        private static readonly List<GameObject> _spawnedAdvancedPages = new();
        private static readonly List<GameObject> _spawnedModifierAdvancedPages = new();
        private static GameObject _modifierSettingsRoot;
        private static GameObject _modifierAdvancedRoot;
        private static GameObject _modifierTabButton;
        private static MinigameButton _modifierLeftArrow;
        private static MinigameButton _modifierRightArrow;
        private static bool _modifierNavigationHooked;

        private static readonly HashSet<MinigameButton> _hookedTabButtons = new();
        private static MinigameButton _hookedLeftArrow;
        private static MinigameButton _hookedRightArrow;

        private static int _modifierCurrentPageIndex;
        private static readonly Dictionary<GameObject, SubRoleData> _advancedSlotRoleMap = new();
        private static readonly Dictionary<GameObject, ModifierData> _advancedSlotModifierMap = new();
        private static readonly Dictionary<SubRoleData, (TextKey NameKey, TextKey DescKey, IntVariable Variable)> _roleAssetCache = new();
        private static readonly Dictionary<ModifierData, (TextKey NameKey, TextKey DescKey, IntVariable Variable)> _modifierAssetCache = new();
        private static readonly Dictionary<GameObject, int> _slotCurrentAmount = new();
        private static readonly Dictionary<GameObject, int> _slotModifierCurrentAmount = new();

        private static GameObject FindPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            var slash = path.IndexOf('/');
            var rootName = slash < 0 ? path : path.Substring(0, slash);
            var rest = slash < 0 ? null : path.Substring(slash + 1);

            for (var s = 0; s < SceneManager.sceneCount; s++)
            {
                var scene = SceneManager.GetSceneAt(s);

                if (!scene.isLoaded)
                    continue;

                foreach (var root in scene.GetRootGameObjects())
                {
                    if (root == null || root.name != rootName)
                        continue;

                    if (rest == null)
                        return root;

                    var child = root.transform.Find(rest);

                    if (child != null)
                        return child.gameObject;
                }
            }

            return GameObject.Find(path);
        }

        private static void RemoveClonedChildren(Transform parent, SingleActiveGroup activeGroup, Func<GameObject, bool> shouldRemove)
        {
            if (parent == null)
                return;

            var toRemove = new List<GameObject>();

            for (var i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i).gameObject;

                if (shouldRemove(child))
                    toRemove.Add(child);
            }

            foreach (var child in toRemove)
            {
                activeGroup?._activeGroup.Remove(child);
                Object.DestroyImmediate(child);
            }
        }
        
        private static void ClearAllButtonListeners(MinigameButton button)
        {
            if (button == null || button.OnButtonPressed == null)
                return;

            var buttonEvent = button.OnButtonPressed;
            buttonEvent.RemoveAllListeners();

            var persistentCallsField = typeof(UnityEventBase).GetField(
                "m_PersistentCalls",
                BindingFlags.Instance | BindingFlags.NonPublic);

            var persistentCalls = persistentCallsField?.GetValue(buttonEvent);
            if (persistentCalls != null)
            {
                var clearMethod = persistentCalls.GetType().GetMethod(
                    "Clear",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                clearMethod?.Invoke(persistentCalls, null);
            }

            var dirtyMethod = typeof(UnityEventBase).GetMethod(
                "DirtyPersistentCalls",
                BindingFlags.Instance | BindingFlags.NonPublic);

            dirtyMethod?.Invoke(buttonEvent, null);
        }

        private static void BuildSettingsPages()
        {
            if (string.IsNullOrEmpty(RolesSettingsPath))
            {
                Logging.Error($"[SettingsMenuManager] Unsupported scene '{SceneManager.GetActiveScene().name}'.");
                return;
            }

            var pageOneObj = FindPath(PageOnePath);

            if (pageOneObj == null)
            {
                Logging.Error("[SettingsMenuManager] Could not find Page One to clone.");
                return;
            }

            var parent = pageOneObj.transform.parent;
            var singleActiveGroup = parent.GetComponent<SingleActiveGroup>();

            if (singleActiveGroup == null)
            {
                Logging.Error("[SettingsMenuManager] Could not find SingleActiveGroup on Roles Settings.");
                return;
            }

            foreach (var page in _spawnedPages.Where(page => page != null))
            {
                singleActiveGroup._activeGroup.Remove(page);
                Object.Destroy(page);
            }

            _spawnedPages.Clear();
            _slotRoleMap.Clear();
            _slotCurrentAmount.Clear();

            var registeredRoles = MoreRolesManager.SubRoleToData.Values.Where(role => role.ShowInSettings).ToList();

            var totalRoles = registeredRoles.Count;
            var totalPages = Mathf.CeilToInt(totalRoles / (float)SettingsPerPage);

            for (var pageIndex = 0; pageIndex < totalPages; pageIndex++)
            {
                var newPage = Object.Instantiate(pageOneObj, parent);
                newPage.name = $"{CustomRolePagePrefix}{pageIndex + 1}";
                _spawnedPages.Add(newPage);

                if (!singleActiveGroup._activeGroup.Contains(newPage)) singleActiveGroup._activeGroup.Add(newPage);

                var slots = new List<Transform>();

                for (var i = 0; i < newPage.transform.childCount; i++) slots.Add(newPage.transform.GetChild(i));

                for (var slotIndex = 0; slotIndex < SettingsPerPage; slotIndex++)
                {
                    var roleIndex =
                        pageIndex * SettingsPerPage + slotIndex;

                    if (slotIndex >= slots.Count)
                        break;

                    var slot = slots[slotIndex];

                    if (roleIndex >= totalRoles)
                    {
                        slot.gameObject.SetActive(false);
                        continue;
                    }

                    var data = registeredRoles[roleIndex];

                    ApplySettingsItem(slot, data);

                    _slotRoleMap[slot.gameObject] = data;
                    slot.gameObject.SetActive(true);
                }

                newPage.SetActive(false);
            }

            HookAllRoleAdvancedButtons();

            Logging.Log($"[SettingsMenuManager] Built {totalPages} role settings page(s) for {totalRoles} role(s).");
        }

        private static void HookAllRoleAdvancedButtons()
        {
            HookVanillaAdvancedButton(pathToEngineer, "Engineer", "Page One");
            HookVanillaAdvancedButton(pathToVigi, "Vigilante", "Page Two");
            HookVanillaAdvancedButton(pathToGA, "Guardian Angel", "Page Three");
            HookVanillaAdvancedButton(pathToTracker, "Tracker", "Page Four");
            HookVanillaAdvancedButton(pathToNumofVIPs, "Scanner", "Page Five");
            HookVanillaAdvancedButton(pathToImpostor, "Impostor", "Page Six");
            HookVanillaAdvancedButton(pathToRevenger, "Wraith", "Page Seven");
            HookVanillaAdvancedButton(pathToSheriff, "Deputy", "Page Eight");
        }
        
        private static void HookVanillaAdvancedButton(string slotPath, string roleName, string advancedPageName)
        {
            if (string.IsNullOrEmpty(slotPath))
                return;

            var slot = FindPath(slotPath);

            if (slot == null)
            {
                Logging.Warn($"[SettingsMenuManager] Could not find vanilla slot '{slotPath}' for '{roleName}'.");
                return;
            }

            var advanced = slot.transform.Find("Advanced");

            if (advanced == null)
            {
                Logging.Warn($"[SettingsMenuManager] Could not find Advanced button under '{slotPath}'.");
                return;
            }

            var button = advanced.GetComponent<MinigameButton>();

            if (button == null)
            {
                Logging.Warn($"[SettingsMenuManager] Advanced object for '{roleName}' has no MinigameButton.");
                return;
            }

            ClearAllButtonListeners(button);

            var capturedRoleName = roleName;
            var capturedPageName = advancedPageName;

            button.OnButtonPressed.AddListener((UnityAction<XRHand>)(hand =>
            {
                OpenVanillaAdvancedPage(capturedRoleName, capturedPageName);
            }));

            Logging.Log($"[SettingsMenuManager] Manually hooked vanilla Advanced button: {roleName} -> {advancedPageName}");
        }

        private static void OpenAdvancedForRole(SubRoleData data)
        {
            if (data == null)
                return;

            var isVanillaRole = !data.isCustomRole;

            Logging.Log($"[SettingsMenuManager][DEBUG] OpenAdvancedForRole '{data.Name}' — IsCustomRole={data.isCustomRole}, isVanillaRole={isVanillaRole}");

            if (isVanillaRole)
            {
                var vanillaPage = FindVanillaAdvancedPage(data.Name);

                Logging.Log($"[SettingsMenuManager][DEBUG] FindVanillaAdvancedPage('{data.Name}') returned: {(vanillaPage ?? "NULL")}");

                if (vanillaPage == null)
                {
                    Logging.Warn($"[SettingsMenuManager] Vanilla role '{data.Name}' has no vanilla advanced page.");
                    return;
                }

                OpenVanillaAdvancedPage(data.Name, vanillaPage);

                return;
            }

            var customPage = _spawnedAdvancedPages.FirstOrDefault(page => page != null && string.Equals(page.name, data.Name, StringComparison.OrdinalIgnoreCase));

            if (customPage != null)
            {
                OpenCustomAdvancedPage(data, customPage);

                return;
            }

            Logging.Warn($"[SettingsMenuManager] Custom role '{data.Name}' has no built advanced page.");
        }
        
        private static void OpenAdvancedForModifier(ModifierData data)
        {
            if (data == null)
                return;

            if (_modifierAdvancedRoot == null)
            {
                Logging.Error($"[SettingsMenuManager] Cannot open advanced settings for modifier '{data.Name}': Modifiers Advanced root has not been built.");
                return;
            }

            var customPage = _spawnedModifierAdvancedPages.FirstOrDefault(page =>
                page != null && page.transform.parent == _modifierAdvancedRoot.transform &&
                string.Equals(page.name, data.Name, StringComparison.OrdinalIgnoreCase));

            if (customPage == null)
            {
                Logging.Warn($"[SettingsMenuManager] Modifier '{data.Name}' has no built page under Modifiers Advanced.");
                return;
            }

            OpenCustomModifierAdvancedPage(data, customPage);
        }

        private static string FindVanillaAdvancedPage(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName) || string.IsNullOrEmpty(AdvancedSettingsPath)) return null;
            
            var advancedRoot = FindPath(AdvancedSettingsPath);

            if (advancedRoot == null)
            {
                Logging.Warn("[SettingsMenuManager][DEBUG] FindVanillaAdvancedPage: could not find AdvancedSettingsPath root.");
                return null;
            }

            foreach (var pageName in TemplatePageNames)
            {
                var page = advancedRoot.transform.Find(pageName);

                if (page == null) continue;

                var optionTexts = page.GetComponentsInChildren<TextMeshPro>(true);

                var foundTexts = string.Join(", ", optionTexts.Select(t => t.text?.Trim()).Where(t => !string.IsNullOrEmpty(t)));

                Logging.Log($"[SettingsMenuManager][DEBUG] {pageName} texts: [{foundTexts}]");

                if (optionTexts.Any(optionText => optionText != null && string.Equals(optionText.text?.Trim(), roleName.Trim(), StringComparison.OrdinalIgnoreCase)))
                {
                    return pageName;
                }
            }

            return null;
        }

        private static void OpenVanillaAdvancedPage(string roleName, string pageName)
        {
            if (string.IsNullOrEmpty(AdvancedSettingsPath))
                return;

            var advancedRoot = FindPath(AdvancedSettingsPath);

            if (advancedRoot == null)
            {
                Logging.Error($"[SettingsMenuManager] Could not find Roles Advanced for '{roleName}'.");
                return;
            }

            var targetPage = advancedRoot.transform.Find(pageName);

            if (targetPage == null)
            {
                Logging.Error($"[SettingsMenuManager] Could not find vanilla advanced page '{pageName}' for '{roleName}'.");
                return;
            }

            if (_modifierAdvancedRoot != null)
                _modifierAdvancedRoot.SetActive(false);

            advancedRoot.SetActive(true);

            var targetObject = targetPage.gameObject;
            var activeGroup = advancedRoot.GetComponent<SingleActiveGroup>();

            foreach (var templateName in TemplatePageNames)
            {
                var page = advancedRoot.transform.Find(templateName);

                if (page == null)
                    continue;

                if (activeGroup != null && !activeGroup._activeGroup.Contains(page.gameObject))
                    activeGroup._activeGroup.Add(page.gameObject);

                page.gameObject.SetActive(page.gameObject == targetObject);
            }

            foreach (var customPage in _spawnedAdvancedPages.Where(page => page != null))
            {
                customPage.SetActive(false);

                if (activeGroup != null && activeGroup._activeGroup.Contains(customPage))
                    activeGroup._activeGroup.Remove(customPage);
            }

            targetObject.SetActive(true);

            Logging.Log($"[SettingsMenuManager] Opened vanilla advanced page '{pageName}' for '{roleName}'.");
        }

        private static void OpenCustomAdvancedPage(SubRoleData data, GameObject targetPage)
        {
            if (targetPage == null)
                return;

            var parent = targetPage.transform.parent;
            var activeGroup = parent != null ? parent.GetComponent<SingleActiveGroup>() : null;

            if (_modifierAdvancedRoot != null)
                _modifierAdvancedRoot.SetActive(false);

            parent?.gameObject.SetActive(true);

            foreach (var page in _spawnedAdvancedPages.Where(page => page != null))
            {
                if (activeGroup != null && !activeGroup._activeGroup.Contains(page))
                    activeGroup._activeGroup.Add(page);

                page.SetActive(page == targetPage);
            }

            foreach (var pageName in TemplatePageNames)
            {
                var template = parent?.Find(pageName);

                if (template == null)
                    continue;

                template.gameObject.SetActive(false);

                if (activeGroup != null && activeGroup._activeGroup.Contains(template.gameObject))
                    activeGroup._activeGroup.Remove(template.gameObject);
            }

            targetPage.SetActive(true);

            Logging.Log($"[SettingsMenuManager] Opened custom advanced page for '{data.Name}'.");
        }
        
        private static void OpenCustomModifierAdvancedPage(ModifierData data, GameObject targetPage)
        {
            if (targetPage == null || _modifierAdvancedRoot == null)
                return;

            var parent = targetPage.transform.parent;
            if (parent != _modifierAdvancedRoot.transform)
            {
                Logging.Error($"[SettingsMenuManager] Advanced page '{targetPage.name}' is not a child of Modifiers Advanced.");
                return;
            }

            var rolesAdvancedRoot = FindPath(AdvancedSettingsPath);
            if (rolesAdvancedRoot != null)
                rolesAdvancedRoot.SetActive(false);

            if (_modifierSettingsRoot != null)
                _modifierSettingsRoot.SetActive(false);

            _modifierAdvancedRoot.SetActive(true);

            var activeGroup = _modifierAdvancedRoot.GetComponent<SingleActiveGroup>();

            foreach (var page in _spawnedModifierAdvancedPages.Where(page => page != null))
            {
                if (activeGroup != null && !activeGroup._activeGroup.Contains(page))
                    activeGroup._activeGroup.Add(page);

                page.SetActive(page == targetPage);
            }

            foreach (var pageName in TemplatePageNames)
            {
                var template = _modifierAdvancedRoot.transform.Find(pageName);
                if (template != null)
                    template.gameObject.SetActive(false);
            }

            targetPage.SetActive(true);

            Logging.Log($"[SettingsMenuManager] Opened Modifiers Advanced page '{targetPage.name}' for '{data.Name}' (root active={_modifierAdvancedRoot.activeSelf}).");
        }

        private static void BuildModifierTabAndNavigation()
        {
            if (string.IsNullOrEmpty(SettingsRootPath))
            {
                Logging.Error("[SettingsMenuManager] Cannot create Modifier tab: SettingsRootPath is null.");
                return;
            }

            string tabButtonsPath = $"{SettingsRootPath}/Tab Buttons";
            string sourceTabPath = $"{tabButtonsPath}/UI_RoleSelectionTab";

            var tabButtons = FindPath(tabButtonsPath);
            var sourceTab = FindPath(sourceTabPath);

            if (tabButtons == null)
            {
                Logging.Error($"[SettingsMenuManager] Tab Buttons parent not found: '{tabButtonsPath}'.");
                return;
            }

            if (sourceTab == null)
            {
                Logging.Error($"[SettingsMenuManager] Role tab template not found: '{sourceTabPath}'. " + $"IsVR={Data.Info.IsVR}");
                return;
            }

            if (_modifierTabButton == null ||
                _modifierTabButton.transform.parent != sourceTab.transform.parent)
            {
                if (_modifierTabButton != null)
                    Object.Destroy(_modifierTabButton);

                _modifierTabButton = Object.Instantiate(sourceTab, sourceTab.transform.parent, false);

                _modifierTabButton.name = "UI_ModifierSelectionTab";

                _modifierTabButton.transform.SetSiblingIndex(
                    sourceTab.transform.GetSiblingIndex() + 1);

                _modifierTabButton.transform.localPosition = new Vector3(0.278f, -1.2842f, 0f);
                var buttontile = _modifierTabButton.transform.FindChild("ButtonTile").gameObject;
                
                Object.DestroyImmediate(buttontile.GetComponent<MeshRenderer>());
                Object.DestroyImmediate(buttontile.GetComponent<MeshFilter>());
                var sprite = buttontile.AddComponent<SpriteRenderer>();
                sprite.sprite = StorageManager.Instance.ModifiersButton;
                buttontile.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);

                Logging.Log("[SettingsMenuManager] Cloned UI_RoleSelectionTab as UI_ModifierSelectionTab.");
            }

            _hookedTabButtons.RemoveWhere(button => button == null);

            var hideModifierMenus = (UnityAction<XRHand>)(_ =>
            {
                if (_modifierSettingsRoot != null)
                    _modifierSettingsRoot.SetActive(false);

                if (_modifierAdvancedRoot != null)
                    _modifierAdvancedRoot.SetActive(false);
            });

            for (var tabIndex = 0; tabIndex < tabButtons.transform.childCount; tabIndex++)
            {
                var tab = tabButtons.transform.GetChild(tabIndex);

                if (tab == null || tab.gameObject == _modifierTabButton)
                    continue;

                var otherTabButton = tab.GetComponentInChildren<MinigameButton>(true);

                if (otherTabButton == null || !_hookedTabButtons.Add(otherTabButton))
                    continue;

                otherTabButton.OnButtonPressed.AddListener(hideModifierMenus);

                Logging.Log($"[SettingsMenuManager] Hooked tab '{tab.name}' to hide the Modifiers menus.");
            }

            var modifierTabButton = _modifierTabButton.GetComponentInChildren<MinigameButton>(true);

            if (modifierTabButton == null)
            {
                Logging.Error("[SettingsMenuManager] Cloned Modifier tab has no MinigameButton.");
                return;
            }

            ClearAllButtonListeners(modifierTabButton);

            modifierTabButton.OnButtonPressed.AddListener((UnityAction<XRHand>)(_ => OpenModifierSettingsTab()));

            var tabTexts = _modifierTabButton.GetComponentsInChildren<TextMeshPro>(true);

            if (tabTexts.Length > 0)
                tabTexts[0].text = "Modifiers";

            _modifierTabButton.SetActive(true);

            string leftPath = $"{SettingsRootPath}/Page Buttons/Left Arrow Button";
            string rightPath = $"{SettingsRootPath}/Page Buttons/Right Arrow Button";

            var leftObject = FindPath(leftPath);
            var rightObject = FindPath(rightPath);

            _modifierLeftArrow = leftObject != null
                ? leftObject.GetComponent<MinigameButton>()
                : null;

            _modifierRightArrow = rightObject != null
                ? rightObject.GetComponent<MinigameButton>()
                : null;

            if (_modifierLeftArrow != null)
            {
                if (_modifierLeftArrow != _hookedLeftArrow)
                {
                    _modifierLeftArrow.OnButtonPressed.AddListener((UnityAction<XRHand>)(_ =>
                    {
                        if (_modifierSettingsRoot != null && _modifierSettingsRoot.activeSelf)
                            CycleModifierSettingsPage(-1);
                    }));

                    _hookedLeftArrow = _modifierLeftArrow;
                }
            }
            else
            {
                Logging.Error($"[SettingsMenuManager] Modifier left arrow not found: '{leftPath}'.");
            }

            if (_modifierRightArrow != null)
            {
                if (_modifierRightArrow != _hookedRightArrow)
                {
                    _modifierRightArrow.OnButtonPressed.AddListener((UnityAction<XRHand>)(_ =>
                    {
                        if (_modifierSettingsRoot != null && _modifierSettingsRoot.activeSelf)
                            CycleModifierSettingsPage(1);
                    }));

                    _hookedRightArrow = _modifierRightArrow;
                }
            }
            else
            {
                Logging.Error($"[SettingsMenuManager] Modifier right arrow not found: '{rightPath}'.");
            }

            _modifierNavigationHooked = _modifierLeftArrow != null || _modifierRightArrow != null;

            Logging.Log(
                $"[SettingsMenuManager] Modifier tab ready. " +
                $"VR={Data.Info.IsVR}, Tab={_modifierTabButton.name}, " +
                $"Pages={_spawnedModifierPages.Count}, " +
                $"LeftArrow={_modifierLeftArrow != null}, " +
                $"RightArrow={_modifierRightArrow != null}.");
        }

        private static void OpenModifierSettingsTab()
        {
            if (_modifierSettingsRoot == null)
            {
                Logging.Error("[SettingsMenuManager] Cannot open Modifiers Settings: root has not been built.");
                return;
            }

            var rolesRoot = FindPath(RolesSettingsPath);
            if (rolesRoot != null)
                rolesRoot.SetActive(false);

            if (_modifierAdvancedRoot != null)
                _modifierAdvancedRoot.SetActive(false);

            _modifierSettingsRoot.SetActive(true);
            _modifierCurrentPageIndex = 0;
            SetModifierSettingsPage(_modifierCurrentPageIndex);
        }

        private static void CycleModifierSettingsPage(int direction)
        {
            var pageCount = _spawnedModifierPages.Count(page => page != null);
            if (pageCount <= 1)
                return;

            _modifierCurrentPageIndex = (_modifierCurrentPageIndex + direction + pageCount) % pageCount;
            SetModifierSettingsPage(_modifierCurrentPageIndex);
        }

        private static void SetModifierSettingsPage(int pageIndex)
        {
            var pages = _spawnedModifierPages.Where(page => page != null).ToList();
            if (pages.Count == 0)
                return;

            _modifierCurrentPageIndex = Mathf.Clamp(pageIndex, 0, pages.Count - 1);
            var activeGroup = _modifierSettingsRoot != null
                ? _modifierSettingsRoot.GetComponent<SingleActiveGroup>()
                : null;

            for (var i = 0; i < pages.Count; i++)
            {
                var page = pages[i];
                if (activeGroup != null && !activeGroup._activeGroup.Contains(page))
                    activeGroup._activeGroup.Add(page);
                page.SetActive(i == _modifierCurrentPageIndex);
            }
        }

        private static void DisableTemplatePages(Transform parent, SingleActiveGroup singleActiveGroup)
        {
            if (parent == null)
                return;

            foreach (var pageName in TemplatePageNames)
            {
                var pageTransform = parent.Find(pageName);

                if (pageTransform == null)
                    continue;

                var page = pageTransform.gameObject;
                page.SetActive(false);

                if (singleActiveGroup != null && singleActiveGroup._activeGroup.Contains(page))
                    singleActiveGroup._activeGroup.Remove(page);
            }
        }

        private static GameObject CloneRootObject(GameObject source, string cloneName, ref GameObject currentClone)
        {
            if (source == null)
            {
                Logging.Error($"[SettingsMenuManager] Cannot clone '{cloneName}': source object is null.");
                return null;
            }

            var sourceParent = source.transform.parent;
            if (currentClone != null && currentClone.transform.parent == sourceParent)
            {
                currentClone.name = cloneName;
                return currentClone;
            }

            if (currentClone != null)
                Object.Destroy(currentClone);

            currentClone = Object.Instantiate(source, sourceParent, false);
            currentClone.name = cloneName;
            currentClone.transform.SetSiblingIndex(source.transform.GetSiblingIndex() + 1);

            Logging.Log($"[SettingsMenuManager] Cloned '{source.name}' as sibling '{cloneName}' under '{sourceParent?.name}'.");
            return currentClone;
        }

        private static void DestroySpawnedPages(List<GameObject> pages, SingleActiveGroup activeGroup)
        {
            foreach (var page in pages.Where(page => page != null).ToList())
            {
                activeGroup?._activeGroup.Remove(page);
                Object.Destroy(page);
            }

            pages.Clear();
        }

        private static void SetOptionText(Transform slot, string text)
        {
            var optionTextTransform =
                slot.Find("Option Text");

            if (optionTextTransform == null)
            {
                Logging.Error($"[SettingsMenuManager] Could not find 'Option Text' under slot '{slot.name}'.");
                return;
            }

            var binding =
                optionTextTransform.GetComponent<UserStringComponent_TMP>();

            if (binding != null) Object.Destroy(binding);

            var tmp = optionTextTransform.GetComponent<TextMeshPro>();

            if (tmp == null)
            {
                Logging.Error($"[SettingsMenuManager] No TextMeshPro on 'Option Text' under slot '{slot.name}'.");
                return;
            }

            tmp.text = text;
        }

        private static void ApplySettingsItem(Transform slot, SubRoleData data)
        {
            var selector =
                slot.GetComponent<UISelector>();

            if (selector == null)
            {
                Logging.Error($"[SettingsMenuManager] No UISelector on slot for '{data.Name}'.");
                slot.gameObject.SetActive(false);
                return;
            }

            if (!_roleAssetCache.TryGetValue(data, out var assets))
            {
                var templateVariable = selector._selectorValues[0].SelectorVariablesInt[0].Variable?._variable;

                if (templateVariable == null)
                {
                    Logging.Error(
                        $"[SettingsMenuManager] No template IntVariable found on slot for '{data.Name}'.");
                    return;
                }

                var sharedVariable =
                    Object.Instantiate(templateVariable);

                var nameKey = CreateTextKey();
                var descKey = CreateTextKey();

                RegisterTextKey(data.Name, nameKey);
                RegisterTextKey(data.AC_Description, descKey);

                assets = (nameKey, descKey, sharedVariable);

                _roleAssetCache[data] = assets;
            }

            foreach (var selectorValue in selector._selectorValues)
            {
                if (selectorValue?.SelectorVariablesInt == null) continue;

                for (var i = 0; i < selectorValue.SelectorVariablesInt.Length; i++)
                {
                    var existing = selectorValue.SelectorVariablesInt[i];

                    if (existing == null) continue;

                    selectorValue.SelectorVariablesInt[i] = new UISelector.SelectorVariableInt
                    {
                        Variable = new IntSettingsItem
                        {
                            _variable = assets.Variable
                        },
                        SetVariableTo = existing.SetVariableTo 
                    };
                }
            }

            var comp =
                slot.GetComponent<UIRoleDescription>();

            if (comp == null)
            {
                Logging.Error($"[SettingsMenuManager] No UIRoleDescription on slot for '{data.Name}'.");
                return;
            }

            comp._roleName = assets.NameKey;
            comp._roleDescription = assets.DescKey;
            comp._gameTeam = data.Team;

            selector.AdjustedBoolSettings.Clear();
            selector.AdjustedFloatSettings.Clear();
            selector.AdjustedIntSettings.Clear();

            selector.AdjustedIntSettings.Add(new IntSettingsItem
            { 
                _variable = assets.Variable 
            });

            SetOptionText(slot, data.Name);

            var startAmount = Mathf.Clamp(data.Amount, 0, MaxRoleAmount);
            
            _slotCurrentAmount[slot.gameObject] = startAmount;

            assets.Variable.Value = startAmount;

            var valueText = slot.Find("Selection Text")?.GetComponent<TextMeshPro>();

            var leftButton = slot.Find("Left Arrow")?.GetComponent<MinigameButton>();

            var rightButton = slot.Find("Right Arrow")?.GetComponent<MinigameButton>();

            if (leftButton != null && rightButton != null)
            {
                var roleKey = data.Name;

                leftButton.OnButtonPressed.RemoveAllListeners();
                rightButton.OnButtonPressed.RemoveAllListeners();

                leftButton.OnButtonPressed.AddListener((UnityAction<XRHand>)(hand => 
                { 
                        MoreRolesManager.Instance.ChangeRoleAmount(roleKey, -1);
                        var amount = Mathf.Clamp(data.Amount, 0, MaxRoleAmount);
                        if (valueText != null) valueText.text = amount.ToString(); 
                }));

                rightButton.OnButtonPressed.AddListener((UnityAction<XRHand>)(hand =>
                { 
                    MoreRolesManager.Instance.ChangeRoleAmount(roleKey, 1); 
                    var amount = Mathf.Clamp(data.Amount, 0, MaxRoleAmount); 
                    if (valueText != null) valueText.text = amount.ToString();
                }));
            }

            var advancedButton = slot.Find("Advanced")?.GetComponent<MinigameButton>();

            if (advancedButton == null) return;

            ClearAllButtonListeners(advancedButton);

            var capturedRole = data;

            advancedButton.OnButtonPressed.AddListener((UnityAction<XRHand>)(hand => OpenAdvancedForRole(capturedRole)));
        }
        
        private static void ApplyModifierSettingsItem(Transform slot, ModifierData data)
        {
            var selector =
                slot.GetComponent<UISelector>();

            if (selector == null)
            {
                Logging.Error($"[SettingsMenuManager] No UISelector on slot for '{data.Name}'.");
                slot.gameObject.SetActive(false);
                return;
            }

            if (!_modifierAssetCache.TryGetValue(data, out var assets))
            {
                var templateVariable = selector._selectorValues[0].SelectorVariablesInt[0].Variable?._variable;

                if (templateVariable == null)
                {
                    Logging.Error(
                        $"[SettingsMenuManager] No template IntVariable found on slot for '{data.Name}'.");
                    return;
                }

                var sharedVariable =
                    Object.Instantiate(templateVariable);

                var nameKey = CreateTextKey();
                var descKey = CreateTextKey();

                RegisterTextKey(data.Name, nameKey);
                RegisterTextKey(data.AC_Description, descKey);

                assets = (nameKey, descKey, sharedVariable);

                _modifierAssetCache[data] = assets;
            }

            foreach (var selectorValue in selector._selectorValues)
            {
                if (selectorValue?.SelectorVariablesInt == null) continue;

                for (var i = 0; i < selectorValue.SelectorVariablesInt.Length; i++)
                {
                    var existing = selectorValue.SelectorVariablesInt[i];

                    if (existing == null) continue;

                    selectorValue.SelectorVariablesInt[i] = new UISelector.SelectorVariableInt
                    {
                        Variable = new IntSettingsItem
                        {
                            _variable = assets.Variable
                        },
                        SetVariableTo = existing.SetVariableTo 
                    };
                }
            }

            var comp =
                slot.GetComponent<UIRoleDescription>();

            if (comp == null)
            {
                Logging.Error($"[SettingsMenuManager] No UIRoleDescription on slot for '{data.Name}'.");
                return;
            }

            comp._roleName = assets.NameKey;
            comp._roleDescription = assets.DescKey;
            comp._gameTeam = data.Team;

            selector.AdjustedBoolSettings.Clear();
            selector.AdjustedFloatSettings.Clear();
            selector.AdjustedIntSettings.Clear();

            selector.AdjustedIntSettings.Add(new IntSettingsItem
            { 
                _variable = assets.Variable 
            });

            SetOptionText(slot, data.Name);

            var startAmount = Mathf.Clamp(data.Amount, 0, MaxModifierAmount);
            
            _slotModifierCurrentAmount[slot.gameObject] = startAmount;

            assets.Variable.Value = startAmount;

            var valueText = slot.Find("Selection Text")?.GetComponent<TextMeshPro>();

            var leftButton = slot.Find("Left Arrow")?.GetComponent<MinigameButton>();

            var rightButton = slot.Find("Right Arrow")?.GetComponent<MinigameButton>();

            if (leftButton != null && rightButton != null)
            {
                var modifierKey = data.Name;

                leftButton.OnButtonPressed.RemoveAllListeners();
                rightButton.OnButtonPressed.RemoveAllListeners();

                leftButton.OnButtonPressed.AddListener((UnityAction<XRHand>)(hand => 
                { 
                        MoreRolesManager.Instance.ChangeModifierAmount(modifierKey, -1);
                        var amount = Mathf.Clamp(data.Amount, 0, MaxModifierAmount);
                        if (valueText != null) valueText.text = amount.ToString(); 
                }));

                rightButton.OnButtonPressed.AddListener((UnityAction<XRHand>)(hand =>
                { 
                    MoreRolesManager.Instance.ChangeModifierAmount(modifierKey, 1); 
                    var amount = Mathf.Clamp(data.Amount, 0, MaxModifierAmount); 
                    if (valueText != null) valueText.text = amount.ToString();
                }));
            }

            var advancedButton = slot.Find("Advanced")?.GetComponent<MinigameButton>();

            if (advancedButton == null) return;

            var hasAdvancedSettings = ModifierAdvancedSettingRegistry.TryGetDefinitions(data.Name, out var definitions)
                                         && definitions != null
                                         && definitions.Count > 0;

            advancedButton.gameObject.SetActive(hasAdvancedSettings);

            if (!hasAdvancedSettings) return;

            ClearAllButtonListeners(advancedButton);

            var capturedModifier = data;

            advancedButton.OnButtonPressed.AddListener((UnityAction<XRHand>)(hand => OpenAdvancedForModifier(capturedModifier)));
        }

        private static void BuildAdvancedPages()
        {
            Logging.Log("[SettingsMenuManager] BuildAdvancedPages() START");

            if (string.IsNullOrEmpty(pathToPage7))
            {
                Logging.Error($"[SettingsMenuManager] Unsupported scene '{SceneManager.GetActiveScene().name}'.");
                return;
            }

            var page7Obj = FindPath(pathToPage7);

            if (page7Obj == null)
            {
                Logging.Error($"[SettingsMenuManager] Could not find Page Seven at path: {pathToPage7}");
                return;
            }

            var parent = page7Obj.transform.parent;

            var singleActiveGroup = parent.GetComponent<SingleActiveGroup>();

            if (singleActiveGroup == null)
            {
                Logging.Error("[SettingsMenuManager] Could not find SingleActiveGroup on Roles Advanced.");
                return;
            }

            foreach (var page in _spawnedAdvancedPages.Where(page => page != null))
            {
                singleActiveGroup._activeGroup.Remove(page);
                Object.Destroy(page);
            }

            _spawnedAdvancedPages.Clear();
            _advancedSlotRoleMap.Clear();

            var rolesWithAdvancedSettings = MoreRolesManager.SubRoleToData.Values.Where(role => role.ShowInSettings && role.isCustomRole).ToList();

            var totalRoles = rolesWithAdvancedSettings.Count;

            for (var roleIndex = 0; roleIndex < totalRoles; roleIndex++)
            {
                BuildAdvancedPage(roleIndex, page7Obj, parent, singleActiveGroup, rolesWithAdvancedSettings[roleIndex]);
            }

            DisableTemplatePages(parent, singleActiveGroup);

            foreach (var page in _spawnedAdvancedPages.Where(page => page != null))
            {
                page.SetActive(false);
            }

            Logging.Log($"[SettingsMenuManager] Built {totalRoles} custom advanced page(s).");
        }
        
        private static void BuildAdvancedModifierPages()
        {
            Logging.Log("[SettingsMenuManager] BuildAdvancedModifierPages() START");

            if (string.IsNullOrEmpty(AdvancedSettingsPath))
            {
                Logging.Error($"[SettingsMenuManager] Unsupported scene '{SceneManager.GetActiveScene().name}'.");
                return;
            }

            var rolesAdvancedRoot = FindPath(AdvancedSettingsPath);

            if (rolesAdvancedRoot == null)
            {
                Logging.Error($"[SettingsMenuManager] Could not find Roles Advanced at path: {AdvancedSettingsPath}");
                return;
            }

            var modifierAdvancedRoot = CloneRootObject(
                rolesAdvancedRoot,
                "Modifiers Advanced",
                ref _modifierAdvancedRoot);

            if (modifierAdvancedRoot == null)
            {
                Logging.Error("[SettingsMenuManager] Failed to clone Modifiers Advanced root.");
                return;
            }

            var parent = modifierAdvancedRoot.transform;
            modifierAdvancedRoot.SetActive(false);
            var singleActiveGroup = modifierAdvancedRoot.GetComponent<SingleActiveGroup>();

            if (singleActiveGroup == null)
            {
                Logging.Error("[SettingsMenuManager] Could not find SingleActiveGroup on cloned Modifiers Advanced.");
                return;
            }

            var roleAdvancedPageNames = new HashSet<string>(
                _spawnedAdvancedPages.Where(page => page != null).Select(page => page.name),
                StringComparer.OrdinalIgnoreCase);

            RemoveClonedChildren(parent, singleActiveGroup, child =>
                !TemplatePageNames.Contains(child.name)
                && roleAdvancedPageNames.Contains(child.name)
                && !_spawnedModifierAdvancedPages.Contains(child));

            var pageSevenTemplate = parent.Find("Page Seven")?.gameObject;

            if (pageSevenTemplate == null)
            {
                Logging.Error("[SettingsMenuManager] Could not find cloned Page Seven template under Modifiers Advanced.");
                return;
            }

            DestroySpawnedPages(_spawnedModifierAdvancedPages, singleActiveGroup);
            _advancedSlotModifierMap.Clear();

            DisableTemplatePages(parent, singleActiveGroup);

            var modifiersWithAdvancedSettings = MoreRolesManager.ModifierToData.Values
                .Where(modifier => modifier.ShowInSettings
                                   && ModifierAdvancedSettingRegistry.TryGetDefinitions(modifier.Name, out var definitions)
                                   && definitions != null
                                   && definitions.Count > 0)
                .ToList();

            for (var modifierIndex = 0; modifierIndex < modifiersWithAdvancedSettings.Count; modifierIndex++)
            {
                BuildAdvancedModifierPage(
                    modifierIndex,
                    pageSevenTemplate,
                    parent,
                    singleActiveGroup,
                    modifiersWithAdvancedSettings[modifierIndex]);
            }

            foreach (var page in _spawnedModifierAdvancedPages.Where(page => page != null))
                page.SetActive(false);

            Logging.Log($"[SettingsMenuManager] Built {_spawnedModifierAdvancedPages.Count} modifier advanced page(s) under Modifiers Advanced.");
        }

        private static void BuildAdvancedPage(int roleIndex, GameObject page7Obj, Transform parent, SingleActiveGroup singleActiveGroup, SubRoleData data)
        {
            var newPage = Object.Instantiate(page7Obj, parent);

            newPage.name = data.Name;

            _spawnedAdvancedPages.Add(newPage);

            if (!singleActiveGroup._activeGroup.Contains(newPage)) singleActiveGroup._activeGroup.Add(newPage);

            var slots = new List<Transform>();

            for (var i = 0; i < newPage.transform.childCount; i++)
            {
                slots.Add(newPage.transform.GetChild(i));
            }

            var definitions = MoreRolesManager.SubRoleToAdvancedSettings.TryGetValue(data.Name, out var defs) ? defs : MoreRolesManager.DefaultAdvancedSettings;

            for (var slotIndex = 0; slotIndex < AdvancedSlotsPerRole && slotIndex < slots.Count; slotIndex++)
            {
                var slot = slots[slotIndex];

                if (slotIndex < definitions.Count)
                {
                    ApplyAdvancedSettingsItem(slot, data, definitions[slotIndex]);
                    _advancedSlotRoleMap[slot.gameObject] = data;
                    slot.gameObject.SetActive(true);
                }
                else
                {
                    slot.gameObject.SetActive(false);
                }
            }

            for (var slotIndex = definitions.Count; slotIndex < slots.Count; slotIndex++)
            {
                slots[slotIndex].gameObject.SetActive(false);
            }
        }

        private static void BuildAdvancedModifierPage(int modifierIndex, GameObject pageTemplate, Transform parent, SingleActiveGroup singleActiveGroup, ModifierData data)
        {
            if (!ModifierAdvancedSettingRegistry.TryGetDefinitions(data.Name, out var definitions)
                || definitions == null
                || definitions.Count == 0)
                return;

            var newPage = Object.Instantiate(pageTemplate, parent);
            newPage.name = data.Name;
            newPage.SetActive(true);
            _spawnedModifierAdvancedPages.Add(newPage);

            if (!singleActiveGroup._activeGroup.Contains(newPage))
                singleActiveGroup._activeGroup.Add(newPage);

            var slots = new List<Transform>();
            for (var i = 0; i < newPage.transform.childCount; i++)
                slots.Add(newPage.transform.GetChild(i));

            if (definitions.Count > AdvancedSlotsPerModifier)
            {
                Logging.Warn($"[SettingsMenuManager] Modifier '{data.Name}' has {definitions.Count} advanced settings; only the first {AdvancedSlotsPerModifier} can be displayed.");
            }

            for (var slotIndex = 0; slotIndex < slots.Count; slotIndex++)
            {
                var slot = slots[slotIndex];

                if (slotIndex < AdvancedSlotsPerModifier && slotIndex < definitions.Count)
                {
                    ApplyAdvancedModifierSettingsItem(slot, data, definitions[slotIndex]);
                    _advancedSlotModifierMap[slot.gameObject] = data;
                    slot.gameObject.SetActive(true);
                }
                else
                {
                    slot.gameObject.SetActive(false);
                }
            }
        }

        private static void ApplyAdvancedSettingsItem(Transform slot, SubRoleData data, AdvancedSettingDefinition definition)
        {
            var selector = slot.GetComponent<UISelector>();

            if (selector == null || selector._selectorValues == null || selector._selectorValues.Length == 0 || selector._selectorValues[0] == null || selector._selectorValues[0].SelectorVariablesInt == null || selector._selectorValues[0].SelectorVariablesInt.Length == 0)
            {
                slot.gameObject.SetActive(false);
                return;
            }

            var templateVariable = selector._selectorValues[0].SelectorVariablesInt[0].Variable?._variable;

            if (templateVariable == null)
            {
                slot.gameObject.SetActive(false);
                return;
            }

            var sharedVariable = Object.Instantiate(templateVariable);

            foreach (var selectorValue in selector._selectorValues)
            {
                if (selectorValue?.SelectorVariablesInt == null) continue;

                for (var i = 0; i < selectorValue.SelectorVariablesInt.Length; i++)
                {
                    var existing = selectorValue.SelectorVariablesInt[i];

                    if (existing == null) continue;

                    selectorValue.SelectorVariablesInt[i] = new UISelector.SelectorVariableInt
                    {
                        Variable = new IntSettingsItem { _variable = sharedVariable }, 
                        SetVariableTo = existing.SetVariableTo 
                    };
                }
            }

            selector.AdjustedBoolSettings.Clear();
            selector.AdjustedFloatSettings.Clear();
            selector.AdjustedIntSettings.Clear();

            selector.AdjustedIntSettings.Add(new IntSettingsItem { _variable = sharedVariable });

            SetOptionText(slot, definition.Label);

            var currentValue = definition.GetValue(data);

            sharedVariable.Value = currentValue;

            var valueText = slot.Find("Selection Text") ?.GetComponent<TextMeshPro>();

            if (valueText != null) valueText.text = currentValue.ToString();

            var leftButton = slot.Find("Left Arrow")?.GetComponent<MinigameButton>();

            var rightButton = slot.Find("Right Arrow")?.GetComponent<MinigameButton>();

            if (leftButton == null || rightButton == null) return;

            var roleKey = data.Name;

            leftButton.OnButtonPressed.RemoveAllListeners();
            rightButton.OnButtonPressed.RemoveAllListeners();

            leftButton.OnButtonPressed.AddListener( (UnityAction<XRHand>)(hand =>
            {
                definition.ChangeValue(roleKey, -definition.Step);

                var value = definition.GetValue(data);

                sharedVariable.Value = value;

                if (valueText != null) valueText.text = value.ToString();
            }));

            rightButton.OnButtonPressed.AddListener((UnityAction<XRHand>)(hand =>
            {
                definition.ChangeValue( roleKey, definition.Step);

                var value = definition.GetValue(data);

                sharedVariable.Value = value;

                if (valueText != null) valueText.text = value.ToString();
            }));
        }
        
        private static void ApplyAdvancedModifierSettingsItem(Transform slot, ModifierData data, ModifierAdvancedSettingDefinition definition)
        {
            var selector = slot.GetComponent<UISelector>();

            if (selector == null || selector._selectorValues == null || selector._selectorValues.Length == 0
                || selector._selectorValues[0] == null || selector._selectorValues[0].SelectorVariablesInt == null
                || selector._selectorValues[0].SelectorVariablesInt.Length == 0)
            {
                slot.gameObject.SetActive(false);
                return;
            }

            var templateVariable = selector._selectorValues[0].SelectorVariablesInt[0].Variable?._variable;
            if (templateVariable == null)
            {
                slot.gameObject.SetActive(false);
                return;
            }

            var sharedVariable = Object.Instantiate(templateVariable);

            foreach (var selectorValue in selector._selectorValues)
            {
                if (selectorValue?.SelectorVariablesInt == null) continue;

                for (var i = 0; i < selectorValue.SelectorVariablesInt.Length; i++)
                {
                    var existing = selectorValue.SelectorVariablesInt[i];
                    if (existing == null) continue;

                    selectorValue.SelectorVariablesInt[i] = new UISelector.SelectorVariableInt
                    {
                        Variable = new IntSettingsItem { _variable = sharedVariable },
                        SetVariableTo = existing.SetVariableTo
                    };
                }
            }

            selector.AdjustedBoolSettings.Clear();
            selector.AdjustedFloatSettings.Clear();
            selector.AdjustedIntSettings.Clear();
            selector.AdjustedIntSettings.Add(new IntSettingsItem { _variable = sharedVariable });

            SetOptionText(slot, definition.Label);

            var currentValue = definition.GetValue(data);
            sharedVariable.Value = currentValue;

            var valueText = slot.Find("Selection Text")?.GetComponent<TextMeshPro>();
            if (valueText != null) valueText.text = currentValue.ToString();

            var leftButton = slot.Find("Left Arrow")?.GetComponent<MinigameButton>();
            var rightButton = slot.Find("Right Arrow")?.GetComponent<MinigameButton>();
            if (leftButton == null || rightButton == null) return;

            var modifierKey = data.Name;

            leftButton.OnButtonPressed.RemoveAllListeners();
            rightButton.OnButtonPressed.RemoveAllListeners();

            leftButton.OnButtonPressed.AddListener((UnityAction<XRHand>)(hand =>
            {
                definition.ChangeValue(modifierKey, -definition.Step);
                var value = definition.GetValue(data);
                sharedVariable.Value = value;
                if (valueText != null) valueText.text = value.ToString();
            }));

            rightButton.OnButtonPressed.AddListener((UnityAction<XRHand>)(hand =>
            {
                definition.ChangeValue(modifierKey, definition.Step);
                var value = definition.GetValue(data);
                sharedVariable.Value = value;
                if (valueText != null) valueText.text = value.ToString();
            }));
        }

        public static void OnUpdate()
        {
            var rolesSettingsObj = string.IsNullOrEmpty(RolesSettingsPath)
                ? null
                : FindPath(RolesSettingsPath);
            var singleActiveGroup = rolesSettingsObj != null
                ? rolesSettingsObj.GetComponent<SingleActiveGroup>()
                : null;

            foreach (var page in _spawnedPages.Where(page => page != null))
            {
                if (singleActiveGroup != null && !singleActiveGroup._activeGroup.Contains(page))
                    singleActiveGroup._activeGroup.Add(page);

                for (var i = 0; i < page.transform.childCount; i++)
                {
                    var slot = page.transform.GetChild(i).gameObject;
                    if (!_slotRoleMap.TryGetValue(slot, out var data))
                        continue;

                    if (!_roleAssetCache.TryGetValue(data, out var assets))
                        continue;

                    var comp = slot.GetComponent<UIRoleDescription>();
                    if (comp == null) continue;

                    comp._roleDescription = assets.DescKey;
                    comp._roleName = assets.NameKey;
                    comp._gameTeam = data.Team;

                    if (comp._crewOrImpText != null)
                        comp._crewOrImpText.text = "<color=#636363>Neutral</color>";

                    var optionText = slot.transform.Find("Option Text")?.GetComponent<TextMeshPro>();
                    if (optionText != null) optionText.text = data.Name;
                }
            }

            foreach (var page in _spawnedModifierPages.Where(page => page != null))
            {
                for (var i = 0; i < page.transform.childCount; i++)
                {
                    var slot = page.transform.GetChild(i).gameObject;
                    if (!_slotModifierMap.TryGetValue(slot, out var data))
                        continue;

                    if (!_modifierAssetCache.TryGetValue(data, out var assets))
                        continue;

                    var comp = slot.GetComponent<UIRoleDescription>();
                    if (comp == null) continue;

                    comp._roleDescription = assets.DescKey;
                    comp._roleName = assets.NameKey;
                    comp._gameTeam = data.Team;

                    if (comp._crewOrImpText != null)
                        comp._crewOrImpText.text = "<color=#636363>Neutral</color>";

                    var optionText = slot.transform.Find("Option Text")?.GetComponent<TextMeshPro>();
                    if (optionText != null) optionText.text = data.Name;
                }
            }
        }

        private static TextKey CreateTextKey() => Object.Instantiate(StaticRefs.Role._availableRoles.AllRoles[3].EjectionReveal);

        private static void RegisterTextKey(string text, TextKey textKey)
        {
            var db =
                StaticRefs.Role
                    ._availableRoles
                    .AllRoles[0]
                    .EjectionReveal
                    ._localizationManager
                    ._currentTextDatabase;

            db.AddNewEntry(
                textKey,
                text);
        }

        public static IEnumerator DelayPatchSettings(int time = 3)
        {
            yield return new WaitForSeconds(time);

            const float timeout = 15f;
            float elapsed = 0f;

            while (elapsed < timeout)
            {
                bool rootsExist =
                    !string.IsNullOrEmpty(RolesSettingsPath) &&
                    FindPath(RolesSettingsPath) != null &&
                    !string.IsNullOrEmpty(AdvancedSettingsPath) &&
                    FindPath(AdvancedSettingsPath) != null;

                bool tabExists =
                    !string.IsNullOrEmpty(SettingsRootPath) &&
                    FindPath($"{SettingsRootPath}/Tab Buttons/UI_RoleSelectionTab") != null;

                if (rootsExist && tabExists)
                    break;

                elapsed += 0.25f;
                yield return new WaitForSeconds(0.25f);
            }

            Logging.Log(
                $"[SettingsMenuManager] Initializing settings. " +
                $"Scene={SceneManager.GetActiveScene().name}, " +
                $"IsVR={Data.Info.IsVR}, " +
                $"SettingsRootPath={SettingsRootPath}");

            if (string.IsNullOrEmpty(RolesSettingsPath) ||
                FindPath(RolesSettingsPath) == null)
            {
                Logging.Error(
                    $"[SettingsMenuManager] Initialization failed: Roles Settings not found at '{RolesSettingsPath}'.");
                yield break;
            }

            if (string.IsNullOrEmpty(AdvancedSettingsPath) ||
                FindPath(AdvancedSettingsPath) == null)
            {
                Logging.Error(
                    $"[SettingsMenuManager] Initialization failed: Roles Advanced not found at '{AdvancedSettingsPath}'.");
                yield break;
            }

            BuildSettingsPages();
            BuildAdvancedPages();

            CreateModifiersPages();
            BuildAdvancedModifierPages();

            Logging.Log("[SettingsMenuManager] Settings initialization finished.");
        }

        public static void CreateModifiersPages()
        {
            if (string.IsNullOrEmpty(RolesSettingsPath))
            {
                Logging.Error($"[SettingsMenuManager] Cannot build Modifiers: unsupported scene '{SceneManager.GetActiveScene().name}'."); 
                return;
            }

            var rolesSettingsRoot = FindPath(RolesSettingsPath);

            if (rolesSettingsRoot == null)
            {
                Logging.Error($"[SettingsMenuManager] Cannot build Modifiers: Roles Settings not found at '{RolesSettingsPath}'.");
                return;
            }

            var modifierSettingsRoot = CloneRootObject(rolesSettingsRoot, "Modifiers Settings", ref _modifierSettingsRoot);

            if (modifierSettingsRoot == null)
            {
                Logging.Error("[SettingsMenuManager] Failed to clone Modifiers Settings root.");
                return;
            }

            modifierSettingsRoot.SetActive(false);

            var parent = modifierSettingsRoot.transform;
            var singleActiveGroup = modifierSettingsRoot.GetComponent<SingleActiveGroup>();

            if (singleActiveGroup == null)
            {
                Logging.Error("[SettingsMenuManager] Cloned Modifiers Settings has no SingleActiveGroup.");
                return;
            }

            RemoveClonedChildren(parent, singleActiveGroup,
                child => child.name.StartsWith(CustomRolePagePrefix, StringComparison.Ordinal));

            var pageOneTemplate = parent.Find("Page One")?.gameObject;

            if (pageOneTemplate == null)
            {
                Logging.Error("[SettingsMenuManager] Cloned Modifiers Settings has no Page One template.");
                return;
            }

            DestroySpawnedPages(_spawnedModifierPages, singleActiveGroup);

            _slotModifierMap.Clear();
            _slotModifierCurrentAmount.Clear();

            DisableTemplatePages(parent, singleActiveGroup);

            var registeredModifiers = MoreRolesManager.ModifierToData.Values
                .Where(data => data != null && data.ShowInSettings)
                    .ToList();

            int totalModifiers = registeredModifiers.Count;
            int totalPages = Mathf.CeilToInt(totalModifiers / (float)SettingsPerPage);

            Logging.Log($"[SettingsMenuManager] Creating Modifier pages: " + $"{totalModifiers} modifiers, {totalPages} pages.");

            for (int pageIndex = 0; pageIndex < totalPages; pageIndex++)
            {
                var newPage = Object.Instantiate(pageOneTemplate, parent);

                newPage.name = $"CustomModifierPage_{pageIndex + 1}";
                newPage.SetActive(true);

                _spawnedModifierPages.Add(newPage);

                if (!singleActiveGroup._activeGroup.Contains(newPage))
                    singleActiveGroup._activeGroup.Add(newPage);

                var slots = new List<Transform>();

                for (int i = 0; i < newPage.transform.childCount; i++)
                    slots.Add(newPage.transform.GetChild(i));

                for (int slotIndex = 0; slotIndex < SettingsPerPage; slotIndex++)
                {
                    int modifierIndex = pageIndex * SettingsPerPage + slotIndex;

                    if (slotIndex >= slots.Count)
                        break;

                    var slot = slots[slotIndex];

                    if (modifierIndex >= totalModifiers)
                    {
                        slot.gameObject.SetActive(false);
                        continue;
                    }

                    var data = registeredModifiers[modifierIndex];

                    ApplyModifierSettingsItem(slot, data);

                    _slotModifierMap[slot.gameObject] = data;
                    slot.gameObject.SetActive(true);
                }
            }

            _modifierCurrentPageIndex = 0;

            if (totalPages > 0)
                SetModifierSettingsPage(0);

            BuildModifierTabAndNavigation();

            Logging.Log($"[SettingsMenuManager] Finished building Modifiers Settings. " + $"Root={modifierSettingsRoot.name}, Pages={_spawnedModifierPages.Count}.");
        }
    }
}