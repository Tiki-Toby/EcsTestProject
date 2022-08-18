using UnityEngine;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer.Api;

namespace XFlow.Modules.Box2D.ClientServer.Components.Other
{
    public struct Box2DContactComponentData
    {
        public EcsPackedEntity EntityA;
        public EcsPackedEntity EntityB;
        public Vector2 VelA;
        public Vector2 VelB;
        public Vector2 Normal;
        public Vector2 ContactPoint1;
        public Vector2 ContactPoint2;
        public int ContactPointCount;

        public void CopyFrom(EcsWorld world, CollisionCallbackData data)
        {
            EntityA = world.PackEntity(data.EntityA);
            EntityB = world.PackEntity(data.EntityB);
            VelA = data.VelA;
            VelB = data.VelB;
            Normal = data.Normal;
            
            ContactPointCount = data.ContactPointCount;
            if (ContactPointCount > 0)
            {
                ContactPoint1 = data.ContactPoints[0];
                if (ContactPointCount > 1)
                    ContactPoint2 = data.ContactPoints[1];
            }
        }
    }
}