using System;

namespace Framewerk.Editor.Wizards
{
    [Serializable]
    public class SceneJob
    {
        public string sceneName;
        public string namespaceName;
        public string sceneFolder;
        public string scriptFolder;

        public string scenePath;
        public string bootstrapTypeName;
        public string contextTypeName;
        public string startCommandTypeName;
    }
}
