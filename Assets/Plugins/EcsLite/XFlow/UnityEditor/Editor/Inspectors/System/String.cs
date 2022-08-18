
using UnityEditor;

namespace Flow.EcsLite.UnityEditor.Inspectors {
    sealed class StringInspector : EcsComponentInspectorTyped<string> {
        public override bool IsNullAllowed () {
            return true;
        }

        public override bool OnGuiTyped (string label, ref string value, EcsEntityDebugView entityView) {
            var newValue = EditorGUILayout.TextField (label, value);
            if (newValue == value) { return false; }
            value = newValue;
            return true;
        }
    }
}