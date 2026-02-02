using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Framewerk.Editor.Wizards
{
    /// <summary>
    /// Handles scene setup after scripts have been compiled.
    /// </summary>
    public class SceneGenerator
    {
        private const string PENDING_SCENE_JOB_KEY = "FramewerkWizard_PendingSceneJob";

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
        }

        private static void OnAfterAssemblyReload()
        {
            // Check if we have a pending scene job
            if (!EditorPrefs.HasKey(PENDING_SCENE_JOB_KEY))
                return;

            string json = EditorPrefs.GetString(PENDING_SCENE_JOB_KEY);
            if (string.IsNullOrEmpty(json))
                return;

            SceneJob job = JsonUtility.FromJson<SceneJob>(json);
            if (job == null)
                return;

            // Clear the pending job immediately to prevent reprocessing
            EditorPrefs.DeleteKey(PENDING_SCENE_JOB_KEY);

            try
            {
                CompleteSceneSetup(job);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to complete scene setup: {e.Message}\n{e.StackTrace}");
            }
        }

        private static void CompleteSceneSetup(SceneJob job)
        {
            // Find the Bootstrap type
            Type bootstrapType = ComponentScaffoldCompleter.FindType(job.bootstrapTypeName);
            if (bootstrapType == null)
            {
                Debug.LogError($"Could not find type: {job.bootstrapTypeName}");
                return;
            }

            // Open the scene
            Scene scene = EditorSceneManager.OpenScene(job.scenePath, OpenSceneMode.Single);

            // Find the Bootstrap GameObject
            GameObject bootstrapGo = null;
            foreach (GameObject rootGo in scene.GetRootGameObjects())
            {
                if (rootGo.name == "Bootstrap")
                {
                    bootstrapGo = rootGo;
                    break;
                }
            }

            if (bootstrapGo == null)
            {
                Debug.LogError("Bootstrap GameObject not found in scene");
                return;
            }

            // Add the Bootstrap component
            Component bootstrapComponent = bootstrapGo.AddComponent(bootstrapType);

            // Get the ViewConfig component
            Component viewConfigComponent = bootstrapGo.GetComponent(typeof(Plugins.Framewerk.ViewConfig));
            if (viewConfigComponent == null)
            {
                Debug.LogError("ViewConfig component not found on Bootstrap GameObject");
                return;
            }

            // Wire the viewConfig reference
            SerializedObject so = new SerializedObject(bootstrapComponent);
            SerializedProperty viewConfigProp = so.FindProperty("viewConfig");
            if (viewConfigProp != null)
            {
                viewConfigProp.objectReferenceValue = viewConfigComponent;
                so.ApplyModifiedProperties();
            }
            else
            {
                Debug.LogWarning($"viewConfig field not found on {bootstrapType.Name}");
            }

            // Save the scene
            EditorSceneManager.SaveScene(scene);

            Debug.Log($"<color=green>✓ Wizard: {job.sceneName} scene created successfully</color>");

            // Highlight the scene in the Project window
            UnityEngine.Object sceneAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(job.scenePath);
            if (sceneAsset != null)
            {
                EditorGUIUtility.PingObject(sceneAsset);
                Selection.activeObject = sceneAsset;
            }
        }
    }
}
