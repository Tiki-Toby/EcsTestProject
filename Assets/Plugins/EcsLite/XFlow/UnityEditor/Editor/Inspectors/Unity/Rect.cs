using UnityEditor;
using UnityEngine;

namespace Flow.EcsLite.UnityEditor.Inspectors {
    sealed class RectInspector : EcsComponentInspectorTyped<Rect> {
        public override bool OnGuiTyped (string label, ref Rect value, EcsEntityDebugView entityView) {
            var newValue = EditorGUILayout.RectField (label, value);
            if (newValue == value) { return false; }
            value = newValue;
            return true;
        }
    }
}