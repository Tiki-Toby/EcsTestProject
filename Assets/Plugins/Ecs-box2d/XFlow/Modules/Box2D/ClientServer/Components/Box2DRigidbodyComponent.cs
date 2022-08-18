using UnityEngine;
using XFlow.Modules.Box2D.ClientServer.Api;

namespace XFlow.Modules.Box2D.ClientServer.Components
{
    [System.Serializable]
    public struct Box2DRigidbodyComponent
    {
        public Vector2 LinearVelocity;
        public float AngularVelocity;
        public BodyType BodyType;
    }
}
