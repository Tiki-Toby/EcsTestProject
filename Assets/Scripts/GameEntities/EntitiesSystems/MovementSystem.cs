using Leopotam.EcsLite;
using UnityEngine;

namespace GameEntities
{
    internal class MovementSystem : IEcsRunSystem
    {
        public void Run(IEcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();
            EcsFilter filter = world
                .Filter<MovingEntityComponent>()
                .Inc<PositionComponent>()
                .End();
            
            var positionPool = world.GetPool<PositionComponent>();
            var moveDirectionPool = world.GetPool<MovingEntityComponent>();
            var velocityPool = world.GetPool<VelocityComponent>();
            var rotationPool = world.GetPool<RotationComponent>();

            foreach (var entity in filter)
            {
                Vector3 moveDirection = moveDirectionPool.Get(entity).direction;
                float speed = velocityPool.Get(entity).velocity;
                Vector3 dir = moveDirection * Time.deltaTime * speed;
                
                ref var positionComponent = ref positionPool.Get(entity);
                positionComponent.currentEntityPosition += dir;

                if (rotationPool.Has(entity))
                {
                    Vector3 axis = new Vector3(dir.z, 0, -dir.x);
                    Quaternion rotation = Quaternion.AngleAxis(1, axis);
                    
                    ref var rotationComponent = ref rotationPool.Get(entity);
                    rotationComponent.rotation *= rotation;
                }
            }
        }
    }
}