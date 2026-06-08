// ============================================
// Bool Event Editor
// ============================================
// Custom inspector for BoolEvent.
// Adds: raise count, time since last raise, last value, Test Value toggle +
//       Test Raise button, and a live list of subscribed listeners during play.
// Students never need to read or edit this.
// ============================================

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Ludocore
{
    [CustomEditor(typeof(BoolEvent))]
    public class BoolEventEditor : UnityEditor.Editor
    {
        private bool _testValue;

        public override bool RequiresConstantRepaint() => Application.isPlaying;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var ev = (BoolEvent)target;

            //---- Diagnostics ----
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Diagnostics", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.IntField("Raise Count", ev.RaiseCount);
                string lastRaised = ev.HasBeenRaised
                    ? $"{Time.realtimeSinceStartup - ev.LastRaiseTime:F2}s ago"
                    : "(never)";
                EditorGUILayout.TextField("Last Raised", lastRaised);
                EditorGUILayout.Toggle("Last Value", ev.LastValue);
            }

            _testValue = EditorGUILayout.Toggle("Test Value", _testValue);
            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                if (GUILayout.Button("Test Raise"))
                    ev.Raise(_testValue);
            }

            //---- Listeners ----
            EditorGUILayout.Space(8);
            DrawListeners(ev.EditorListeners);
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
