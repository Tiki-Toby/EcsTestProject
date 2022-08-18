using UnityEditor;
using UnityEngine;

namespace Flow.EcsLite.UnityEditor.Inspectors {
    sealed class Vector3IntInspector : EcsComponentInspectorTyped<Vector3Int> {
        public override bool OnGuiTyped (string label, ref Vector3Int value, EcsEntityDebugView entityView) {
            var newValue = EditorGUILayout.Vector3IntField (label, value);
            if (newValue == value) { return false; }
            value = newValue;
            return true;
        }
    }
}