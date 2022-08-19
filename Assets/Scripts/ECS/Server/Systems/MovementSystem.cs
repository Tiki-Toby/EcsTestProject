using UnityEngine;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer.Components;

namespace ECS.Server
{
    internal class MovementSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();
            EcsFilter filter = world
                .Filter<MovingEntityComponent>()
                .Inc<PositionComponent>()
                .Exc<Box2DRigidbodyDefinitionComponent>()
                .End();
            
            var positionPool = world.GetPool<PositionComponent>();
            var moveDirectionPool = world.GetPool<MovingEntityComponent>();
            var velocityPool = world.GetPool<VelocityComponent>();
            var rotationPool = world.GetPool<RotationComponent>();
            var radiusPool = world.GetPool<RadiusComponent>();

            foreach (var entity in filter)
            {
                Vector3 moveDirection = moveDirectionPool.Get(entity).direction;
                float speed = velocityPool.Get(entity).velocity;
                Vector3 dir = moveDirection * Time.deltaTime * speed;
                
                ref var positionComponent = ref positionPool.GetRef(entity);
                positionComponent.value += dir;

                if (rotationPool.Has(entity))
                {
                    ref var rotationComponent = ref rotationPool.GetRef(entity);
                    float radius = radiusPool.GetRef(entity).radius;
                    
                    Vector3 axis = new Vector3(dir.z, 0, -dir.x);
                    float angel = Mathf.Atan(dir.magnitude / radius) * Mathf.Rad2Deg;
                    Quaternion rotation = Quaternion.AngleAxis(angel, axis);
                    
                    rotationComponent.rotation = rotation * rotationComponent.rotation;
                }
            }
        }
    }
}