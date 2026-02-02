using System.IO;
using UnityEditor;
using UnityEngine;

namespace Framewerk.Editor.Wizards
{
    public class SceneScaffoldWizard : EditorWindow
    {
        private const string PREFS_NAMESPACE = "FramewerkSceneWizard_Namespace";
        private const string PREFS_SCRIPT_FOLDER = "FramewerkSceneWizard_ScriptFolder";
        private const string PREFS_SCENE_FOLDER = "FramewerkSceneWizard_SceneFolder";

        private string sceneName = "";
        private string namespaceName = "";
        private string scriptFolder = "Assets/Scripts";
        private string sceneFolder = "Assets/Scenes";

        private Vector2 scrollPosition;

        [MenuItem("Framewerk/Create Scene")]
        public static void ShowWindow()
        {
            var window = GetWindow<SceneScaffoldWizard>("Framewerk Scene Wizard");
            window.minSize = new Vector2(450, 400);
            window.Show();
        }

        private void OnEnable()
        {
            LoadPreferences();
        }

        private void LoadPreferences()
        {
            namespaceName = EditorPrefs.GetString(PREFS_NAMESPACE, "");
            scriptFolder = EditorPrefs.GetString(PREFS_SCRIPT_FOLDER, "Assets/Scripts");
            sceneFolder = EditorPrefs.GetString(PREFS_SCENE_FOLDER, "Assets/Scenes");
        }

