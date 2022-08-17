using Leopotam.EcsLite;
using UnityEngine;

namespace GameEntities
{
    public class GameProcessDebugSystem : IEcsRunSystem
    {
        private bool debugMoving, debugTriggers;
        
        public GameProcessDebugSystem(bool debugMoving, bool debugTriggers)
        {
            this.debugMoving = debugMoving;
            this.debugTriggers = debugTriggers;
        }
        
        public void Run(IEcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();

            if (debugMoving)
                DebugMoving(world);

            if (debugTriggers)
                DebugTriggers(world);
        }

        private void DebugMoving(EcsWorld world)
        {
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
                
                if(rotationPool.Has(entity))
                    Debug.Log("Rotation: " + rotationPool.Get(entity).rotation.ToString());
                
                Debug.Log("Direction: " + moveDirection);
                Debug.Log("Position: " + positionComponent.currentEntityPosition.ToString());
            }
        }

        private void DebugTriggers(EcsWorld world)
        {
            EcsFilter filter = world
                .Filter<DoorButtonComponent>()
                .Inc<TriggerEnterTag>()
                .End();
            
            var buttonPool = world.GetPool<DoorButtonComponent>();
            foreach (var button in filter)
            {
                ref var buttonComponent = ref buttonPool.Get(button);
                Debug.Log($"Triggered button with id {buttonComponent.buttonId}");
            }
        }
    }
}