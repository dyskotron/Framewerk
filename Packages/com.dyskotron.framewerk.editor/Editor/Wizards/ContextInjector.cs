using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Framewerk.Editor.Wizards
{
    public static class ContextInjector
    {
        public static bool InjectBinding(string contextFilePath, string viewName, string mediatorName, string namespaceName)
        {
            if (!File.Exists(contextFilePath))
            {
                Debug.LogError($"Context file not found: {contextFilePath}");
                return false;
            }

            string content = File.ReadAllText(contextFilePath);
            string originalContent = content;

            // Add using statement if not present
            string usingStatement = $"using {namespaceName};";
            if (!content.Contains(usingStatement))
            {
                // Find the last using statement
                var usingMatches = Regex.Matches(content, @"^using .*?;", RegexOptions.Multiline);
                if (usingMatches.Count > 0)
                {
                    var lastUsing = usingMatches[usingMatches.Count - 1];
                    int insertPos = lastUsing.Index + lastUsing.Length;
                    content = content.Insert(insertPos, "\n" + usingStatement);
                }
                else
                {
                    // No using statements found, add at the top after any comments
                    content = usingStatement + "\n\n" + content;
                }
            }

            // Find the last mediationBinder.Bind line
            var bindMatches = Regex.Matches(content, @"^(\s*)mediationBinder\.Bind<.*?>\(\)\.To<.*?>\(\);", RegexOptions.Multiline);

            if (bindMatches.Count > 0)
            {
                var lastBind = bindMatches[bindMatches.Count - 1];
                string indentation = lastBind.Groups[1].Value;
                int insertPos = lastBind.Index + lastBind.Length;

                string bindingLine = $"\n{indentation}mediationBinder.Bind<{viewName}>().To<{mediatorName}>();";
                content = content.Insert(insertPos, bindingLine);
            }
            else
            {
                // Fallback: try to find mapBindings method and add before closing brace
                var mapBindingsMatch = Regex.Match(content, @"(protected override void mapBindings\(\).*?\{)(.*?)(\n\s*\})", RegexOptions.Singleline);

                if (mapBindingsMatch.Success)
                {
                    // Get indentation from the method
                    string methodIndent = GetIndentation(content, mapBindingsMatch.Index);
                    string bindingIndent = methodIndent + "    "; // Add one level of indentation

                    string bindingLine = $"{bindingIndent}mediationBinder.Bind<{viewName}>().To<{mediatorName}>();\n";

                    // Insert before the closing brace
                    int insertPos = mapBindingsMatch.Groups[3].Index;
                    content = content.Insert(insertPos, bindingLine);
                }
                else
                {
                    Debug.LogWarning($"Could not find appropriate location to inject binding in {contextFilePath}");
                    return false;
                }
            }

            if (content != originalContent)
            {
                File.WriteAllText(contextFilePath, content);
                Debug.Log($"Injected binding for {viewName} → {mediatorName} into {Path.GetFileName(contextFilePath)}");
                return true;
            }

            return false;
        }

        private static string GetIndentation(string text, int position)
        {
            int lineStart = text.LastIndexOf('\n', position) + 1;
            int indentEnd = lineStart;
            while (indentEnd < text.Length && (text[indentEnd] == ' ' || text[indentEnd] == '\t'))
            {
                indentEnd++;
            }
            return text.Substring(lineStart, indentEnd - lineStart);
        }
    }
}
