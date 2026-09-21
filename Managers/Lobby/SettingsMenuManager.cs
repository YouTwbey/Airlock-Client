using System;
using System.Collections.Generic;
using System.Linq;
using AirlockClient.Attributes;
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
    public class AdvancedSettingDefinition(
        string label,
        Func<SubRoleData, int> getValue,
        Action<string, int> changeValue,
        int step = 5)
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

    public static class SettingsMenuManager
    {
        private const int SettingsPerPage = 5;
        private const int MaxRoleAmount = 10;
        private const int AdvancedSlotsPerRole = 4;

        private const string SkeldSettingsPath = "-------- UI OBJECTS --------/UI_LobbyScreen/LobbyScreenParent/Match Customization/UI_MatchCustomization";
        private const string PolusPointSettingsPath = "P_Office_01/PortalCulling_Exclude/UI_LobbyScreen/LobbyScreenParent/Match Customization/UI_MatchCustomization";
        private const string MessHallSettingsPath = "P_Dropship_ModMap02/PortalCulling_Exclude/SpawnRoomEssentials/UI_LobbyScreen/LobbyScreenParent/Match Customization/UI_MatchCustomization";

        private static string SettingsRootPath
        {
            get
            {
                return SceneManager.GetActiveScene().name switch
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

        private static readonly string[] TemplatePageNames =
        [
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
        private static readonly Dictionary<GameObject, SubRoleData> _slotRoleMap = new();
        private static readonly List<GameObject> _spawnedAdvancedPages = new();
        private static readonly Dictionary<GameObject, SubRoleData> _advancedSlotRoleMap = new();
        private static readonly Dictionary<SubRoleData, (TextKey NameKey, TextKey DescKey, IntVariable Variable)> _roleAssetCache = new();
        private static readonly Dictionary<GameObject, int> _slotCurrentAmount = new();

        public static void BuildSettingsPages()
        {
            if (string.IsNullOrEmpty(RolesSettingsPath))
            {
                Logging.Error(
                    $"[SettingsMenuManager] Unsupported scene '{SceneManager.GetActiveScene().name}'.");
                return;
            }

            var pageOneObj = GameObject.Find(PageOnePath);

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
                newPage.name = $"CustomRolePage_{pageIndex + 1}";
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

            var slot = GameObject.Find(slotPath);

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

            button.OnButtonPressed.RemoveAllListeners();

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

        private static string FindVanillaAdvancedPage(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName) || string.IsNullOrEmpty(AdvancedSettingsPath)) return null;
            
            var advancedRoot = GameObject.Find(AdvancedSettingsPath);

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

            var advancedRoot = GameObject.Find(AdvancedSettingsPath);

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

            var targetObject = targetPage.gameObject;
            var activeGroup = advancedRoot.GetComponent<SingleActiveGroup>();

            foreach (var templateName in TemplatePageNames)
            {
                var page = advancedRoot.transform.Find(templateName);

                if (page == null) continue;

                if (activeGroup != null && !activeGroup._activeGroup.Contains(page.gameObject))
                {
                    activeGroup._activeGroup.Add(page.gameObject);
                }

                page.gameObject.SetActive(page.gameObject == targetObject);
            }

            foreach (var customPage in _spawnedAdvancedPages.Where(customPage => customPage != null))
            {
                customPage.SetActive(false);

                if (activeGroup != null && activeGroup._activeGroup.Contains(customPage))
                {
                    activeGroup._activeGroup.Remove(customPage);
                }
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

            foreach (var page in _spawnedAdvancedPages.Where(page => page != null))
            {
                if (activeGroup != null && !activeGroup._activeGroup.Contains(page))
                {
                    activeGroup._activeGroup.Add(page);
                }

                page.SetActive(page == targetPage);
            }

            foreach (var pageName in TemplatePageNames)
            {
                var template = parent?.Find(pageName);

                if (template == null) continue;

                template.gameObject.SetActive(false);

                if (activeGroup != null && activeGroup._activeGroup.Contains(template.gameObject))
                {
                    activeGroup._activeGroup.Remove(template.gameObject);
                }
            }

            targetPage.SetActive(true);

            Logging.Log($"[SettingsMenuManager] Opened custom advanced page for '{data.Name}'.");
        }

        private static void DisableTemplatePages(Transform parent, SingleActiveGroup singleActiveGroup)
        {
            if (parent == null)
                return;

            foreach (var pageName in TemplatePageNames)
            {
                var pageTransform =
                    parent.Find(pageName);

                if (pageTransform == null)
                    continue;

                var page = pageTransform.gameObject;

                page.SetActive(false);

                if (singleActiveGroup != null && singleActiveGroup._activeGroup.Contains(page))
                {
                    singleActiveGroup._activeGroup.Remove(page);
                }
            }
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

            advancedButton.OnButtonPressed.RemoveAllListeners();

            var capturedRole = data;

            advancedButton.OnButtonPressed.AddListener((UnityAction<XRHand>)(hand => OpenAdvancedForRole(capturedRole)));
        }

        public static void BuildAdvancedPages()
        {
            Logging.Log("[SettingsMenuManager] BuildAdvancedPages() START");

            if (string.IsNullOrEmpty(pathToPage7))
            {
                Logging.Error($"[SettingsMenuManager] Unsupported scene '{SceneManager.GetActiveScene().name}'.");
                return;
            }

            var page7Obj = GameObject.Find(pathToPage7);

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

        public static void OnUpdate()
        {
            if (string.IsNullOrEmpty(RolesSettingsPath))
                return;

            var rolesSettingsObj =
                GameObject.Find(RolesSettingsPath);

            if (rolesSettingsObj == null)
                return;

            var singleActiveGroup =
                rolesSettingsObj.GetComponent<SingleActiveGroup>();

            if (singleActiveGroup == null)
                return;

            foreach (var page in _spawnedPages.Where(page => page != null))
            {
                if (!singleActiveGroup._activeGroup.Contains(page))
                    singleActiveGroup._activeGroup.Add(page);

                for (var i = 0; i < page.transform.childCount; i++)
                {
                    var slot =
                        page.transform.GetChild(i).gameObject;

                    if (!_slotRoleMap.TryGetValue(
                            slot,
                            out var data))
                        continue;

                    if (!_roleAssetCache.TryGetValue( data, out var assets)) continue;
                    var comp = slot.GetComponent<UIRoleDescription>();

                    if (comp == null) continue;

                    comp._roleDescription = assets.DescKey;
                    comp._roleName = assets.NameKey;
                    comp._gameTeam = data.Team;
                    
                    if (comp._crewOrImpText != null)
                    {
                        comp._crewOrImpText.text = "<color=#636363>Neutral</color>";
                    }
                    var optionText = slot.transform .Find("Option Text") ?.GetComponent<TextMeshPro>();

                    if (optionText != null) optionText.text = data.Name;
                }
            }
        }

        private static TextKey CreateTextKey() =>
            Object.Instantiate(
                StaticRefs.Role
                    ._availableRoles
                    .AllRoles[3]
                    .EjectionReveal);

        private static void RegisterTextKey(
            string text,
            TextKey textKey)
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
    }
}