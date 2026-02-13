using UnityEditor;
[InitializeOnLoad]
public class UnlockAssembly { static UnlockAssembly() { EditorApplication.UnlockReloadAssemblies(); } }