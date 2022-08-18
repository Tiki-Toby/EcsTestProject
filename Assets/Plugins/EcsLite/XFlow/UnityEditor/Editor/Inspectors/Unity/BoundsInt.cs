using UnityEditor;
using UnityEngine;

namespace Flow.EcsLite.UnityEditor.Inspectors {
    sealed class BoundsIntInspector : EcsComponentInspectorTyped<BoundsInt> {
        public override bool OnGuiTyped (string label, ref BoundsInt value, EcsEntityDebugView entityView) {
            var newValue = EditorGUILayout.BoundsIntField (label, value);
            if (newValue == value) { return false; }
            value = newValue;
            return true;
        }
    }
}