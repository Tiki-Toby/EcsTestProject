using UnityEngine;
using XFlow.EcsLite;

namespace GameEntities
{
    internal class MovementSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
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
                
                ref var positionComponent = ref positionPool.GetRef(entity);
                positionComponent.currentEntityPosition += dir;

                if (rotationPool.Has(entity))
                {
                    ref var rotationComponent = ref rotationPool.GetRef(entity);
                    
                    //Vector3 axis = Vector3.Cross(moveDirection.normalized, Vector3.up);
                    Vector3 axis = new Vector3(dir.z, 0, -dir.x);
                    float angel = 1;//dir.magnitude * Mathf.PI;
                    Quaternion rotation = Quaternion.AngleAxis(angel, axis);
                    
                    rotationComponent.rotation = rotation * rotationComponent.rotation;
                }
            }
        }
    }
}