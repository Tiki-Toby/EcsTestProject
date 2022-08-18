using UnityEditor;

namespace Flow.EcsLite.UnityEditor.Inspectors {
    sealed class LongInspector : EcsComponentInspectorTyped<long> {
        public override bool OnGuiTyped (string label, ref long value, EcsEntityDebugView entityView) {
            var newValue = EditorGUILayout.LongField (label, value);
            if (newValue == value) { return false; }
            value = newValue;
            return true;
        }
    }
}