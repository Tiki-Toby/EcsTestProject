using UnityEngine;
using XFlow.EcsLite;

namespace GameEntities
{
    public class MoveToTargetSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();
            EcsFilter filter = world
                .Filter<MoveToTargetComponent>()
                .Inc<PositionComponent>()
                .End();

            var targetPool = world.GetPool<MoveToTargetComponent>();
            var positionPool = world.GetPool<PositionComponent>();
            var movingPool = world.GetPool<MovingEntityComponent>();

            foreach (int i in filter)
            {
                Vector3 deltaPosition = targetPool.Get(i).target - positionPool.Get(i).currentEntityPosition;
                deltaPosition.y = 0;

                if (deltaPosition.sqrMagnitude <= 0.1f)
                {
                    if(movingPool.Has(i))
                        movingPool.Del(i);
                    targetPool.Del(i);
                    
                    continue;
                }
                
                Vector3 moveDirection = deltaPosition.normalized;

                if (movingPool.Has(i))
                    movingPool.GetRef(i).direction = moveDirection;
                else
                    movingPool.Add(i).direction = moveDirection;
            }
        }
    }
}