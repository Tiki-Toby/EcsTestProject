using UnityEditor;
using UnityEngine;

namespace Flow.EcsLite.UnityEditor.Inspectors {
    sealed class Vector3Inspector : EcsComponentInspectorTyped<Vector3> {
        public override bool OnGuiTyped (string label, ref Vector3 value, EcsEntityDebugView entityView) {
            var newValue = EditorGUILayout.Vector3Field (label, value);
            if (newValue == value) { return false; }
            value = newValue;
            return true;
        }
    }
}