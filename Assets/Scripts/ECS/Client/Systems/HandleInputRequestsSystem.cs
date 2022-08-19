using GameEntities;
using UnityEngine;
using XFlow.EcsLite;

namespace InputModule
{
    public class HandleInputRequestsSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld inputWorld, world;
        private EcsFilter inputEntityFilter;
        
        public void Init(EcsSystems systems)
        {
            world = systems.GetWorld();
            inputWorld = systems.GetWorld("Input");
        }
        
        public void Run(EcsSystems systems)
        {
            inputEntityFilter = inputWorld.Filter<InputEntityTag>().End();
            var inputEntityIdPool = inputWorld.GetPool<InputEntityIdComponent>();
            
            var newTargetRequestPool = inputWorld.GetPool<InputTargetRequest>();
            var moveDirectionRequestPool = inputWorld.GetPool<InputDirectionComponent>();
            
            foreach (int i in inputEntityFilter)
            {
                if (newTargetRequestPool.Has(i))
                    SetTarget(inputEntityIdPool.Get(i).entity, newTargetRequestPool.Get(i).target);
                
                if (moveDirectionRequestPool.Has(i))
                    SetMoveDirection(inputEntityIdPool.Get(i).entity, moveDirectionRequestPool.Get(i).MoveDirection);
            }
        }

        private void SetTarget(int playerId, Vector3 targetPosition)
        {
            var targetPool = world.GetPool<MoveToTargetComponent>();

            if (targetPool.Has(playerId))
            {
                Vector3 oldTarget = targetPool.Get(playerId).target;
                if ((oldTarget - targetPosition).sqrMagnitude > 0.1f)
                {
                    ref var moveToTargetComponent = ref targetPool.GetRef(playerId);
                    moveToTargetComponent.target = targetPosition;
                }
            }
            else
            {
                targetPool.Add(playerId).target = targetPosition;
            }
        }
        
        private void SetMoveDirection(int playerId, Vector3 dir)
        {
            if (dir.sqrMagnitude > 0.1f)
            {
                world.GetPool<MoveToTargetComponent>().Del(playerId);
                var pool = world.GetPool<MovingEntityComponent>();
                if (!pool.Has(playerId))
                    pool.Add(playerId).direction = dir;
                else
                    pool.GetRef(playerId).direction = dir;
            }
            else
            {
                var pool = world.GetPool<MovingEntityComponent>();
                if (pool.Has(playerId))
                    pool.Del(playerId);
            }
        }
    }
}