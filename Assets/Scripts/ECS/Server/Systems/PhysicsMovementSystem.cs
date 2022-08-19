using System;
using UnityEngine;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer;
using XFlow.Modules.Box2D.ClientServer.Api;
using XFlow.Modules.Box2D.ClientServer.Components;
using XFlow.Modules.Box2D.ClientServer.Components.Colliders;
using XFlow.Utils;

namespace ECS.Server
{
    public class PhysicsMovementSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();
            EcsFilter filter = world
                .Filter<MovingEntityComponent>()
                .Inc<Box2DRigidbodyDefinitionComponent>()
                .Inc<PositionComponent>()
                .End();
            
            var positionPool = world.GetPool<PositionComponent>();
            var moveDirectionPool = world.GetPool<MovingEntityComponent>();
            var velocityPool = world.GetPool<VelocityComponent>();
            var rotationPool = world.GetPool<RotationComponent>();
            var circlePool = world.GetPool<Box2DCircleColliderComponent>();

            foreach (var entity in filter)
            {
                Vector3 moveDirection = moveDirectionPool.Get(entity).direction;
                float speed = velocityPool.Get(entity).velocity;

                IntPtr body = world.GetBodyRefFromEntity(entity);
                
                if(body == IntPtr.Zero)
                    body = Box2DServices.CreateBodyNow(world, entity);
                
                ref Vector3 position = ref positionPool.GetRef(entity).value;
                var force = moveDirection.ToVector2XZ() * speed;
                Box2DApi.ApplyForce(body, force, position.ToVector2XZ());
                
                var bodyInfo = Box2DApi.GetBodyInfo(body);

                if (rotationPool.Has(entity))
                {
                    //Box2DApi.ApplyTorque(body, 1 , true);
                    //float radius = circlePool.GetRef(entity).Radius;
                    //float angel = 2 * Mathf.Asin(speed * Time.deltaTime / radius) * Mathf.Rad2Deg;
                    
                    //ref var rotationComponent = ref rotationPool.GetRef(entity);
                    
                    //Vector3 axis = new Vector3(moveDirection.z, 0, -moveDirection.x);
                    //Quaternion rotation = Quaternion.AngleAxis(angel, axis);
                    
                    //rotationComponent.rotation = rotation * rotationComponent.rotation;
                }
            }
        }
    }
}