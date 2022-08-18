using UnityEditor;
using UnityEngine;

namespace Flow.EcsLite.UnityEditor.Inspectors {
    sealed class BoundsInspector : EcsComponentInspectorTyped<Bounds> {
        public override bool OnGuiTyped (string label, ref Bounds value, EcsEntityDebugView entityView) {
            var newValue = EditorGUILayout.BoundsField (label, value);
            if (newValue == value) { return false; }
            value = newValue;
            return true;
        }
    }
}