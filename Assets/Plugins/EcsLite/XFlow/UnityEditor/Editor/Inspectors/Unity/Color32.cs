using UnityEditor;
using UnityEngine;

namespace Flow.EcsLite.UnityEditor.Inspectors {
    sealed class Color32Inspector : EcsComponentInspectorTyped<Color32> {
        public override bool OnGuiTyped (string label, ref Color32 value, EcsEntityDebugView entityView) {
            var newValue = EditorGUILayout.ColorField (label, value);
            if (newValue == value) { return false; }
            value = newValue;
            return true;
        }
    }
}