using UnityEditor;
using UnityEngine;

namespace Flow.EcsLite.UnityEditor.Inspectors {
    sealed class Vector4Inspector : EcsComponentInspectorTyped<Vector4> {
        public override bool OnGuiTyped (string label, ref Vector4 value, EcsEntityDebugView entityView) {
            var newValue = EditorGUILayout.Vector4Field (label, value);
            if (newValue == value) { return false; }
            value = newValue;
            return true;
        }
    }
}