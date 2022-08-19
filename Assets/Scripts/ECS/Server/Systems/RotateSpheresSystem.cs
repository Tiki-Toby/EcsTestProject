
using System;
using UnityEngine;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer;
using XFlow.Modules.Box2D.ClientServer.Api;
using XFlow.Modules.Box2D.ClientServer.Components.Colliders;

namespace ECS.Server
{
    public class RotateSpheresSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();

            EcsFilter filter = world
                .Filter<PositionComponent>()
                .Inc<Box2DCircleColliderComponent>()
                .Inc<RotationComponent>()
                .End();

            var rotationPool = world.GetPool<RotationComponent>();
            var circlePool = world.GetPool<Box2DCircleColliderComponent>();

            foreach (int sphere in filter)
            {
                IntPtr body = world.GetBodyRefFromEntity(sphere);
                
                if(body == IntPtr.Zero)
                    body = Box2DServices.CreateBodyNow(world, sphere);
                
                var bodyInfo = Box2DApi.GetBodyInfo(body);
                float radius = circlePool.GetRef(sphere).Radius;
                
                Vector3 axis = new Vector3(bodyInfo.LinearVelocity.y, 0, -bodyInfo.LinearVelocity.x);
                float angel = 2 * Mathf.Asin(bodyInfo.LinearVelocity.magnitude * Time.deltaTime / radius) * Mathf.Rad2Deg;
                
                ref var rotationComponent = ref rotationPool.GetRef(sphere);
                
                Quaternion rotation = Quaternion.AngleAxis(angel, axis);
                rotationComponent.rotation = rotation * rotationComponent.rotation;
            }
        }
    }
}