// ============================================
// Float Variable Editor
// ============================================
// Custom inspector for FloatVariable.
// Adds a live list of subscribed listeners during play.
// Students never need to read or edit this.
// ============================================

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Ludocore
{
    [CustomEditor(typeof(FloatVariable))]
    public class FloatVariableEditor : UnityEditor.Editor
    {
        public override bool RequiresConstantRepaint() => Application.isPlaying;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space(8);
            DrawListeners(((FloatVariable)target).EditorListeners);
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
