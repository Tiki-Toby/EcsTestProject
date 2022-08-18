using UnityEngine;
using XFlow.Ecs.ClientServer.WorldDiff.Attributes;

namespace XFlow.Modules.Box2D.ClientServer.Components.Colliders
{
    [ForceJsonSerialize]
    [System.Serializable]
    public struct Box2DPolygonColliderComponent
    {
        public int[] Anchors;
        //public List<Vector2> Vertices;//todo, refactor to array[]
        public Vector2[] Vertices;
    }
}