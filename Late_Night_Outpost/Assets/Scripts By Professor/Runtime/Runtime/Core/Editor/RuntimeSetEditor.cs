// ============================================
// Runtime Set Editor
// ============================================
// Custom inspector for any RuntimeSet<T> subclass.
// Adds: live list of registered items, count, and listeners panel.
// Students never need to read or edit this.
// ============================================

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Ludocore
{
    [CustomEditor(typeof(RuntimeSetBase), editorForChildClasses: true)]
    public class RuntimeSetEditor : UnityEditor.Editor
    {
        public override bool RequiresConstantRepaint() => Application.isPlaying;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var set = (RuntimeSetBase)target;

            //---- Registered Items ----
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField($"Registered Items ({set.Count})", EditorStyles.boldLabel);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    "Items register at runtime. Press Play to see what's in the set.",
                    MessageType.Info);
            }
            else if (set.Count == 0)
            {
                EditorGUILayout.HelpBox("Set is empty.", MessageType.None);
            }
            else
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    foreach (var item in set.ItemsAsObjects)
                        EditorGUILayout.ObjectField(item, typeof(Object), true);
                }
            }

            using (new EditorGUI.DisabledScope(!Application.isPlaying || set.Count == 0))
            {
                if (GUILayout.Button("Clear All"))
                    set.Clear();
            }

            //---- Listeners ----
            EditorGUILayout.Space(8);
            DrawListeners(set.EditorListeners);
        }

        private static void DrawListeners(IReadOnlyList<Object> listeners)
        {
            EditorGUILayout.LabelField("Listeners", EditorStyles.boldLabel);
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    "Listeners register at runtime. Press Play to see who is subscribed.",
                    MessageType.Info);
                return;
            }
            if (listeners == null || listeners.Count == 0)
            {
                EditorGUILayout.HelpBox("No listeners currently subscribed.", MessageType.None);
                return;
            }
            using (new EditorGUI.DisabledScope(true))
            {
                foreach (var l in listeners)
                    EditorGUILayout.ObjectField(l, typeof(Object), true);
            }
        }
    }
}
