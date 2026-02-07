using UnityEngine;

namespace Plugins.Framewerk
{
    /// <summary>
    /// ScriptableObject for defining the context prefix segment in Addressable IDs.
    /// Can be shared across multiple contexts or unique per context.
    /// </summary>
    [CreateAssetMenu(fileName = "ContextPrefix", menuName = "Framewerk/Context Prefix")]
    public class ContextPrefix : ScriptableObject
    {
        [Tooltip("The context prefix segment (e.g. 'Examples', 'ListPopupDemo')")]
        public string Prefix;
    }
}
