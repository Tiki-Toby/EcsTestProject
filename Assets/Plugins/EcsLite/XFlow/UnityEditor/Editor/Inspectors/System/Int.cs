using UnityEditor;

namespace Flow.EcsLite.UnityEditor.Inspectors {
    sealed class IntInspector : EcsComponentInspectorTyped<int> {
        public override bool OnGuiTyped (string label, ref int value, EcsEntityDebugView entityView) {
            var newValue = EditorGUILayout.IntField (label, value);
            if (newValue == value) { return false; }
            value = newValue;
            return true;
        }
    }
}