using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Framewerk;
using Framewerk.Editor.Wizards;
using Plugins.Framewerk;

namespace Framewerk.Editor.Settings
{
    /// <summary>
    /// Unified settings window for Framewerk project configuration.
    /// Displays and allows editing of all Framewerk config assets.
    /// </summary>
    public class FramewerkSettingsWindow : EditorWindow
    {
        private const string DEFAULT_TEMPLATE_SET_CONFIG_PATH = "Assets/Settings/Framewerk/TemplateSetConfig.asset";
        private const string ACTIVE_TEMPLATE_SET_PREF_KEY = "Framewerk_ActiveTemplateSetGUID";
        
        // Editor Tools preferences
        public const string UI_PREFAB_PARENTING_FIXER_ENABLED_KEY = "Framewerk_UIPrefabParentingFixerEnabled";

        private TemplateSetConfig _templateSetConfig;
        private SerializedObject _templateSetConfigSO;

        private Vector2 _scrollPosition;
        
        // Address resolver
        private string _addressPattern;
        
        // Multiple template sets support
        private List<TemplateSetConfig> _allTemplateSetConfigs = new List<TemplateSetConfig>();
        private string[] _templateSetNames;
        private int _selectedTemplateSetIndex;
        
        // Cached styles
        private GUIStyle _sectionHeaderStyle;

        [MenuItem("Framewerk/Settings")]
        public static void ShowWindow()
        {
            var window = GetWindow<FramewerkSettingsWindow>("Framewerk Settings");
            window.minSize = new Vector2(450, 400);
            window.Show();
        }

        private void OnEnable()
        {
            RefreshConfigs();
        }

        private void OnFocus()
        {
            RefreshConfigs();
        }

