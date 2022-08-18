using UnityEditor;
using UnityEngine;

namespace Flow.EcsLite.UnityEditor.Inspectors {
    sealed class Vector2IntInspector : EcsComponentInspectorTyped<Vector2Int> {
        public override bool OnGuiTyped (string label, ref Vector2Int value, EcsEntityDebugView entityView) {
            var newValue = EditorGUILayout.Vector2IntField (label, value);
            if (newValue == value) { return false; }
            value = newValue;
            return true;
        }
    }
}