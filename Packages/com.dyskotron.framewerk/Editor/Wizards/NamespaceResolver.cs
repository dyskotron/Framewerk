namespace Framewerk.Editor.Wizards
{
    public static class NamespaceResolver
    {
        public static string ResolveFromPath(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath))
                return string.Empty;

            // Normalize path separators
            folderPath = folderPath.Replace('\\', '/');

            // Try to strip "Assets/Scripts/" prefix first
            if (folderPath.Contains("/Scripts/"))
            {
                int scriptsIndex = folderPath.IndexOf("/Scripts/");
                folderPath = folderPath.Substring(scriptsIndex + "/Scripts/".Length);
            }
            // Fallback to stripping just "Assets/" if no Scripts folder
            else if (folderPath.StartsWith("Assets/"))
            {
                folderPath = folderPath.Substring("Assets/".Length);
            }

            // Remove trailing slashes
            folderPath = folderPath.TrimEnd('/');

            // Replace "/" with "."
            string namespaceName = folderPath.Replace('/', '.');

            return namespaceName;
        }
    }
}