        private void InitStyles()
        {
            if (_sectionHeaderStyle == null)
            {
                _sectionHeaderStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 14,
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black }
                };
            }
        }

        private void RefreshConfigs()
        {
            RefreshAllTemplateSetConfigs();
            _addressPattern = AddressBuilder.GetPattern();
        }

        private void RefreshAllTemplateSetConfigs()
        {
            _allTemplateSetConfigs.Clear();
            
            // Find all TemplateSetConfig assets in project
            string[] guids = AssetDatabase.FindAssets("t:TemplateSetConfig", new[] { "Assets" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var config = AssetDatabase.LoadAssetAtPath<TemplateSetConfig>(path);
                if (config != null)
                {
                    _allTemplateSetConfigs.Add(config);
                }
            }
            
            // Build names array for dropdown
            _templateSetNames = new string[_allTemplateSetConfigs.Count];
            for (int i = 0; i < _allTemplateSetConfigs.Count; i++)
            {
                string name = string.IsNullOrEmpty(_allTemplateSetConfigs[i].TemplateSetName) 
                    ? _allTemplateSetConfigs[i].name 
                    : _allTemplateSetConfigs[i].TemplateSetName;
                _templateSetNames[i] = name;
            }
            
            // Find active template set index
            string activeGuid = EditorPrefs.GetString(ACTIVE_TEMPLATE_SET_PREF_KEY, "");
            _selectedTemplateSetIndex = -1;
            
            for (int i = 0; i < _allTemplateSetConfigs.Count; i++)
            {
                string path = AssetDatabase.GetAssetPath(_allTemplateSetConfigs[i]);
                string guid = AssetDatabase.AssetPathToGUID(path);
                if (guid == activeGuid)
                {
                    _selectedTemplateSetIndex = i;
                    break;
                }
            }
            
            // If no active template set set but template sets exist, default to first
            if (_selectedTemplateSetIndex < 0 && _allTemplateSetConfigs.Count > 0)
            {
                _selectedTemplateSetIndex = 0;
                SetActiveTemplateSet(_allTemplateSetConfigs[0]);
            }
            
            // Update current template set config
            if (_selectedTemplateSetIndex >= 0 && _selectedTemplateSetIndex < _allTemplateSetConfigs.Count)
            {
                _templateSetConfig = _allTemplateSetConfigs[_selectedTemplateSetIndex];
                _templateSetConfigSO = new SerializedObject(_templateSetConfig);
            }
            else
            {
                _templateSetConfig = null;
                _templateSetConfigSO = null;
            }
        }

        private void SetActiveTemplateSet(TemplateSetConfig config)
        {
            if (config == null)
            {
                EditorPrefs.DeleteKey(ACTIVE_TEMPLATE_SET_PREF_KEY);
            }
            else
            {
                string path = AssetDatabase.GetAssetPath(config);
                string guid = AssetDatabase.AssetPathToGUID(path);
                EditorPrefs.SetString(ACTIVE_TEMPLATE_SET_PREF_KEY, guid);
            }
            
            // Clear TemplateSetResolver cache so it picks up the change
            TemplateSetResolver.ClearCache();
        }

        private void OnGUI()
        {
            InitStyles();
            
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Framewerk Project Settings", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Configure project-wide Framewerk settings.",
                MessageType.Info);
            GUILayout.Space(10);

            DrawEditorToolsSection();
            GUILayout.Space(10);
            DrawAddressResolverSection();
            GUILayout.Space(10);
            DrawTemplateSetConfigSection();

            EditorGUILayout.EndScrollView();
        }

        private void DrawSectionHeader(string title)
        {
            GUILayout.Space(8);
            
            // Draw background rect
            Rect headerRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.ExpandWidth(true), GUILayout.Height(28));
            
            // Background color - dark and prominent
            Color bgColor = EditorGUIUtility.isProSkin 
                ? new Color(0.1f, 0.1f, 0.1f, 1f) 
                : new Color(0.5f, 0.5f, 0.5f, 1f);
            EditorGUI.DrawRect(headerRect, bgColor);
            
            // Draw border lines for definition
            Color borderColor = EditorGUIUtility.isProSkin 
                ? new Color(0.05f, 0.05f, 0.05f, 1f) 
                : new Color(0.35f, 0.35f, 0.35f, 1f);
            EditorGUI.DrawRect(new Rect(headerRect.x, headerRect.y, headerRect.width, 1), borderColor);
            EditorGUI.DrawRect(new Rect(headerRect.x, headerRect.yMax - 1, headerRect.width, 1), borderColor);
            
            // Draw centered text
            GUI.Label(headerRect, title, _sectionHeaderStyle);
            
            GUILayout.Space(8);
        }

        private void DrawEditorToolsSection()
        {
            DrawSectionHeader("Editor Tools");
            
            // UI Prefab Parenting Fixer toggle
            bool fixerEnabled = EditorPrefs.GetBool(UI_PREFAB_PARENTING_FIXER_ENABLED_KEY, true);
            EditorGUI.BeginChangeCheck();
            fixerEnabled = EditorGUILayout.Toggle(
                new GUIContent("UI Prefab Parenting Fixer", 
                    "Automatically resets RectTransform position when UI prefabs are parented under a Canvas, " +
                    "fixing the common issue where prefabs get placed at canvas center offset."),
                fixerEnabled);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetBool(UI_PREFAB_PARENTING_FIXER_ENABLED_KEY, fixerEnabled);
                Debug.Log($"[Framewerk] UI Prefab Parenting Fixer {(fixerEnabled ? "enabled" : "disabled")}");
            }
        }

        private void DrawTemplateSetConfigSection()
        {
            DrawSectionHeader("Template Set");
            
            // Template set selector row (always visible, even if no template sets exist yet)
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.LabelField("Active Template Set:", GUILayout.Width(130));
            
            if (_allTemplateSetConfigs.Count > 0)
            {
                EditorGUI.BeginChangeCheck();
                _selectedTemplateSetIndex = EditorGUILayout.Popup(_selectedTemplateSetIndex, _templateSetNames);
                if (EditorGUI.EndChangeCheck() && _selectedTemplateSetIndex >= 0 && _selectedTemplateSetIndex < _allTemplateSetConfigs.Count)
                {
                    _templateSetConfig = _allTemplateSetConfigs[_selectedTemplateSetIndex];
                    _templateSetConfigSO = new SerializedObject(_templateSetConfig);
                    SetActiveTemplateSet(_templateSetConfig);
                }
            }
            else
            {
                EditorGUILayout.LabelField("(none)", EditorStyles.miniLabel);
            }
            
            // Plus button - create new template set
            if (GUILayout.Button("+", GUILayout.Width(25)))
            {
                ShowCreateTemplateSetDialog();
            }
            
            // Delete button
            EditorGUI.BeginDisabledGroup(_templateSetConfig == null);
            GUI.backgroundColor = _templateSetConfig != null ? new Color(1f, 0.6f, 0.6f) : Color.white;
            if (GUILayout.Button("−", GUILayout.Width(25)))
            {
                if (EditorUtility.DisplayDialog("Delete Template Set",
                    $"Are you sure you want to delete '{_templateSetConfig.TemplateSetName}'?\n\nThis cannot be undone.",
                    "Delete", "Cancel"))
                {
                    DeleteCurrentTemplateSet();
                }
            }
            GUI.backgroundColor = Color.white;
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(5);

            if (_templateSetConfig == null)
            {
                EditorGUILayout.HelpBox(
                    "No TemplateSetConfig found.\n" +
                    "Click + to create a new template set with template overrides.",
                    MessageType.None);
            }
            else
            {
                // Show asset location
                string path = AssetDatabase.GetAssetPath(_templateSetConfig);
                EditorGUILayout.LabelField("Location:", path, EditorStyles.miniLabel);

                GUILayout.Space(5);

                // Draw inline editor
                _templateSetConfigSO.Update();

                SerializedProperty iterator = _templateSetConfigSO.GetIterator();
                iterator.NextVisible(true); // Skip m_Script

                while (iterator.NextVisible(false))
                {
                    EditorGUILayout.PropertyField(iterator, true);
                }

                if (_templateSetConfigSO.ApplyModifiedProperties())
                {
                    EditorUtility.SetDirty(_templateSetConfig);
                    // Update dropdown names if template set name changed
                    RefreshAllTemplateSetConfigs();
                }
            }
        }

        private void DrawAddressResolverSection()
        {
            DrawSectionHeader("Address Resolver");

            EditorGUILayout.HelpBox(
                "Pattern for generating Addressable IDs (shared across the team).\n" +
                "Tokens: {ContextPrefix}, {CustomPrefix}, {UI}, {TypeKey}, {ClassName}",
                MessageType.None);

            GUILayout.Space(5);

            // Pattern text field - auto-saves on change
            EditorGUI.BeginChangeCheck();
            _addressPattern = EditorGUILayout.TextField("Pattern", _addressPattern);
            if (EditorGUI.EndChangeCheck())
            {
                SaveAddressPattern(_addressPattern);
            }

            GUILayout.Space(5);

            // Show live preview
            string preview = AddressBuilder.ResolvePattern(_addressPattern, new AddressContext
            {
                ContextPrefix = "Examples",
                CustomPrefix = "CustomPrefix",
                TypeKey = "Popup",
                ClassName = "SamplePopup"
            });
            EditorGUILayout.LabelField("Preview:", preview, EditorStyles.helpBox);

            GUILayout.Space(5);

            // Reset to Default button
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            bool isDefault = _addressPattern == AddressBuilder.DEFAULT_PATTERN;
            EditorGUI.BeginDisabledGroup(isDefault);
            if (GUILayout.Button("Reset to Default", GUILayout.Width(120)))
            {
                DeleteAddressResolverConfig();
                _addressPattern = AddressBuilder.DEFAULT_PATTERN;
                Debug.Log("[Framewerk] Address pattern reset to default");
            }
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.EndHorizontal();
        }

        private void SaveAddressPattern(string pattern)
        {
            // AddressBuilder.SetPattern handles all file management:
            // - Creates config file if needed
            // - Deletes config file if pattern equals default
            AddressBuilder.SetPattern(pattern);
        }

        private void DeleteAddressResolverConfig()
        {
            // Passing null resets to default and deletes the config file
            AddressBuilder.SetPattern(null);
        }

        private void ShowCreateTemplateSetDialog()
        {
            TemplateSetNamePopup.Show(CreateTemplateSetWithName);
        }

        private void CreateTemplateSetWithName(string templateSetName)
        {
            if (string.IsNullOrWhiteSpace(templateSetName))
            {
                templateSetName = "Default Set";
            }
            
            // Generate unique filename
            string basePath = "Assets/Settings/Framewerk/";
            string fileName = templateSetName.Replace(" ", "") + "TemplateSetConfig";
            string fullPath = basePath + fileName + ".asset";
            
            // Make path unique if file exists
            int counter = 1;
            while (System.IO.File.Exists(fullPath))
            {
                fullPath = basePath + fileName + counter + ".asset";
                counter++;
            }
            
            EnsureDirectoryExists(fullPath);
            
            var config = CreateInstance<TemplateSetConfig>();
            config.TemplateSetName = templateSetName;
            
            AssetDatabase.CreateAsset(config, fullPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            // Set as active and refresh
            SetActiveTemplateSet(config);
            RefreshConfigs();
            
            Debug.Log($"[Framewerk] Created TemplateSetConfig '{templateSetName}' at {fullPath}");
        }

        private void DeleteCurrentTemplateSet()
        {
            if (_templateSetConfig == null)
                return;

            string path = AssetDatabase.GetAssetPath(_templateSetConfig);
            string deletedName = _templateSetConfig.TemplateSetName;
            
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            // Clear active template set pref if it was the deleted one
            SetActiveTemplateSet(null);
            RefreshConfigs();
            
            Debug.Log($"[Framewerk] Deleted TemplateSetConfig '{deletedName}' at {path}");
        }

        private void EnsureDirectoryExists(string assetPath)
        {
            string directory = System.IO.Path.GetDirectoryName(assetPath);
            
            if (!AssetDatabase.IsValidFolder(directory))
            {
                // Create folder hierarchy
                string[] parts = directory.Split('/');
                string currentPath = parts[0]; // "Assets"
                
                for (int i = 1; i < parts.Length; i++)
                {
                    string nextPath = currentPath + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(nextPath))
                    {
                        AssetDatabase.CreateFolder(currentPath, parts[i]);
                    }
                    currentPath = nextPath;
                }
            }
        }
    }

    /// <summary>
    /// Simple popup window for entering a template set name.
    /// </summary>
    public class TemplateSetNamePopup : EditorWindow
    {
        private string _templateSetName = "Default Set";
        private System.Action<string> _onComplete;
        private bool _focusTextField = true;

        public static void Show(System.Action<string> onComplete)
        {
            var window = CreateInstance<TemplateSetNamePopup>();
            window._onComplete = onComplete;
            window.titleContent = new GUIContent("New Template Set");
            window.minSize = new Vector2(300, 80);
            window.maxSize = new Vector2(300, 80);
            window.ShowUtility();
            window.CenterOnMainWin();
        }

        private void CenterOnMainWin()
        {
            var main = EditorGUIUtility.GetMainWindowPosition();
            var pos = position;
            pos.x = main.x + (main.width - pos.width) * 0.5f;
            pos.y = main.y + (main.height - pos.height) * 0.3f;
            position = pos;
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            
            EditorGUILayout.LabelField("Enter template set name:");
            
            GUI.SetNextControlName("TemplateSetNameField");
            _templateSetName = EditorGUILayout.TextField(_templateSetName);
            
            if (_focusTextField)
            {
                EditorGUI.FocusTextInControl("TemplateSetNameField");
                _focusTextField = false;
            }
            
            GUILayout.Space(10);
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Cancel", GUILayout.Width(80)))
            {
                Close();
            }
            
            if (GUILayout.Button("Create", GUILayout.Width(80)) || 
                (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return))
            {
                _onComplete?.Invoke(_templateSetName);
                Close();
            }
            
            EditorGUILayout.EndHorizontal();
        }
    }
}
