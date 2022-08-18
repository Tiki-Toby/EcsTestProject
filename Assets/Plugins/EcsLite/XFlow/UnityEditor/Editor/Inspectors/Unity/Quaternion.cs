using UnityEditor;
using UnityEngine;

namespace Flow.EcsLite.UnityEditor.Inspectors {
    sealed class QuaternionInspector : EcsComponentInspectorTyped<Quaternion> {
        public override bool OnGuiTyped (string label, ref Quaternion value, EcsEntityDebugView entityView) {
            var eulerAngles = value.eulerAngles;
            var newValue = EditorGUILayout.Vector3Field (label, eulerAngles);
            if (newValue == eulerAngles) { return false; }
            value = Quaternion.Euler (newValue);
            return true;
        }
    }
}