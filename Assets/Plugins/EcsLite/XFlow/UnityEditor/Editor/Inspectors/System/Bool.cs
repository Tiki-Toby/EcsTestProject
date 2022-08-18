using UnityEditor;

namespace Flow.EcsLite.UnityEditor.Inspectors {
    sealed class BoolInspector : EcsComponentInspectorTyped<bool> {
        public override bool OnGuiTyped (string label, ref bool value, EcsEntityDebugView entityView) {
            var newValue = EditorGUILayout.Toggle (label, value);
            if (newValue == value) { return false; }
            value = newValue;
            return true;
        }
    }
}