        private void SavePreferences()
        {
            EditorPrefs.SetString(PREFS_NAMESPACE, namespaceName);
            EditorPrefs.SetString(PREFS_SCRIPT_FOLDER, scriptFolder);
            EditorPrefs.SetString(PREFS_SCENE_FOLDER, sceneFolder);
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Space(10);

            EditorGUILayout.LabelField("Framewerk Scene Wizard", EditorStyles.boldLabel);

            GUILayout.Space(10);

            // Scene Name
            sceneName = EditorGUILayout.TextField("Scene Name", sceneName);

            GUILayout.Space(10);

            // Namespace
            EditorGUILayout.BeginHorizontal();
            namespaceName = EditorGUILayout.TextField("Namespace", namespaceName);
            if (GUILayout.Button("Auto", GUILayout.Width(50)))
            {
                namespaceName = NamespaceResolver.ResolveFromPath(scriptFolder);
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            // Script Folder
            EditorGUILayout.BeginHorizontal();
            scriptFolder = EditorGUILayout.TextField("Script Folder", scriptFolder);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFolderPanel("Select Script Folder", scriptFolder, "");
                if (!string.IsNullOrEmpty(path))
                {
                    if (path.StartsWith(Application.dataPath))
                    {
                        scriptFolder = "Assets" + path.Substring(Application.dataPath.Length);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            // Scene Folder
            EditorGUILayout.BeginHorizontal();
            sceneFolder = EditorGUILayout.TextField("Scene Folder", sceneFolder);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFolderPanel("Select Scene Folder", sceneFolder, "");
                if (!string.IsNullOrEmpty(path))
                {
                    if (path.StartsWith(Application.dataPath))
                    {
                        sceneFolder = "Assets" + path.Substring(Application.dataPath.Length);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            // Preview
            DrawPreview();

            GUILayout.Space(10);

            // Buttons
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Generate", GUILayout.Width(100), GUILayout.Height(30)))
            {
                Generate();
            }

            if (GUILayout.Button("Cancel", GUILayout.Width(100), GUILayout.Height(30)))
            {
                Close();
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            EditorGUILayout.EndScrollView();
        }

        private void DrawPreview()
        {
            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (string.IsNullOrEmpty(sceneName))
            {
                EditorGUILayout.LabelField("Enter a scene name to see preview", EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                EditorGUILayout.LabelField("Will generate:", EditorStyles.miniBoldLabel);

                EditorGUILayout.LabelField($"  • {sceneName}Bootstrap.cs", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"  • {sceneName}Context.cs", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"  • {sceneName}StartCommand.cs", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"  • {sceneName}.unity", EditorStyles.miniLabel);
            }

            EditorGUILayout.EndVertical();
        }

        private void Generate()
        {
            // Validate
            if (string.IsNullOrEmpty(sceneName))
            {
                EditorUtility.DisplayDialog("Validation Error", "Scene name is required.", "OK");
                return;
            }

            if (string.IsNullOrEmpty(namespaceName))
            {
                EditorUtility.DisplayDialog("Validation Error", "Namespace is required.", "OK");
                return;
            }

            // Save preferences
            SavePreferences();

            // Create folders if needed
            if (!Directory.Exists(scriptFolder))
            {
                Directory.CreateDirectory(scriptFolder);
            }

            if (!Directory.Exists(sceneFolder))
            {
                Directory.CreateDirectory(sceneFolder);
            }

            // Build paths
            string scenePath = Path.Combine(sceneFolder, $"{sceneName}.unity");
            string bootstrapPath = Path.Combine(scriptFolder, $"{sceneName}Bootstrap.cs");
            string contextPath = Path.Combine(scriptFolder, $"{sceneName}Context.cs");
            string startCommandPath = Path.Combine(scriptFolder, $"{sceneName}StartCommand.cs");

            // Check for existing files
            var existingFiles = new System.Collections.Generic.List<string>();
            if (File.Exists(scenePath)) existingFiles.Add(scenePath);
            if (File.Exists(bootstrapPath)) existingFiles.Add(bootstrapPath);
            if (File.Exists(contextPath)) existingFiles.Add(contextPath);
            if (File.Exists(startCommandPath)) existingFiles.Add(startCommandPath);

            if (existingFiles.Count > 0)
            {
                string fileList = string.Join("\n", existingFiles);
                if (!EditorUtility.DisplayDialog("Files Already Exist",
                    $"The following files already exist:\n\n{fileList}\n\nOverwrite?",
                    "Overwrite", "Cancel"))
                {
                    return;
                }
            }

            // Load templates
            string templatePath = "Packages/com.dyskotron.framewerk/Editor/Wizards/Templates/";
            string bootstrapTemplate = File.ReadAllText(templatePath + "Bootstrap.cs.txt");
            string contextTemplate = File.ReadAllText(templatePath + "Context.cs.txt");
            string startCommandTemplate = File.ReadAllText(templatePath + "StartCommand.cs.txt");

            // Replace placeholders
            string bootstrapCode = bootstrapTemplate
                .Replace("#SCENENAME#", sceneName)
                .Replace("#NAMESPACE#", namespaceName);
            string contextCode = contextTemplate
                .Replace("#SCENENAME#", sceneName)
                .Replace("#NAMESPACE#", namespaceName);
            string startCommandCode = startCommandTemplate
                .Replace("#SCENENAME#", sceneName)
                .Replace("#NAMESPACE#", namespaceName);

            // Write scripts
            File.WriteAllText(bootstrapPath, bootstrapCode);
            File.WriteAllText(contextPath, contextCode);
            File.WriteAllText(startCommandPath, startCommandCode);

            // Copy scene template
            string sceneTemplatePath = "Packages/com.dyskotron.framewerk/Editor/Wizards/Templates/SceneTemplate.unity";
            if (!File.Exists(sceneTemplatePath))
            {
                EditorUtility.DisplayDialog("Error", "SceneTemplate.unity not found in Templates folder.", "OK");
                return;
            }

            File.Copy(sceneTemplatePath, scenePath, true);

            // Create SceneJob for post-domain reload setup
            SceneJob job = new SceneJob
            {
                sceneName = sceneName,
                namespaceName = namespaceName,
                sceneFolder = sceneFolder,
                scriptFolder = scriptFolder,
                scenePath = scenePath,
                bootstrapTypeName = $"{namespaceName}.{sceneName}Bootstrap",
                contextTypeName = $"{namespaceName}.{sceneName}Context",
                startCommandTypeName = $"{namespaceName}.{sceneName}StartCommand"
            };

            // Save job to EditorPrefs
            string json = JsonUtility.ToJson(job);
            EditorPrefs.SetString("FramewerkWizard_PendingSceneJob", json);

            // Refresh to trigger recompile
            AssetDatabase.Refresh();

            Debug.Log($"Generated scene and scripts for {sceneName}. Waiting for recompile to complete scene setup...");

            Close();
        }
    }
}
