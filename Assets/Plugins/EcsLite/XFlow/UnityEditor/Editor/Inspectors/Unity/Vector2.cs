using UnityEditor;
using UnityEngine;

namespace Flow.EcsLite.UnityEditor.Inspectors {
    sealed class Vector2Inspector : EcsComponentInspectorTyped<Vector2> {
        public override bool OnGuiTyped (string label, ref Vector2 value, EcsEntityDebugView entityView) {
            var newValue = EditorGUILayout.Vector2Field (label, value);
            if (newValue == value) { return false; }
            value = newValue;
            return true;
        }
    }
}