using UnityEditor;
using UnityEngine;

namespace Flow.EcsLite.UnityEditor.Inspectors {
    sealed class ColorInspector : EcsComponentInspectorTyped<Color> {
        public override bool OnGuiTyped (string label, ref Color value, EcsEntityDebugView entityView) {
            var newValue = EditorGUILayout.ColorField (label, value);
            if (newValue == value) { return false; }
            value = newValue;
            return true;
        }
    }
}