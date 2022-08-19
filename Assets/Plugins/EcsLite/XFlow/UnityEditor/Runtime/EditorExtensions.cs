#if UNITY_EDITOR
using System;
using UnityEngine;
using XFlow.EcsLite;

namespace Flow.EcsLite.UnityEditor {
    public static class EditorExtensions {
        public static string GetCleanGenericTypeName (Type type) {
            if (!type.IsGenericType) {
                return type.Name;
            }
            var constraints = "";
            foreach (var constraint in type.GetGenericArguments ()) {
                constraints += constraints.Length > 0 ? $", {GetCleanGenericTypeName (constraint)}" : constraint.Name;
            }
            var genericIndex = type.Name.LastIndexOf ("`", StringComparison.Ordinal);
            var typeName = genericIndex == -1
                ? type.Name
                : type.Name.Substring (0, genericIndex);
            return $"{typeName}<{constraints}>";
        }
    }

    public sealed class EcsEntityDebugView : MonoBehaviour {
        [NonSerialized]
        public EcsWorld World;
        [NonSerialized]
        public int Entity;
        [NonSerialized]
        public EcsWorldDebugSystem DebugSystem;
    }
}
#endif