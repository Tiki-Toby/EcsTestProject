using System;
using UnityEngine;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer;
using XFlow.Modules.Box2D.ClientServer.Api;
using XFlow.Modules.Box2D.ClientServer.Components;
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

            foreach (var entity in filter)
            {
                Vector3 moveDirection = moveDirectionPool.Get(entity).direction;
                float speed = velocityPool.Get(entity).velocity;

                IntPtr body = world.GetBodyRefFromEntity(entity);
                
                ref Vector3 position = ref positionPool.GetRef(entity).value;
                var force = moveDirection.ToVector2XZ() * speed;
                Box2DApi.ApplyForce(body, force, position.ToVector2XZ());
            }
        }
    }
}