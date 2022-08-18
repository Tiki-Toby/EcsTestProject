using UnityEditor;
using UnityEngine;

namespace Flow.EcsLite.UnityEditor.Inspectors {
    sealed class LayerMaskInspector : EcsComponentInspectorTyped<LayerMask> {
        public override bool OnGuiTyped (string label, ref LayerMask value, EcsEntityDebugView entityView) {
            var newValue = EditorGUILayout.LayerField (label, value);
            if (newValue == value) { return false; }
            value = newValue;
            return true;
        }
    }
}