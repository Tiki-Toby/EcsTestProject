using Leopotam.EcsLite;
using UnityEngine;

namespace GameEntities
{
    public class GameProcessDebugSystem : IEcsRunSystem
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
                
                //if(rotationPool.Has(entity))
                //    Debug.Log("Rotation: " + rotationPool.Get(entity).rotation.ToString());
                //
                //Debug.Log("Direction: " + moveDirection);
                //Debug.Log("Position: " + positionComponent.currentEntityPosition.ToString());
            }

            var buttonPool = world.GetPool<DoorButtonComponent>();

            filter = world.Filter<DoorButtonComponent>().Inc<TriggerEnterTag>().End();
            foreach (var button in filter)
            {
                ref var buttonComponent = ref buttonPool.Get(button);
                Debug.Log($"Triggered button with id {buttonComponent.buttonId}");
            }
        }
    }
}