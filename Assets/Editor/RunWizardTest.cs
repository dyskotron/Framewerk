using UnityEditor;
using UnityEngine;
using Framewerk.Editor.Wizards;

[InitializeOnLoad]
public static class RunWizardTest
{
    static RunWizardTest()
    {
        EditorApplication.delayCall += SetupWizardJob;
    }

    private static void SetupWizardJob()
    {
        // Only run once - check if we've already set this
        if (EditorPrefs.HasKey("FramewerkWizard_PendingJob"))
        {
            Debug.Log("Wizard job already pending, skipping test setup");
            return;
        }

        // Create wizard job for HexTest List component
        WizardJob job = new WizardJob
        {
            componentName = "HexTest",
            componentType = (int)ComponentType.List,
            namespaceName = "Scripts",
            scriptFolder = "Assets/Scripts",
            prefabFolder = "Assets/Prefabs",
            contextFilePath = null, // Not setting context binding for test
            markAddressable = false,
            openAfterCreate = false,
            viewTypeName = "Scripts.HexTestView",
            mediatorTypeName = "Scripts.HexTestMediator",
            prefabPath = "Assets/Prefabs/HexTest.prefab",
            addressableAddress = "UI/HexTest",
            dataTypeName = "Scripts.HexTestData",
            itemViewTypeName = "Scripts.HexTestItemView",
            itemMediatorTypeName = "Scripts.HexTestItemMediator",
            itemPrefabPath = "Assets/Prefabs/HexTestItem.prefab"
        };

        // Save job to EditorPrefs
        string json = JsonUtility.ToJson(job);
        EditorPrefs.SetString("FramewerkWizard_PendingJob", json);

        Debug.Log($"Set pending wizard job for HexTest. Job JSON:\n{json}");
        Debug.Log("Triggering asset refresh - wizard will complete after recompile...");

        // Trigger refresh to cause recompile
        AssetDatabase.Refresh();
    }
}