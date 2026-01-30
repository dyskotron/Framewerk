using System;

namespace Framewerk.Editor.Wizards
{
    [Serializable]
    public class WizardJob
    {
        public string componentName;
        public int componentType;
        public string namespaceName;
        public string scriptFolder;
        public string prefabFolder;
        public string contextFilePath;
        public bool markAddressable;
        public bool openAfterCreate;

        public string viewTypeName;
        public string mediatorTypeName;
        public string prefabPath;
        public string addressableAddress;

        // For List type
        public string itemViewTypeName;
        public string itemMediatorTypeName;
        public string itemPrefabPath;
        public string itemAddressableAddress;
        public string dataTypeName;
    }
}
