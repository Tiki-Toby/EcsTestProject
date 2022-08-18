using UnityEditor;

namespace Flow.EcsLite.UnityEditor.Inspectors {
    sealed class DoubleInspector : EcsComponentInspectorTyped<double> {
        public override bool OnGuiTyped (string label, ref double value, EcsEntityDebugView entityView) {
            var newValue = EditorGUILayout.DoubleField (label, value);
            if (System.Math.Abs (newValue - value) < double.Epsilon) { return false; }
            value = newValue;
            return true;
        }
    }
}