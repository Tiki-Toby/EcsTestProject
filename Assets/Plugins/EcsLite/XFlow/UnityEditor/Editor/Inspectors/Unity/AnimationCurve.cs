using UnityEditor;
using UnityEngine;

namespace Flow.EcsLite.UnityEditor.Inspectors {
    sealed class AnimationCurveInspector : EcsComponentInspectorTyped<AnimationCurve> {
        public override bool OnGuiTyped (string label, ref AnimationCurve value, EcsEntityDebugView entityView) {
            var newValue = EditorGUILayout.CurveField (label, value);
            if (newValue.Equals (value)) { return false; }
            value = newValue;
            return true;
        }
    }
}