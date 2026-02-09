using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using Framewerk.Editor.Settings;

namespace Framewerk.Editor
{
    /// <summary>
    /// Automatically resets RectTransform anchoredPosition when a UI prefab
    /// is dragged/parented under a Canvas. Fixes the common issue where prefabs get
    /// placed at canvas center offset instead of their intended position.
    /// 
    /// Approach: On every hierarchyChanged, scan all Canvases for new RectTransforms
    /// that weren't tracked before. Watch those for position offset and fix if needed.
    /// </summary>
    [InitializeOnLoad]
    public static class UIPrefabParentingFixer
    {
        private const string LOG_PREFIX = "[UIPrefabParentingFixer] ";
        private const float POSITION_EPSILON = 1f; // Difference threshold to detect unwanted offset
        private const int MAX_WATCH_FRAMES = 10;
        
        /// <summary>
        /// Check if the fixer is enabled via Framewerk settings.
        /// </summary>
        private static bool IsEnabled => EditorPrefs.GetBool(FramewerkSettingsWindow.UI_PREFAB_PARENTING_FIXER_ENABLED_KEY, true);
        
        // All known RectTransform instance IDs under Canvases
        private static readonly HashSet<int> _knownRectTransforms = new HashSet<int>();
        
        // Objects we're watching for position changes
        private class WatchEntry
        {
            public RectTransform rectTransform;
            public Vector2 originalPrefabPosition;
            public int framesRemaining;
        }
        private static readonly List<WatchEntry> _watching = new List<WatchEntry>();
        
        static UIPrefabParentingFixer()
        {
            Debug.Log(LOG_PREFIX + "★★★ INITIALIZING ★★★");
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            EditorApplication.update += OnUpdate;
            
            // Initial scan to populate known set
            ScanAndTrackAllCanvasRectTransforms(watchNew: false);
        }
        
        private static void OnHierarchyChanged()
        {
            if (!IsEnabled)
                return;
            
            // Scan all canvases for new RectTransforms
            ScanAndTrackAllCanvasRectTransforms(watchNew: true);
        }
        
        private static void ScanAndTrackAllCanvasRectTransforms(bool watchNew)
        {
            // Find all root Canvases in the scene
            var allCanvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            
            foreach (var canvas in allCanvases)
            {
                if (canvas == null) continue;
                
                // Get all RectTransforms under this canvas (including nested)
                var allRects = canvas.GetComponentsInChildren<RectTransform>(true);
                
                foreach (var rt in allRects)
                {
                    if (rt == null) continue;
                    
                    int instanceId = rt.GetInstanceID();
                    
                    if (!_knownRectTransforms.Contains(instanceId))
                    {
                        // New RectTransform detected!
                        _knownRectTransforms.Add(instanceId);
                        
                        if (watchNew)
                        {
                            // Skip if parent has a LayoutGroup - it controls positioning
                            if (rt.parent != null)
                            {
                                var parentLayoutGroup = rt.parent.GetComponent<LayoutGroup>();
                                if (parentLayoutGroup != null)
                                {
                                    continue;
                                }
                            }
                            
                            // Check if it's from a prefab
                            var sourceRt = PrefabUtility.GetCorrespondingObjectFromSource(rt);
                            if (sourceRt != null)
                            {
                                var originalPosition = sourceRt.anchoredPosition;
                                
                                Debug.Log(LOG_PREFIX + $"Watching '{rt.name}' for offset (prefab pos: {originalPosition})");
                                
                                _watching.Add(new WatchEntry
                                {
                                    rectTransform = rt,
                                    originalPrefabPosition = originalPosition,
                                    framesRemaining = MAX_WATCH_FRAMES
                                });
                            }
                        }
                    }
                }
            }
            
            // Cleanup: remove instance IDs of destroyed objects
            // (Only do this occasionally to avoid performance issues)
            if (_knownRectTransforms.Count > 1000)
            {
                CleanupDestroyedReferences();
            }
        }
        
        private static void CleanupDestroyedReferences()
        {
            // Can't easily check if instanceId is still valid, so we rebuild
            _knownRectTransforms.Clear();
            ScanAndTrackAllCanvasRectTransforms(watchNew: false);
        }
        
        private static void OnUpdate()
        {
            if (!IsEnabled || _watching.Count == 0)
                return;
            
            for (int i = _watching.Count - 1; i >= 0; i--)
            {
                var entry = _watching[i];
                
                if (entry.rectTransform == null)
                {
                    _watching.RemoveAt(i);
                    continue;
                }
                
                var currentPos = entry.rectTransform.anchoredPosition;
                float distanceFromOriginal = Vector2.Distance(currentPos, entry.originalPrefabPosition);
                
                if (distanceFromOriginal > POSITION_EPSILON)
                {
                    // Unity applied the wrong offset - fix it!
                    Debug.Log(LOG_PREFIX + $"★ CAUGHT! '{entry.rectTransform.name}' offset to {currentPos}, fixing to {entry.originalPrefabPosition}");
                    
                    // Capture values for closure
                    var rt = entry.rectTransform;
                    var targetPos = entry.originalPrefabPosition;
                    
                    // Use delayCall to run AFTER Unity finishes its layout pass
                    EditorApplication.delayCall += () =>
                    {
                        if (rt == null) return;
                        
                        var posBeforeFix = rt.anchoredPosition;
                        Undo.RecordObject(rt, "Fix UI Prefab Position");
                        rt.anchoredPosition = targetPos;
                        EditorUtility.SetDirty(rt);
                        
                        Debug.Log(LOG_PREFIX + $"★ FIXED '{rt.name}' to {targetPos} (was {posBeforeFix})");
                        
                        // Double delayCall: re-apply after another frame in case Unity overwrites again
                        EditorApplication.delayCall += () =>
                        {
                            if (rt == null) return;
                            if (Vector2.Distance(rt.anchoredPosition, targetPos) > POSITION_EPSILON)
                            {
                                Debug.Log(LOG_PREFIX + $"★ RE-FIXING '{rt.name}' (Unity overwrote to {rt.anchoredPosition})");
                                Undo.RecordObject(rt, "Fix UI Prefab Position");
                                rt.anchoredPosition = targetPos;
                                EditorUtility.SetDirty(rt);
                            }
                        };
                    };
                    
                    _watching.RemoveAt(i);
                }
                else
                {
                    entry.framesRemaining--;
                    if (entry.framesRemaining <= 0)
                    {
                        Debug.Log(LOG_PREFIX + $"Watch expired for '{entry.rectTransform.name}' (pos stayed {currentPos})");
                        _watching.RemoveAt(i);
                    }
                }
            }
        }
    }
}
