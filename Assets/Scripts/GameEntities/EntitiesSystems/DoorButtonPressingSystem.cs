using Leopotam.EcsLite;
using UnityEngine;

namespace GameEntities
{
    public class DoorButtonPressingSystem : IEcsRunSystem
    {
        public void Run(IEcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();

            EcsFilter filter = world
                .Filter<DoorButtonComponent>()
                .Inc<TriggerEnterTag>()
                .End();

            var doorPool = world.GetPool<DoorComponent>();
            var buttonPool = world.GetPool<DoorButtonComponent>();
            
            var positionPool = world.GetPool<PositionComponent>();
            var movingPool = world.GetPool<MovingEntityComponent>();

            var triggerEnterPool = world.GetPool<TriggerEnterTag>();
            var triggerExitPool = world.GetPool<TriggerExitTag>();

            foreach (int i in filter)
            {
                int doorEntity = buttonPool.Get(i).doorEntity;
                ref var doorComponent = ref doorPool.Get(doorEntity);

                if (triggerExitPool.Has(i))
                {
                    triggerEnterPool.Del(i);
                    triggerExitPool.Del(i);
                    
                    if (movingPool.Has(doorEntity))
                        movingPool.Del(doorEntity);
                    
                    continue;
                }
                
                Vector3 position = positionPool.Get(doorEntity).currentEntityPosition;
                if ((position - doorComponent.finalPosition).sqrMagnitude > 0.1f)
                {
                    if (!movingPool.Has(doorEntity))
                        movingPool.Add(doorEntity).direction = doorComponent.moveDirecition;
                    else
                        movingPool.Get(doorEntity).direction = doorComponent.moveDirecition;
                }
                else if (movingPool.Has(doorEntity))
                        movingPool.Del(doorEntity);
            }
        }
    }
}