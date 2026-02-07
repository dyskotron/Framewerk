using System.Linq;

namespace Framewerk.Editor.Wizards
{
    public static class NamespaceResolver
    {
        public static string ResolveFromPath(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath))
                return string.Empty;

            var segments = folderPath.Replace('\\', '/').TrimEnd('/')
                .Split('/')
                .SkipWhile(s => s == "Assets" || s == "Scripts")
                .Where(s => !string.IsNullOrEmpty(s));

            return string.Join(".", segments);
        }
    }
}
